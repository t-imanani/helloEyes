using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace helloEyes
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        const double DEFAULT_CLIENT_AREA_WIDTH  = 256;
        const double DEFAULT_CLIENT_AREA_HEIGHT = 256;

        const double DEFAULT_EYE_WIDTH  = 25;
        const double DEFAULT_EYE_HEIGHT = 50;

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int x;
            public int y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.x, point.y);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.Width  = DEFAULT_CLIENT_AREA_WIDTH;
            this.Height = DEFAULT_CLIENT_AREA_HEIGHT;

            this.m_LeftEye.Width = DEFAULT_EYE_WIDTH;
            this.m_LeftEye.Height = DEFAULT_EYE_HEIGHT;
            this.m_RightEye.Width = DEFAULT_EYE_WIDTH;
            this.m_RightEye.Height = DEFAULT_EYE_HEIGHT;
        }

        /// <summary>
        /// 瞳オブジェクトの位置更新
        /// </summary>
        private void UpdateEyesPosition()
        {
            var mouseCursor = GetCursorPosition();
            var clientOrigin = this.m_ClientArea.PointToScreen(new System.Windows.Point(0, 0));
            var w = this.m_ClientArea.ActualWidth;
            var h = this.m_ClientArea.ActualHeight;

            Point leftEyeOffset = new Point(-57, -15);
            Point leftEyeMaxRadius = new Point(23, 62);
            SetEyePos(this.m_LeftEye, w, h, leftEyeOffset, leftEyeMaxRadius, mouseCursor, clientOrigin);

            Point rightEyeOffset = new Point(58, -20);
            Point rightEyeMaxRadius = new Point(28, 67);
            SetEyePos(this.m_RightEye, w, h, rightEyeOffset, rightEyeMaxRadius, mouseCursor, clientOrigin);
        }

        /// <summary>
        /// 瞳オブジェクトをCanvasの座標に設定
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="offset"></param>
        /// <param name="maxRadius"></param>
        /// <param name="mouseCursor"></param>
        /// <param name="clientOrigin"></param>
        private void SetEyePos(Ellipse eye, double w, double h, Point offset, Point maxRadius, Point mouseCursor, Point clientOrigin)
        {
            var scaleX = w / 256.0;
            var scaleY = h / 256.0;
            var screenEyePos = new Point(clientOrigin.X + w / 2 + scaleX * offset.X, clientOrigin.Y + h / 2 + scaleY * offset.Y);
            var distance = GetDistance(screenEyePos, mouseCursor);
            var radiusX = Math.Min(maxRadius.X, distance) * scaleX;
            var radiusY = Math.Min(maxRadius.Y, distance) * scaleY;
            var rad = GetAngle(screenEyePos, mouseCursor);
            Canvas.SetLeft(eye, -eye.ActualWidth / 2 + w / 2 + scaleX * offset.X + radiusX * Math.Cos(rad));
            Canvas.SetTop(eye, -eye.ActualHeight / 2 + h / 2 + scaleY * offset.Y + radiusY * Math.Sin(rad));
        }

        /// <summary>
        /// 2点間の距離を取得
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        }

        /// <summary>
        /// 2点間のなす角を取得
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double GetAngle(Point p1, Point p2)
        {
            return Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        }

        /// <summary>
        /// マウスカーソル位置の取得
        /// </summary>
        /// <returns></returns>
        private Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        /// <summary>
        /// 瞳の大きさを変更
        /// </summary>
        private void UpdateEyesSize()
        {
            var scaleX = m_ClientArea.ActualWidth  / DEFAULT_CLIENT_AREA_WIDTH;
            var scaleY = m_ClientArea.ActualHeight / DEFAULT_CLIENT_AREA_HEIGHT;

            this.m_LeftEye.Width   = scaleX * DEFAULT_EYE_WIDTH;
            this.m_LeftEye.Height  = scaleY * DEFAULT_EYE_HEIGHT;
            this.m_RightEye.Width  = scaleX * DEFAULT_EYE_WIDTH;
            this.m_RightEye.Height = scaleY * DEFAULT_EYE_HEIGHT;
        }

        /// <summary>
        /// ロード時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Application.Current?.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                    {
                        UpdateEyesPosition();
                    }));
                    Thread.Sleep(30);
                }
            });             
        }

        /// <summary>
        /// 左クリック時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        /// <summary>
        /// 画面サイズ変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateEyesSize();
        }

        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 背景のチェック状態が変更された時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundCheckChanged(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItem menuItem)
            {
                this.Background = menuItem.IsChecked ? Brushes.White : Brushes.Transparent;
                this.ResizeMode = menuItem.IsChecked ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
            }
        }
    }
}
