using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class GardenWeatherSync : MonoBehaviour
    {
        [SerializeField] private GardenWeatherSettings settings;
        [SerializeField] private Text statusLabel;
        private bool hasSnapshot;
        private string lastSummary;

        private void Start()
        {
            if (!settings || !statusLabel)
            {
                Debug.LogWarning("Weather sync needs settings and a status label.", this);
                return;
            }
            if (!settings.Validate(out string error))
            {
                statusLabel.text = "WEATHER unavailable · Open-Meteo";
                Debug.LogWarning("Weather sync disabled: " + error, this);
                return;
            }

            statusLabel.text = "WEATHER loading · Open-Meteo";
            StartCoroutine(PollWeather());
        }

        private IEnumerator PollWeather()
        {
            while (true)
            {
                yield return FetchWeather();
                yield return new WaitForSecondsRealtime(settings.RefreshIntervalSeconds);
            }
        }

        private IEnumerator FetchWeather()
        {
            string url = GardenWeatherClient.BuildRequestUrl(settings.Latitude, settings.Longitude);
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 8;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    ShowUnavailableOrStale();
                    Debug.LogWarning("Weather sync failed: " + request.error, this);
                    yield break;
                }
                if (!GardenWeatherClient.TryParseCurrent(request.downloadHandler.text, out GardenWeatherReport report))
                {
                    ShowUnavailableOrStale();
                    Debug.LogWarning("Weather sync returned an invalid response.", this);
                    yield break;
                }

                lastSummary = GardenWeatherClient.HudSummary(report);
                hasSnapshot = true;
                statusLabel.text = lastSummary;
            }
        }

        private void ShowUnavailableOrStale()
        {
            statusLabel.text = hasSnapshot ? lastSummary + " · stale" : "WEATHER unavailable · Open-Meteo";
        }
    }
}
