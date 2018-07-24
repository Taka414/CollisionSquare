using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CollisionSquare
{
    // ひし型の内外判定を行うクラス
    public partial class MainWindow : Window
    {
        // キャンバスをクリックした回数
        private int _ClickCount;
        // 緑色の点のリスト
        private List<Ellipse> _EllipseList= new List<Ellipse>();
        // 赤色の点
        private Ellipse _P;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        // クリアボタンが押された時
        protected virtual void Button_Click(object sender, RoutedEventArgs e)
        {
            this._ClickCount = 0;
            this.Canvas.Children.Clear();
            this._EllipseList.Clear();
            //this._LineList.Clear();
            this._P = null;
        }

        // キャンバス上で左クリックされた時
        protected virtual void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(this.Canvas);
            this.TextBlock.Text = $"({p.X}, {p.Y})";

            // 4点の配置
            if (this._ClickCount < 4)
            {
                var _ellipse = new Ellipse()
                {
                    Width = 7,
                    Height = 7,
                    //Fill = new SolidColorBrush(Colors.Green),
                    Fill = Brushes.Green,
                };

                Canvas.SetTop(_ellipse, p.Y - _ellipse.Height / 2);
                Canvas.SetLeft(_ellipse, p.X - _ellipse.Width / 2);

                this.Canvas.Children.Add(_ellipse);
                this._EllipseList.Add(_ellipse);
            }

            // ひとつ前の点と今の点を線でつなぐ
            if (this._ClickCount >= 1 && this._ClickCount < 4)
            {
                Ellipse p1 = this._EllipseList[this._EllipseList.Count - 2];
                Ellipse p2 = this._EllipseList[this._EllipseList.Count - 1];
                DrawLineBetweenEllipse(p1, p2);
            }

            // 最初の点と最後の点を線でつなぐ
            if (this._ClickCount == 3)
            {
                Ellipse p1 = this._EllipseList[0];
                Ellipse p2 = this._EllipseList[this._EllipseList.Count - 1];
                DrawLineBetweenEllipse(p1, p2);
            }

            if (++this._ClickCount <= 4)
            {
                return;
            }

            // ---- 以降は内外判定の処理 ----

            if (this._P != null)
            {
                this.Canvas.Children.Remove(this._P); // 以前の地点を消す
            }

            this._P = new Ellipse()
            {
                Width = 7,
                Height = 7,
                Fill = Brushes.Red,
            };

            Canvas.SetTop(this._P, p.Y - this._P.Height / 2);
            Canvas.SetLeft(this._P, p.X - this._P.Width / 2);

            this.Canvas.Children.Add(this._P);

            // 内外判定の計算
            Point2f pa = GetPoint2f(this._EllipseList[0]);
            Point2f pb = GetPoint2f(this._EllipseList[1]);
            Point2f pc = GetPoint2f(this._EllipseList[2]);
            Point2f pd = GetPoint2f(this._EllipseList[3]);
            Point2f pp = GetPoint2f(this._P);

            bool ret = CollisionUtil.IsInSquare(pa, pb, pc, pd, pp);

            string atx = $"A=({Canvas.GetLeft(this._EllipseList[0]) + this._EllipseList[0].Width / 2}, {Canvas.GetTop(this._EllipseList[0]) + this._EllipseList[0].Height/ 2})";
            string btx = $"B=({Canvas.GetLeft(this._EllipseList[1]) + this._EllipseList[1].Width / 2}, {Canvas.GetTop(this._EllipseList[1]) + this._EllipseList[0].Height / 2})";
            string ctx = $"C=({Canvas.GetLeft(this._EllipseList[2]) + this._EllipseList[2].Width / 2}, {Canvas.GetTop(this._EllipseList[2]) + this._EllipseList[0].Height / 2})";
            string dtx = $"D=({Canvas.GetLeft(this._EllipseList[3]) + this._EllipseList[3].Width / 2}, {Canvas.GetTop(this._EllipseList[3]) + this._EllipseList[0].Height / 2})";
            string ptx = $"P=({Canvas.GetLeft(this._P) + this._P.Width / 2}, {Canvas.GetTop(this._P) + this._P.Height / 2})";

            this.TextBlock.Text = $"{atx}, {btx}, {ctx}, {dtx} & {ptx} = {ret.ToString().ToLower()}";
            Trace.WriteLine(this.TextBlock.Text);
        }

        // 2つのEllipseを線で結ぶ線を作成する
        public void DrawLineBetweenEllipse(Ellipse p1, Ellipse p2)
        {
            var _line = new Line()
            {
                X1 = Canvas.GetLeft(p1) + p1.Width / 2,
                X2 = Canvas.GetLeft(p2) + p1.Width / 2,
                Y1 = Canvas.GetTop(p1) + p1.Height / 2,
                Y2 = Canvas.GetTop(p2) + p1.Height / 2,
                Stroke = new SolidColorBrush(new Color() { R = 0x11, G = 0x83, B = 0x8B, A = 255 }),
                StrokeThickness = 0.5,
            };

            this.Canvas.Children.Add(_line); // ここで追加してしまう
        }

        // Ellipseオブジェクトから点を取得する
        public Point2f GetPoint2f(Ellipse p)
        {
            return new Point2f((float)(Canvas.GetLeft(p) + p.Width / 2), (float)(Canvas.GetTop(p) + p.Height / 2));
        }
    }

    // 衝突判定系の汎用処理クラス
    public static class CollisionUtil
    {
        // 指定した4点の中にpがあるかどうかを判定する
        // 但し、4つそれぞれの内角のいずれかが180度以上の場合、正しく判定できない。
        public static bool IsInSquare(Point2f pa, Point2f pb, Point2f pc, Point2f pd, Point2f p)
        {
            float a = CalcExteriorProduct(pa, pb, p);
            float b = CalcExteriorProduct(pb, pc, p);
            float c = CalcExteriorProduct(pc, pd, p);
            float d = CalcExteriorProduct(pd, pa, p);

            Trace.WriteLine($"{a}, {b}, {c}, {d}");

            bool res = a > 0 && b > 0 && c > 0 && d > 0;

            return res;
        }

        // 指定した2点間と1点の外積を計算する
        public static float CalcExteriorProduct(Point2f a, Point2f b, Point2f p)
        {
            // 点 a,b のベクトル
            var vecab = new Point2f(a.X - b.X, a.Y - b.Y); // ここは固定なら最初から計算しておくのもアリかも
            // 点a と 点のベクトル
            var vecpa = new Point2f(a.X - p.X, a.Y - p.Y);
            // 外積を計算する
            float ext = vecab.X * vecpa.Y - vecpa.X * vecab.Y;

            return ext;
        }
    }

    // 単純な2点の情報を表すクラス
    public class Point2f
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public Point2f(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString() => $"({this.X}, {this.Y})";
    }
}