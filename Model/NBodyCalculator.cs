using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfCanvas
{
    //  座標を扱うクラス
    public class PT
    {
        public PT(double ix, double iy) { x = ix; y = iy; }
        public double x { get; set; } = 0;
        public double y { get; set; } = 0;

        public static PT Delta(PT ptMain, PT ptSub)
        {
            PT dPT = new PT((ptSub.x - ptMain.x), (ptSub.y - ptMain.y));
            return dPT;
        }
    }


    //  二点間の距離を扱うクラス
    public class DPT
    {
        public DPT(PT ptMain, PT ptSub) {
            dx = (ptSub.x - ptMain.x);
            dy = (ptSub.y - ptMain.y);

            Dist2 = dx * dx + dy * dy;
            Dist = Math.Sqrt(Dist2);
        }
        public double dx { get; set; } = 0;
        public double dy { get; set; } = 0;
        public double Dist2 { get; set; } = 0;
        public double Dist { get; set; } = 0;
    }

    //  移動ベクトルを扱うクラス
    public class VCT
    {
        public VCT() { }
        public VCT(DPT dPT)
        {
            //  XY成分の計算
            double ux = dPT.dx / dPT.Dist;
            double uy = dPT.dy / dPT.Dist;

            //  引力の計算
            double df = 1 / dPT.Dist2;
            dfx = df * ux;
            dfy = df * uy;
        }
        public double dfx { get; set; } = 0;
        public double dfy { get; set; } = 0;

        public double df()
        {
            return Math.Sqrt(dfx * dfx + dfy * dfy);
        }
    }

    //  N体の集約と拡散を取り扱うクラス
    public class NBodyCalculator
    {
        const int Resolution = 100;     //  分解能≒精度
        const double AggregationGain = 0.1; //  集約の強さ
        const double DiffusionGain = 0.02; //  拡散の強さ
        const double Fluctuations = 0.02;  //  ゆらぎの大きさ

        Random RandSeed = new Random();

        public List<PT> Objects { get; set; } = new List<PT>();
        int ObjSize = 0;
        Rect ValidRect;
        double AreaExtent = 0;

        public NBodyCalculator(List<PT> obj, int objSize, Rect rct)
        {
            Objects = new List<PT>(obj);
            ObjSize = objSize;
            ValidRect = rct;
            AreaExtent = (rct.Width + rct.Height) / 2;
        }


        //  集約計算
        public void Aggregation()
        {
            for (int i = 0; i < Resolution; i++)
            {
                AggregationCore();
            }
        }
        public void AggregationCore()
        {
            //  引力の計算
            double absMax = 0;
            List<VCT> vectors = new List<VCT>();
            foreach (var pt1 in Objects)
            {
                VCT vctP1 = new VCT();

                //  引力計算
                CalcAggregationVector( pt1, vctP1, Objects);

                //  配列格納
                vectors.Add(vctP1);
                absMax = Math.Max(absMax, vctP1.df());
            }

            //  座標移動
            Move(vectors, AggregationGain * AreaExtent / absMax);
        }
        private void CalcAggregationVector( PT pt1, VCT vctRet, List<PT> objects)
        {
            foreach (var pt2 in objects)
            {
                if (pt1 != pt2)
                {
                    //  ベクトル計算
                    DPT dPT = new DPT(pt1, pt2);
                    VCT vct = new VCT(dPT);
                    if (dPT.Dist > ObjSize)
                    {
                        vctRet.dfx += vct.dfx;
                        vctRet.dfy += vct.dfy;
                    }
                    else
                    {
                        vctRet.dfx -= vct.dfx;
                        vctRet.dfy -= vct.dfy;
                    }
                }
            }
        }


        //  拡散計算
        public void Diffusion()
        {
            for (int i = 0; i < Resolution; i++)
            {
                DiffusionCore();
            }
        }
        public void DiffusionCore()
        {
            //  斥力の計算
            double absMax = 0;
            List<VCT> vectors = new List<VCT>();
            foreach (var pt1 in Objects)
            {
                VCT vctP1 = new VCT();

                //  壁からの斥力を計算
                List<PT> ptEdges = new List<PT>();
                ptEdges.Add(new PT(ValidRect.Left,  pt1.y           ));
                ptEdges.Add(new PT(pt1.x,           ValidRect.Top   ));
                ptEdges.Add(new PT(ValidRect.Right, pt1.y           ));
                ptEdges.Add(new PT(pt1.x,           ValidRect.Bottom));
                CalcDiffusionVector(pt1, vctP1, ptEdges);

                //  オブジェクトの斥力
                CalcDiffusionVector(pt1, vctP1, Objects);

                //  配列格納
                vectors.Add(vctP1);
                absMax = Math.Max(absMax, vctP1.df());
            }

            //  座標移動
            Move(vectors, DiffusionGain * AreaExtent / absMax);
        }
        private void CalcDiffusionVector(PT pt1, VCT vctRet, List<PT> objects)
        {
            foreach (var pt2 in objects)
            {
                if (pt1 != pt2)
                {
                    //  ベクトル計算
                    DPT dPT = new DPT(pt1, pt2);
                    VCT vct = new VCT(dPT);
                    vctRet.dfx -= vct.dfx;
                    vctRet.dfy -= vct.dfy;
                }
            }
        }



        //  座標移動
        void Move(List<VCT> vectors, double Normalize)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                double dx = Normalize * vectors[i].dfx;
                double dy = Normalize * vectors[i].dfy;
                Objects[i].x += dx / Resolution;
                Objects[i].y += dy / Resolution;

                //  ゆらぎの取り入れ
                Objects[i].x += Fluctuations * ObjSize * 2 * (RandSeed.NextDouble() - 0.5);
                Objects[i].y += Fluctuations * ObjSize * 2 * (RandSeed.NextDouble() - 0.5);

                //  壁でバウンド
                if (Objects[i].x < ValidRect.Left)   { Objects[i].x += 2 * (ValidRect.Left - Objects[i].x); }
                if (Objects[i].y < ValidRect.Top)    { Objects[i].y += 2 * (ValidRect.Top  - Objects[i].y); }
                if (Objects[i].x > ValidRect.Right)  { Objects[i].x -= 2 * (Objects[i].x - ValidRect.Right); }
                if (Objects[i].y > ValidRect.Bottom) { Objects[i].y -= 2 * (Objects[i].y - ValidRect.Bottom); }
            }
        }
    }
}
