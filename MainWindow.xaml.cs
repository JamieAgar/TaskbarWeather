﻿using System;
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
using System.Windows.Controls;
using TaskbarWeather.UserControls;
using System.Globalization;

namespace TaskbarWeather
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon TBWeatherIcon = new NotifyIcon();

        private readonly string OpenWeatherAppid;
        private readonly string WeatherBitAppid;
        private readonly string WeatherAPIAppid;

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

            TBWeatherIcon.Icon = new Icon(@"Resources/icon.ico");
            TBWeatherIcon.Visible = true;
            TBWeatherIcon.Text = "Weather";
            TBWeatherIcon.DoubleClick += new EventHandler(tbWeatherIcon_DoubleClick);

            //This is the data we use to create the temperature/time graph
            //Use some default data to create the control, also may alert the user that something went wrong
            SeriesCollection Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<int> {10,0,10,0,10,0,10,0,10,0,10,0}
                }
            };
            string[] Labels = new[] {"2am", "3am", "4am", "5am", "6am", "7am", "8am", "9am", "10am", "11am", "12am", "1am" };
            HourlyTemperatureGraph temperatureGraph = HourlyTemperatureGraphControl;
            temperatureGraph.SeriesCollection = Series;
            temperatureGraph.Labels = Labels;

            try
            {
                OpenWeatherAppid = APIKeys.OpenWeatherAPIKey;
                WeatherBitAppid = APIKeys.WeatherBitAPIKey;
                WeatherAPIAppid = APIKeys.WeatherAPIKey;
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Missing APIKeys.cs, or missing value for one of the APIKeys");
                ex.ToString();
            }

            ReadFavouritesFromFile();

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

            if (Favourites1.IsChecked == true)
            {
                GetAllWeather(Favourite1);
            }
            else if (Favourites2.IsChecked == true)
            {
                GetAllWeather(Favourite2);
            }
            else if (Favourites3.IsChecked == true)
            {
                GetAllWeather(Favourite3);
            }
            else if (txtSearch.Text != "")
            {
                GetAllWeather(txtSearch.Text);
            }
            else 
            {
                GetAllWeather();
            }
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

        #region Control Event Handlers

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
                GetAllWeather(txtSearch.Text);
            }
        }
        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            Favourites1.IsChecked = false;
            Favourites2.IsChecked = false;
            Favourites3.IsChecked = false;
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
        #endregion

        #region API
        /*
         * Due to limitations of free accounts of the various weather APIs, we have to use multiple to get all the results we want
         * Current weather -> Openweathermap
         * 16day (only using 7day) -> Weatherbit
         * Hourly -> WeatherAPI
         * WeatherAPI has support for all of these so could refactor the code, though not necessary
         */

        private string curLocation = "";
        private async Task GetCurrentWeather(string location = "London")
        {
            //No need to do an API call if we are already displaying this location
            if (location == curLocation) return;

            var currentWeather = await WeatherProcessor.GetCurrentWeather(OpenWeatherAppid, location);
            LocationLabel.Text = currentWeather.name;
            TemperatureLabel.Text = Math.Round(currentWeather.main.temp).ToString();
            WindLabel.Text = Math.Round(currentWeather.wind.speed).ToString() + "m/s";
            HumidityLabel.Text = currentWeather.main.humidity + "%";
            WeatherIcon.Source = GetWeatherIconImage(currentWeather.weather[0].icon + "@2x", "http://openweathermap.org/img/wn/");

            //Converts "light rain" to "Light Rain"
            CloudinessLabel.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(currentWeather.weather[0].description.ToLower());
        }

        public BitmapImage GetWeatherIconImage(string icon, string url)
        {
            using (WebClient client = new WebClient())
            {
                if (File.Exists(Path.GetTempPath() + icon + ".png"))
                {
                    return new BitmapImage(new Uri(Path.GetTempPath() + icon + ".png"));
                }

                client.DownloadFile(url + icon + ".png",
                    Path.GetTempPath() + icon + ".png");
                return new BitmapImage(new Uri(Path.GetTempPath() + icon + ".png"));
            }

        }

        private async Task GetWeekWeather(string location = "London")
        {
            //No need to do an API call if we are already displaying this location
            if (location == curLocation) return;

            var weekWeather = await WeatherProcessor.GetWeekWeather(WeatherBitAppid, location);

            //Iterate through all the card days, first find them via the name then change their text
            StackPanel cardDayContainer = WeekForecast;
            for(int i = 0; i < 7; i++)
            {
                CardDay cardDay = (CardDay)cardDayContainer.FindName("Day" + i);
                if(cardDay != null)
                {
                    var data = weekWeather.data[i];

                    //Convert UNIX timestamp to DateTime
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(data.ts).ToLocalTime();
                    Console.WriteLine(dateTime.ToLocalTime());

                    cardDay.Day = dateTime.DayOfWeek.ToString().Substring(0, 3);
                    cardDay.MinNum = Math.Round(data.min_temp).ToString() + "°C";
                    cardDay.MaxNum = Math.Round(data.max_temp).ToString() + "°C";
                    cardDay.Source = GetWeatherIconImage(data.weather.icon, "https://www.weatherbit.io/static/img/icons/");
                    cardDay.ToolTip = data.weather.description;
                }
                else
                {
                    Console.WriteLine("Did not find day" + i);
                }
            }
        }
        private async Task GetHourlyTemperature(string location = "London")
        {
            //No need to do an API call if we are already displaying this location
            if (location == curLocation) return;

            var hourlyTemperature = await WeatherProcessor.GetHourlyTemperature(WeatherAPIAppid, location);
            var forecast = hourlyTemperature.forecast;

            //Create a collection of 10 ints
            ChartValues<int> Temperatures = new ChartValues<int>(new int[10]);
            string[] labels = new string[10];

            //Starting hour will allow us to only get future hours from the current day, instead of showing only the first 10 hours of today
            int startingHour = DateTime.Now.Hour;

            //TempNumber is a pointer to the position of the collection we are in
            int TempNumber = 0;
            //j is the day, i is the hour. 
            for (int j = 0; j < forecast.forecastday.Length; j++)
            {
                for (int i = startingHour; i < forecast.forecastday[j].hour.Length; i++)
                {
                    //We only want 10 temperatures
                    if (TempNumber < 10)
                    {
                        Temperatures[TempNumber] = (int)Math.Round(forecast.forecastday[j].hour[i].temp_c);
                        labels[TempNumber] = i.ToString() + ":00";
                        TempNumber++;
                    }
                }
                //We want to get values for the next day starting at hour 0
                startingHour = 0;
            }

            //Now create a series collection and labels from the data
            SeriesCollection series = new SeriesCollection
            {
                new LineSeries {
                    Title = "",
                    Values = Temperatures 
                }
            };

            HourlyTemperatureGraph tempGraph = HourlyTemperatureGraphControl;
            tempGraph.SeriesCollection = series;
            tempGraph.Labels = labels;
        }

        private void GetAllWeather(string location = "London")
        {
            GetCurrentWeather(location);
            GetWeekWeather(location);
            GetHourlyTemperature(location);

            //No need to do an API call if we are already displaying this location
            curLocation = location;
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
                GetAllWeather(Favourite1);
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
                GetAllWeather(Favourite2);
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
                GetAllWeather(Favourite3);
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
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                txtSearch.Text = txtSearch.Text.ToUpper();
                if (Favourites1.IsChecked.Value)
                {
                    Favourite1 = txtSearch.Text;
                    Favourites1.Content = txtSearch.Text[0];
                }
                if (Favourites2.IsChecked.Value)
                {
                    Favourite2 = txtSearch.Text;
                    Favourites2.Content = txtSearch.Text[0];
                }
                if (Favourites3.IsChecked.Value)
                {
                    Favourite3 = txtSearch.Text;
                    Favourites3.Content = txtSearch.Text[0];
                }
                GetAllWeather(txtSearch.Text);
            }
            SaveFavouritesToFile();
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
            SaveFavouritesToFile();
            ReplaceDeleteWithStar();
        }

        private void SaveFavouritesToFile()
        {
            Console.WriteLine("Saving to file");
            if (!File.Exists(@"favourites.txt"))
            {
                File.Create(@"favourites.txt");
            }
            using (StreamWriter sw = new StreamWriter(@"favourites.txt"))
            {
                //Null check. Shorthand for (Favourite1 != null?Favourite1 : "")
                sw.WriteLine(Favourite1 ?? "");
                sw.WriteLine(Favourite2 ?? "");
                sw.WriteLine(Favourite3 ?? "");
            }
        }

        private void ReadFavouritesFromFile()
        {
            Console.WriteLine("Reading from file");
            if (!File.Exists(@"favourites.txt"))
            {
                File.Create(@"favourites.txt");
            }

            string[] Favourites = new string[3];
            using (StreamReader sr = new StreamReader("favourites.txt"))
            {
                int i = 0;
                string line = "";
                while((line = sr.ReadLine()) != null)
                {
                    //Break out of the while loop instead of returning, to make sure the streamreader closes
                    if (i <= Favourites.Length - 1)
                    {
                        Favourites[i] = line;
                        i++;
                    }
                }
            }
            Favourite1 = Favourites[0];
            Favourite2 = Favourites[1];
            Favourite3 = Favourites[2];
            Favourites1.Content = Favourite1.Length > 0 ? Favourite1[0].ToString() : "";
            Favourites2.Content = Favourite2.Length > 0 ? Favourite2[0].ToString() : "";
            Favourites3.Content = Favourite3.Length > 0 ? Favourite3[0].ToString() : "";
        }

        #endregion


    }
}
