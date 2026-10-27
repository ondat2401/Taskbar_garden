using System;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenWeatherVerification
    {
        [MenuItem("Tools/Idle Garden/Verify Weather Sync F14")]
        public static void Run()
        {
            Require(GardenWeatherClient.FromWmoCode(0) == GardenWeatherKind.Clear, "Clear mapping");
            Require(GardenWeatherClient.FromWmoCode(3) == GardenWeatherKind.Overcast, "Overcast mapping");
            Require(GardenWeatherClient.FromWmoCode(45) == GardenWeatherKind.Fog, "Fog mapping");
            Require(GardenWeatherClient.FromWmoCode(61) == GardenWeatherKind.Rain, "Rain mapping");
            Require(GardenWeatherClient.FromWmoCode(73) == GardenWeatherKind.Snow, "Snow mapping");
            Require(GardenWeatherClient.FromWmoCode(95) == GardenWeatherKind.Thunderstorm, "Storm mapping");
            Require(GardenWeatherClient.FromWmoCode(97) == GardenWeatherKind.Thunderstorm, "Heavy storm mapping");
            Require(GardenWeatherClient.FromWmoCode(-1) == GardenWeatherKind.Unknown, "Unknown mapping");

            string url = GardenWeatherClient.BuildRequestUrl(10.8231f, 106.6297f);
            Require(url.Contains("latitude=10.8231") && url.Contains("longitude=106.6297") && url.Contains("weather_code"), "Request URL");
            Require(GardenWeatherClient.TryParseCurrent("{\"current\":{\"temperature_2m\":28.7,\"weather_code\":61}}", out var report), "Valid response parse");
            Require(report.Kind == GardenWeatherKind.Rain && Math.Abs(report.TemperatureC - 28.7f) < .01f, "Parsed current weather");
            Require(!GardenWeatherClient.TryParseCurrent("{invalid", out _), "Malformed response rejection");
            Require(!GardenWeatherClient.TryParseCurrent("{\"current\":{\"temperature_2m\":NaN,\"weather_code\":0}}", out _), "Non-finite temperature rejection");

            Debug.Log("F14 weather mapping, URL and response validation pass.");
        }

        private static void Require(bool condition, string check)
        {
            if (!condition) throw new InvalidOperationException("F14 weather verification failed: " + check);
        }
    }
}
