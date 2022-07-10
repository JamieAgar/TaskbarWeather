using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarWeather
{
    public class WeatherProcessor
    {
        public static async Task<CurrentWeatherModel> GetCurrentWeather(string appid, string cityName = "London")
        {
            string url = $"weather?q={cityName}&appid={appid}&units=metric";
            using (HttpResponseMessage response = await WeatherAPICaller.ApiClient.GetAsync(url))
            {
                if(response.IsSuccessStatusCode)
                {
                    CurrentWeatherModel currentWeather = await response.Content.ReadAsAsync<CurrentWeatherModel>();
                    return currentWeather;
                } else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

    }
}
