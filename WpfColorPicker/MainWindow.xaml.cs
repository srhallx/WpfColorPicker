using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WpfColorPicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        System.Timers.Timer MouseMoveTimer;
        Bitmap windowPixel = new Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        public MainWindow()
        {
            InitializeComponent();
            MouseMoveTimer = new System.Timers.Timer(100);
            MouseMoveTimer.Elapsed += MouseMoveTimer_Elapsed;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MouseMoveTimer.Start();
        }


        private void MouseMoveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Drawing.Point cursor = new System.Drawing.Point();
            GetCursorPos(ref cursor);

            var c = ColorAtLocation(cursor);

            ScreenViewer.Dispatcher.Invoke(() =>
                {
                    ScreenViewer.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(windowPixel.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                });

            ScreenColor.Dispatcher.Invoke( () => {
                    ScreenColor.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
                hexRGB.Text = String.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
                });
        }

       

        public System.Drawing.Color ColorAtLocation(System.Drawing.Point location)
        {
            using (Graphics graphicsOut = Graphics.FromImage(windowPixel))
            {
                using (Graphics graphicsIn = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hInContext = graphicsIn.GetHdc();
                    IntPtr hOutContext = graphicsOut.GetHdc();

                    int retval = BitBlt(hOutContext, 0, 0, 8, 8, hInContext, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);

                    graphicsOut.ReleaseHdc();
                    graphicsIn.ReleaseHdc();
                }
            }

            System.Drawing.Color myColor = windowPixel.GetPixel(4, 4);
            return myColor;
        }
    }


}

