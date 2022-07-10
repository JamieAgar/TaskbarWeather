using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarWeather
{
    //This contains the data for the "week" section of the window
    public class WeekWeatherModel
    {
        public Datum[] data { get; set; }

        public class Datum
        {
            public Weather weather { get; set; }
            public float max_temp { get; set; }
            public float min_temp { get; set; }
            public int ts { get; set; }
        }

        public class Weather
        {
            public string icon { get; set; }
            public int code { get; set; }
            public string description { get; set; }
        }



    }
}
