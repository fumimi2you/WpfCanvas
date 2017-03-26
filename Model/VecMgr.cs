using System;
using System.Linq;
using System.Collections.Generic;


namespace NDimVecManater
{
    /// <summary>
    /// ベクトル情報
    /// </summary>
    public class ValObj
    {
        public string Name { get; set; } = "";
        public double Val { get; set; } = 0;
    }

    /// <summary>
    /// 1単語のベクトル情報
    /// </summary>
    public class WordObj
    {
        public string Name { get; set; } = "";

        public List<float> VctOrg { get; set; } = new List<float>();
        public double Mag { get; set; } = 0;

        public List<float> VctNml { get; set; } = new List<float>();
    }

    public class VecMgr
    {
        const char SPLITTER = ' ';

        int VDimCnt { get; set; } = 0;
        double MagAve = 0;

        public List<WordObj> Words { get; set; }

        List<ValObj> MaxObjNames;
        List<ValObj> MinObjNames;

        public bool ReadData(string filePath)
        {
            try
            {
                // csvファイルを開く
                using (var sr = new System.IO.StreamReader(filePath))
                {
                    Words = new List<WordObj>();

                    double magSum = 0;

                    // ストリームの末尾まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        // ファイルから一行読み込みスプリッタに分けて配列に格納する
                        var line = sr.ReadLine();
                        var values = new List<string>( line.Split(SPLITTER) );

                        //  最初の1行はデータサイズ
                        if (VDimCnt == 0)
                        {
                            VDimCnt = Convert.ToInt32(values[1]);
                        }
                        else
                        {
                            //  ワード情報読み込み
                            var Word = new WordObj();

                            //  最初は名前
                            Word.Name = values[0];
                            values.RemoveAt(0);
                                
                            //  残りは値
                            for (int v = 0; v < VDimCnt; v++)
                            {
                                var val = Convert.ToDouble(values[v]);
                                Word.VctOrg.Add( (float)val );
                                Word.Mag += val * val;
                            }

                            //  正規化する。
                            Word.Mag = Math.Sqrt(Word.Mag);
                            for (int v = 0; v < VDimCnt; v++)
                            {
                                Word.VctNml.Add((float)(Word.VctOrg[v]/ Word.Mag));
                            }

                            //  辞書に追加
                            Words.Add(Word);

                            //  平均値算出用
                            magSum += Word.Mag;
                        }
                    }

                    MagAve = magSum / Words.Count;
                }

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }


        public void RefineWords()
        {
            //  単語の絞込
            var ReWords =
                from word in Words
                where ( word.Mag < MagAve * 2 && word.Mag > MagAve * 0.5  )
                select word;
            Words = new List<WordObj>(ReWords);

            //  []の排除
            foreach (var word in Words)
            {
                word.Name = word.Name.Replace("[", "");
                word.Name = word.Name.Replace("]", "");
                word.Name = word.Name.Replace("#", "");
            }

            Words.Sort((x, y) => x.Name.CompareTo(y.Name));
        }


        public bool CalcMinMaxVector()
        {
            MaxObjNames = new List<ValObj>();
            MinObjNames = new List<ValObj>();
            for (int v = 0; v < VDimCnt; v++)
            {
                MaxObjNames.Add(new ValObj());
                MinObjNames.Add(new ValObj());
            }


            foreach( var word in Words )
            {
                for (int v = 0; v < VDimCnt; v++)
                {
                    var valVct = word.VctNml[v];
                    if (valVct > MaxObjNames[v].Val)
                    {
                        MaxObjNames[v].Name = word.Name;
                        MaxObjNames[v].Val = valVct;
                    }
                    if (valVct < MinObjNames[v].Val)
                    {
                        MinObjNames[v].Name = word.Name;
                        MinObjNames[v].Val = valVct;
                    }
                }
            }

            return true;
        }

        public void SaveData(string filePath)
        {
            //  CSVファイルに書き込むときに使うEncoding
            System.Text.Encoding enc =
                System.Text.Encoding.GetEncoding("UTF-8");

            //  書き込むファイルを開く
            System.IO.StreamWriter sr =
                new System.IO.StreamWriter(filePath, false, enc);

            //  ヘッダーを書き込む
            sr.Write(Words.Count);
            sr.Write(" ");
            sr.Write(VDimCnt);
            sr.Write("\r\n");

            //レコードを書き込む
            foreach (var word in Words)
            {
                sr.Write(word.Name);
                sr.Write(" ");

                foreach ( var vct in word.VctNml )
                {
                    sr.Write(vct.ToString());
                    sr.Write(' ');
                }
                //改行する
                sr.Write("\r\n");
            }

            //閉じる
            sr.Close();
        }

        public void SaveMinMax(string filePath)
        {
            //CSVファイルに書き込むときに使うEncoding
            System.Text.Encoding enc =
                System.Text.Encoding.GetEncoding("UTF-8");

            //書き込むファイルを開く
            System.IO.StreamWriter sr =
                new System.IO.StreamWriter(filePath, false, enc);

            sr.Write("Idx, Min語, Max語, MinVal, MaxVal \r\n");

            for (int v = 0; v < VDimCnt; v++)
            {
                sr.Write(v);
                sr.Write(", ");

                sr.Write(MinObjNames[v].Name);
                sr.Write(", ");

                sr.Write(MaxObjNames[v].Name);
                sr.Write(", ");

                sr.Write(MinObjNames[v].Val);
                sr.Write(", ");

                sr.Write(MaxObjNames[v].Val);
                sr.Write("\r\n");
            }


            //閉じる
            sr.Close();
        }
    }
}
