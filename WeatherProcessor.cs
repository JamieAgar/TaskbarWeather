﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarWeather
{
    public class WeatherProcessor
    {
        /*
         * Due to limitations of free accounts of the various weather APIs, we have to use multiple to get all the results we want
         * Current weather -> Openweathermap
         * 16day (only using 7day) -> Weatherbit
         * Hourly -> WeatherAPI
         */

        //Returns data for today's weather
        public static async Task<CurrentWeatherModel> GetCurrentWeather(string appid, string cityName = "London")
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={appid}&units=metric";
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
        //Returns data for the weeks' weather. Can modify the function to retrieve up to 16 days, if necessary
        public static async Task<WeekWeatherModel> GetWeekWeather(string appid, string cityName = "London")
        {
            //"days" is the number of days we wish to retrieve. It can be changed up to 16 days
            string url = $"https://api.weatherbit.io/v2.0/forecast/daily?city={cityName}&key={appid}&units=M&days=7";
            using (HttpResponseMessage response = await WeatherAPICaller.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    WeekWeatherModel weekWeather = await response.Content.ReadAsAsync<WeekWeatherModel>();
                    return weekWeather;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
        //Returns hourly data for the next 2 days, from 00-23 both days. This so we can display the next 10 hours even if there's less than 10
        //remaining in the day
        public static async Task<HourlyTemperatureModel> GetHourlyTemperature(string appid, string cityName = "London")
        {
            //"days" is the number of days we wish to retrieve. It can be changed up to 16 days
            string url = $"https://api.weatherapi.com/v1/forecast.json?key={appid}&q={cityName}&days=2&aqi=no&alerts=no";
            using (HttpResponseMessage response = await WeatherAPICaller.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    HourlyTemperatureModel hourlyTemperature = await response.Content.ReadAsAsync<HourlyTemperatureModel>();
                    return hourlyTemperature;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

    }
}