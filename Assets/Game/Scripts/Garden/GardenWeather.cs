using System;
using System.Globalization;
using UnityEngine;

namespace IdleGarden
{
    public enum GardenWeatherKind
    {
        Unknown,
        Clear,
        PartlyCloudy,
        Overcast,
        Fog,
        Rain,
        Snow,
        Thunderstorm
    }

    public readonly struct GardenWeatherReport
    {
        public readonly GardenWeatherKind Kind;
        public readonly int WmoCode;
        public readonly float TemperatureC;

        public GardenWeatherReport(GardenWeatherKind kind, int wmoCode, float temperatureC)
        {
            Kind = kind;
            WmoCode = wmoCode;
            TemperatureC = temperatureC;
        }
    }

    [Serializable]
    public sealed class GardenWeatherApiResponse
    {
        public Current current;

        [Serializable]
        public sealed class Current
        {
            public float temperature_2m;
            public int weather_code;
        }
    }

    public static class GardenWeatherClient
    {
        public const string Attribution = "Open-Meteo";
        // shortcut: the free endpoint is non-commercial; switch to a licensed provider before commercial release.
        private const string ForecastEndpoint = "https://api.open-meteo.com/v1/forecast";

        public static string BuildRequestUrl(float latitude, float longitude)
        {
            return string.Format(CultureInfo.InvariantCulture,
                ForecastEndpoint + "?latitude={0:0.####}&longitude={1:0.####}&current=temperature_2m,weather_code&timezone=auto",
                latitude, longitude);
        }

        public static bool TryParseCurrent(string json, out GardenWeatherReport report)
        {
            report = default;
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                var response = JsonUtility.FromJson<GardenWeatherApiResponse>(json);
                if (response == null || response.current == null || float.IsNaN(response.current.temperature_2m) || float.IsInfinity(response.current.temperature_2m))
                    return false;
                int code = response.current.weather_code;
                report = new GardenWeatherReport(FromWmoCode(code), code, response.current.temperature_2m);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static GardenWeatherKind FromWmoCode(int code)
        {
            if (code == 0 || code == 1) return GardenWeatherKind.Clear;
            if (code == 2) return GardenWeatherKind.PartlyCloudy;
            if (code == 3) return GardenWeatherKind.Overcast;
            if (code == 45 || code == 48) return GardenWeatherKind.Fog;
            if ((code >= 51 && code <= 67) || (code >= 80 && code <= 82)) return GardenWeatherKind.Rain;
            if ((code >= 71 && code <= 77) || code == 85 || code == 86) return GardenWeatherKind.Snow;
            if (code == 95 || code == 96 || code == 97 || code == 99) return GardenWeatherKind.Thunderstorm;
            return GardenWeatherKind.Unknown;
        }

        public static string DisplayName(GardenWeatherKind kind)
        {
            switch (kind)
            {
                case GardenWeatherKind.Clear: return "Clear";
                case GardenWeatherKind.PartlyCloudy: return "Partly cloudy";
                case GardenWeatherKind.Overcast: return "Overcast";
                case GardenWeatherKind.Fog: return "Fog";
                case GardenWeatherKind.Rain: return "Rain";
                case GardenWeatherKind.Snow: return "Snow";
                case GardenWeatherKind.Thunderstorm: return "Storm";
                default: return "Unknown";
            }
        }

        public static string HudSummary(GardenWeatherReport report)
        {
            return string.Format(CultureInfo.InvariantCulture, "WEATHER {0} {1:0}°C · {2}",
                DisplayName(report.Kind), report.TemperatureC, Attribution);
        }
    }
}
