using System;
using System.Windows;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Interop;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;

namespace TaskbarWeather
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon TBWeatherIcon = new NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();
            Hide();
            //Right click menu
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            //Closes the app
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem
            {
                Text = "Exit",
            };
            exitMenuItem.Click += (e, s) =>
            {
                System.Windows.Application.Current.Shutdown();
            };

            contextMenuStrip.Items.Add(exitMenuItem);
            TBWeatherIcon.ContextMenuStrip = contextMenuStrip;

            TBWeatherIcon.Icon = new Icon(@"Resources/Icon.ico");
            TBWeatherIcon.Visible = true;
            TBWeatherIcon.Text = "Tray Application";
            TBWeatherIcon.DoubleClick += new EventHandler(tbWeatherIcon_DoubleClick);
        }

        //Handles displaying the window
        private void tbWeatherIcon_DoubleClick(object Sender, EventArgs e)
        {
            Trace.WriteLine("Doubleclicked");
            // Show the form when the user double clicks on the notify icon.

            // Set the WindowState to normal if the form is minimized.
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            // Activate the form.
            Activate();
            var coor = GetMousePositionWindowsForms();
            Show_Window(coor.X);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Trace.WriteLine("Deactivated");
            Hide();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            Trace.WriteLine("Activated");
            var coor = GetMousePositionWindowsForms();
            Show_Window(coor.X);
        }
        private void Show_Window(double cursorX)
        {
            AdjustWindowPosition(cursorX);
            Show();
        }

        //Takes in location of cursor at the time the notifyicon is clicked, and moves the window to an appropriate location
        private void AdjustWindowPosition(double cursorX)
        {
            Screen sc = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            if (sc.WorkingArea.Top > 0)
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                var middleOfWindow = desktopWorkingArea.Right - (Width / 2);
                var gapToMiddle = middleOfWindow - cursorX;
                if (gapToMiddle < 0) gapToMiddle = 0;
                Left = desktopWorkingArea.Right - Width - gapToMiddle;
                Top = desktopWorkingArea.Top;
            }

            else if ((sc.Bounds.Height - sc.WorkingArea.Height) > 0)
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                var middleOfWindow = desktopWorkingArea.Right - (Width / 2);
                var gapToMiddle = middleOfWindow - cursorX;
                if (gapToMiddle < 0) gapToMiddle = 0;
                Left = desktopWorkingArea.Right - Width - gapToMiddle;
                Top = desktopWorkingArea.Bottom - Height;
            }
            else
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
            }
        }

        public static System.Windows.Point GetMousePositionWindowsForms()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            var pixelX = (int)((96 / g.DpiX) * point.X);
            var pixelY = (int)((96 / g.DpiY) * point.X);
            return new System.Windows.Point(pixelX, pixelY);
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void textHint_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            textSearch.Focus();
        }

        private void textSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textSearch.Text) && textSearch.Text.Length > 0)
            {
                textHint.Visibility = Visibility.Collapsed;
            }
            else
            {
                textHint.Visibility = Visibility.Visible;
            }
        }

        private void Week_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TodayForecast.Visibility = Visibility.Collapsed;
            WeekForecast.Visibility = Visibility.Visible;
            WeekLabel.Style = FindResource("activeTextButton") as Style;
            TodayLabel.Style = FindResource("textButton") as Style;
        }
        
        private void Today_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WeekForecast.Visibility = Visibility.Collapsed;
            TodayForecast.Visibility = Visibility.Visible;
            TodayLabel.Style = FindResource("activeTextButton") as Style;
            WeekLabel.Style = FindResource("textButton") as Style;
        }

    }
}
