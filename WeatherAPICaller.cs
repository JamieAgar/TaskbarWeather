using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarWeather
{
    public static class WeatherAPICaller
    {
        public static HttpClient ApiClient { get; set; }

        public static void InitializeClient()
        {
            ApiClient = new HttpClient();
            //No longer use BaseAddress as I'm using multiple different APIs with different URLs
            //ApiClient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");

            //This tells us we are specifically looking for json instead of a webpage
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

    }
}
