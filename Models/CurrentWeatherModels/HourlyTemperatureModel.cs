using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarWeather
{
    //This contains the data for the "week" section of the window
    public class HourlyTemperatureModel
    {
        public Forecast forecast { get; set; }
        public class Forecast
        {
            public Forecastday[] forecastday { get; set; }
        }

        public class Forecastday
        {
            public string date { get; set; }
            public int date_epoch { get; set; }
            public Hour[] hour { get; set; }
        }

        public class Hour
        {
            public int time_epoch { get; set; }
            public string time { get; set; }
            public float temp_c { get; set; }
            public int is_day { get; set; }
            public Condition condition { get; set; }
        }

        public class Condition
        {
            public string text { get; set; }
            public string icon { get; set; }
            public int code { get; set; }
        }
    }
}
