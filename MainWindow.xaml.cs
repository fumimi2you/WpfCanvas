using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfCanvas
{


    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        const double TxtFont = 24.0; // テキストのフォント
        const int ObjSize = 50; // 付箋のサイズ
        const int RectMax = 900; // 領域サイズ
        Rect ValidRect;


        Random RandSeed = new Random();
        List<PT> Objects = new List<PT>();
        List<Rectangle> Rects = new List<Rectangle>();

        public MainWindow()
        {
            InitializeComponent();
            ValidRect = new Rect(0, 0, RectMax, RectMax);
        }

        Rectangle MakeRect()
        {
            Rectangle rect = new Rectangle
            {
                Height = ObjSize,
                Width = ObjSize,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Color.FromArgb(0x80,
                    (byte)RandSeed.Next(0xFF),
                    (byte)RandSeed.Next(0xFF),
                    (byte)RandSeed.Next(0xFF))),
            };
            return rect;
        }

        ContentControl MakeText(string text, double x, double y )
        {
            ContentControl content = new ContentControl();
            Canvas.SetLeft(content, x);
            Canvas.SetTop(content, y);
            content.Width = ObjSize;
            content.Height = ObjSize;

            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.FontSize = TxtFont;
            tb.Foreground = Brushes.Black;
            content.Content = tb;

            return content;
        }

        private void Clk_CreateObject(object sender, RoutedEventArgs e)
        {
            PT pt = new PT(RandSeed.Next(RectMax), RandSeed.Next(RectMax));
            Rectangle rect = MakeRect();

            canvas1.Children.Add(rect);
            Canvas.SetLeft(rect, pt.x);
            Canvas.SetTop(rect, pt.y);

            canvas1.Children.Add(MakeText("Txt", pt.x, pt.y));

            Objects.Add(pt);
            Rects.Add(rect);
        }


        private void ShowObjects()
        {
            canvas1.Children.Clear();

            //  描画
            for( int i = 0; i < Objects.Count; i++ )
            {
                canvas1.Children.Add(Rects[i]);
                Canvas.SetLeft(Rects[i], Objects[i].x);
                Canvas.SetTop(Rects[i], Objects[i].y);
            }
        }

        private void Clk_Aggregation(object sender, RoutedEventArgs e)
        {
            //  引力の計算と座標移動
            NBodyCalculator nBody = new NBodyCalculator(Objects, ObjSize, ValidRect);
            nBody.Aggregation();

            Objects = new List<PT>(nBody.Objects);
            ShowObjects();
        }

        private void Clk_Diffusion(object sender, RoutedEventArgs e)
        {
            //  斥力の計算と座標移動
            NBodyCalculator nBody = new NBodyCalculator(Objects, ObjSize, ValidRect);
            nBody.Diffusion();

            Objects = new List<PT>(nBody.Objects);
            ShowObjects();
        }


    }
}
