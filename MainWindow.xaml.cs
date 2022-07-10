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
        public SeriesCollection WindowSeriesCollection
        { 
            get { return (SeriesCollection)GetValue(WindowSeriesCollectionProperty); }
            set { SetValue(WindowSeriesCollectionProperty, value); }
        }
        public static readonly DependencyProperty WindowSeriesCollectionProperty = DependencyProperty.Register(
            "WindowSeriesCollection", 
            typeof(SeriesCollection), 
            typeof(MainWindow));

        public string[] WindowLabels
        {
            get { return (string[])GetValue(WindowLabelsProperty); }
            set { SetValue(WindowLabelsProperty, value); }
        }
        public static readonly DependencyProperty WindowLabelsProperty = DependencyProperty.Register("WindowLabels", typeof(string[]), typeof(MainWindow));

        private readonly string appid;

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

            //This is the data we use to create the temperature/time graph
            WindowSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<int> {10,0,10,0,10,0,10,0,10,0,10,0}
                }
            };
            WindowLabels = new[] {"2am", "3am", "4am", "5am", "6am", "7am", "8am", "9am", "10am", "11am", "12am", "1am" };

            try
            {
                appid = APIKeys.WeatherAPIKey;
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Missing value for 'WeatherAPIKey' in APIKeys.cs");
                appid = "missing";
                ex.ToString();
            }

            WeatherAPICaller.InitializeClient();
            //Need to pass the appid to the API caller
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

            GetCurrentWeather();
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

        private void textSearch_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtSearch.Focus();
        }

        private void txtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text) && txtSearch.Text.Length > 0)
            {
                textSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                textSearch.Visibility = Visibility.Visible;
            }
        }
        private void txtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                GetCurrentWeather(txtSearch.Text);
            }
        }
        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            Favourites1.IsChecked = false;
            Favourites2.IsChecked = false;
            Favourites3.IsChecked = false;
        }

        #region API
        //https://openweathermap.org/current - Today
        //https://openweathermap.org/forecast16 - Week (can set cnt to 7)

        private string curLocation = "";
        private async Task GetCurrentWeather(string location = "London")
        {
            //No need to do an API call if we are already displaying this location
            if (location == curLocation) return;
            curLocation = location;

            var currentWeather = await WeatherProcessor.GetCurrentWeather(appid, location);
            LocationLabel.Text = currentWeather.name;
            TemperatureLabel.Text = Math.Round(currentWeather.main.temp).ToString();
            WindLabel.Text = Math.Round(currentWeather.wind.speed).ToString() + "m/s";
            HumidityLabel.Text = currentWeather.main.humidity + "%";
            WeatherIcon.Source = GetWeatherIconImage(currentWeather.weather[0].icon);

            //Can't use switch statement as not available in framework 4.7.2
            if (currentWeather.clouds.all > 84)
            {
                CloudinessLabel.Text = "Overcast Clouds";
            }
            else if (currentWeather.clouds.all > 50)
            {
                CloudinessLabel.Text = "Broken Clouds";
            }
            else if (currentWeather.clouds.all > 24)
            {
                CloudinessLabel.Text = "Scattered Clouds";
            }
            else if (currentWeather.clouds.all > 10)
            {
                CloudinessLabel.Text = "Few Clouds";
            } 
            else
            {
                CloudinessLabel.Text = "Clear Sky";
            }
        }

        public BitmapImage GetWeatherIconImage(string icon)
        {
            using (WebClient client = new WebClient())
            {
                if (File.Exists(Path.GetTempPath() + icon + ".png"))
                {
                    return new BitmapImage(new Uri(Path.GetTempPath() + icon + ".png"));
                }

                client.DownloadFile("http://openweathermap.org/img/wn/" + icon + "@2x.png",
                    Path.GetTempPath() + icon + ".png");
                return new BitmapImage(new Uri(Path.GetTempPath() + icon + ".png"));
            }

        }

        #endregion

        #region FavouritesSystem
        private string Favourite1 = "";
        private string Favourite2 = "";
        private string Favourite3 = "";

        private void Favourites1_Checked(object sender, RoutedEventArgs e)
        {
            Favourites2.IsChecked = false;
            Favourites2.Background = System.Windows.Media.Brushes.Transparent;
            Favourites3.IsChecked = false;
            Favourites3.Background = System.Windows.Media.Brushes.Transparent;

            if(Favourite1.Length > 0)
            {
                ReplaceStarWithDelete();
                GetCurrentWeather(Favourite1);
            }
            else
            {
                ReplaceDeleteWithStar();
            }
        }
        private void Favourites2_Checked(object sender, RoutedEventArgs e)
        {

            Favourites1.IsChecked = false;
            Favourites1.Background = System.Windows.Media.Brushes.Transparent;
            Favourites3.IsChecked = false;
            Favourites3.Background = System.Windows.Media.Brushes.Transparent;

            if (Favourite2.Length > 0)
            {
                ReplaceStarWithDelete();
                GetCurrentWeather(Favourite2);
            }
            else
            {
                ReplaceDeleteWithStar();
            }
        }
        private void Favourites3_Checked(object sender, RoutedEventArgs e)
        {

            Favourites1.IsChecked = false;
            Favourites1.Background = System.Windows.Media.Brushes.Transparent;
            Favourites2.IsChecked = false;
            Favourites2.Background = System.Windows.Media.Brushes.Transparent;

            if (Favourite3.Length > 0)
            {
                ReplaceStarWithDelete();
                GetCurrentWeather(Favourite3);
            }
            else
            {
                ReplaceDeleteWithStar();
            }
        }
        private void ReplaceStarWithDelete()
        {
            StarButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Visible;
        }
        private void ReplaceDeleteWithStar()
        {
            DeleteButton.Visibility = Visibility.Collapsed;
            StarButton.Visibility = Visibility.Visible;
        }
        private bool IsAnyChecked()
        {
            return Favourites1.IsChecked.Value || Favourites2.IsChecked.Value || Favourites3.IsChecked.Value;
        }
        private void Favourites_Unchecked(object sender, RoutedEventArgs e)
        {
            if(IsAnyChecked())
            {
                return;
            }
            ReplaceDeleteWithStar();
        }
        private void Star_Clicked(object sender, RoutedEventArgs e)
        {
            if (!IsAnyChecked()) return; //None of the favourites are checked, do nothing
            if (Favourites1.IsChecked.Value)
            {
                if(!string.IsNullOrEmpty(txtSearch.Text))
                {
                    Favourite1 = txtSearch.Text;
                    Favourites1.Content = txtSearch.Text[0];
                }
            }
            if (Favourites2.IsChecked.Value)
            {
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    Favourite2 = txtSearch.Text;
                    Favourites2.Content = txtSearch.Text[0];
                }
            }
            if (Favourites3.IsChecked.Value)
            {
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    Favourite3 = txtSearch.Text;
                    Favourites3.Content = txtSearch.Text[0];
                }
            }
            ReplaceStarWithDelete();

        }
        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            if (!IsAnyChecked()) return; //None of the favourites are checked, do nothing
            if (Favourites1.IsChecked.Value)
            {
                Favourite1 = "";
                Favourites1.Content = "";
            }
            if (Favourites2.IsChecked.Value)
            {
                Favourite2 = "";
                Favourites2.Content = "";
            }
            if (Favourites3.IsChecked.Value)
            {
                Favourite3 = "";
                Favourites3.Content = "";
            }
            ReplaceDeleteWithStar();
        }
        #endregion

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
