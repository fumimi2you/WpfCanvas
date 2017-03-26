using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NDimVecManater;

namespace WpfCanvas
{
    public class TagText
    {
        public const int ObjSize = 40; // 付箋のサイズ
        const double FontSize = 20; // フォントのサイズ
        Random RandSeed = new Random();

        public Rectangle RectCtrl { get; set; }
        public ContentControl TextCtrl { get; set; }

        public string Text { get; set; }
        public PT Pt { get; set; }
        public Color Col { get; set; }

        public TagText( string text, PT pt )
        {
            Text = text;
            Pt = pt;

            byte R = (byte)RandSeed.Next(0xFF);
            byte G = (byte)RandSeed.Next(0xFF);
            byte B = (byte)RandSeed.Next(0xFF);
            Col = Color.FromArgb(0x80, R, G, B);

            CreateRect();
            CreateText();
        }

        public void SetPoint(PT pt)
        {
            Pt = pt;
            Canvas.SetLeft(RectCtrl, Pt.x);
            Canvas.SetTop(RectCtrl, Pt.y);
            Canvas.SetLeft(TextCtrl, Pt.x);
            Canvas.SetTop(TextCtrl, Pt.y);
        }


        private void CreateRect()
        {
            RectCtrl = new Rectangle
            {
                Height = ObjSize,
                Width = ObjSize * 3,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Col),
            };
            Canvas.SetLeft(RectCtrl, Pt.x);
            Canvas.SetTop(RectCtrl, Pt.y);
        }

        private void CreateText()
        {
            TextCtrl = new ContentControl();
            Canvas.SetLeft(TextCtrl, Pt.x);
            Canvas.SetTop(TextCtrl, Pt.y);

            TextCtrl.Height = ObjSize;
            TextCtrl.Width = ObjSize*3;

            TextBlock tb = new TextBlock();
            tb.Text = Text;
            tb.FontSize = FontSize;
            tb.Foreground = Brushes.Black;
            TextCtrl.Content = tb;
        }
    }



    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        VecMgr VM;

        const int RectMaxW = 1200; // 領域サイズ
        const int RectMaxH =  600; // 領域サイズ
        Rect ValidRect;


        Random RandSeed = new Random();
        List<PT> Objects = new List<PT>();
        List<TagText> TagTexts = new List<TagText>();

        public MainWindow()
        {
            InitializeComponent();
            ValidRect = new Rect(0, 0, RectMaxW, RectMaxH);
        }

        private void Clk_DataLoad(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == false)
                return;

            VM = new VecMgr();
            VM.ReadData(dialog.FileName);
        }

        private void Clk_CreateObject(object sender, RoutedEventArgs e)
        {
            byte R = (byte)RandSeed.Next(0xFF);
            byte G = (byte)RandSeed.Next(0xFF);
            byte B = (byte)RandSeed.Next(0xFF);
            Color col = Color.FromArgb(0x80, R, G, B);

            //  付箋の作成
            string test = VM.Words[RandSeed.Next(VM.Words.Count)].Name;
            PT pt = new PT(RandSeed.Next(RectMaxW), RandSeed.Next(RectMaxH));
            TagText tagText = new TagText(test, pt);
            pt.vx = ( - 0.14713 * R - 0.28886 * G + 0.43600 * B ) / 128;
            pt.vy = ( + 0.61500 * R - 0.51499 * G - 0.10001 * B ) / 128;

            //  キャンバスに描画
            canvas1.Children.Add(tagText.RectCtrl);
            canvas1.Children.Add(tagText.TextCtrl);

            //  オブジェクト追加
            Objects.Add(pt);
            TagTexts.Add(tagText);
        }


        private void ShowObjects()
        {
            canvas1.Children.Clear();

            //  描画
            for( int i = 0; i < TagTexts.Count; i++ )
            {
                TagTexts[i].SetPoint(Objects[i]);
                canvas1.Children.Add(TagTexts[i].RectCtrl);
                canvas1.Children.Add(TagTexts[i].TextCtrl);
            }
        }

        private void Clk_Aggregation(object sender, RoutedEventArgs e)
        {
            //  引力の計算と座標移動
            NBodyCalculator nBody = new NBodyCalculator(Objects, TagText.ObjSize, ValidRect);
            nBody.Aggregation();

            Objects = new List<PT>(nBody.Objects);
            ShowObjects();
        }

        private void Clk_Diffusion(object sender, RoutedEventArgs e)
        {
            //  斥力の計算と座標移動
            NBodyCalculator nBody = new NBodyCalculator(Objects, TagText.ObjSize, ValidRect);
            nBody.Diffusion();

            Objects = new List<PT>(nBody.Objects);
            ShowObjects();
        }

        private void Clk_Classification(object sender, RoutedEventArgs e)
        {
            //  斥力の計算と座標移動
            NBodyCalculator nBody = new NBodyCalculator(Objects, TagText.ObjSize, ValidRect);
            nBody.Classification();

            Objects = new List<PT>(nBody.Objects);
            ShowObjects();
        }
    }
}
