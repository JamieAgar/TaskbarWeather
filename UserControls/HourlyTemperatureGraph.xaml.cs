using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace TaskbarWeather.UserControls
{
    /// <summary>
    /// Interaction logic for HourlyTemperatureGraph.xaml
    /// </summary>
    public partial class HourlyTemperatureGraph : UserControl, INotifyPropertyChanged
    {
        public SeriesCollection SeriesCollection
        {
            get { return (SeriesCollection)GetValue(SeriesCollectionProperty); }
            set { SetValue(SeriesCollectionProperty, value); }
        }
        public static readonly DependencyProperty SeriesCollectionProperty = DependencyProperty.Register("SeriesCollection", typeof(SeriesCollection), typeof(HourlyTemperatureGraph));

        public string[] Labels
        {
            get { return (string[])GetValue(LabelsProperty); }
            set { SetValue(LabelsProperty, value); }
        }
        public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register("Labels", typeof(string[]), typeof(HourlyTemperatureGraph));
        public Func<double, string> YFormatter { get; set; }

        public HourlyTemperatureGraph()
        {
            DataContext = this;
            InitializeComponent();
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title="Series 1",
                    Values = new ChartValues<int> {2,4,6,8,10,8,6,4,2,1,0,5}
                }
            };

            Labels = new[] { "12am", "1am", "2am", "3am", "4am", "5am", "6am", "7am", "8am", "9am", "10am", "11am"};
            YAxis.LabelFormatter = val => val + "°C";


        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
