using UnityEngine;

namespace IdleGarden
{
    [CreateAssetMenu(menuName = "Idle Garden/Weather Settings")]
    public sealed class GardenWeatherSettings : ScriptableObject
    {
        [SerializeField] private string locationName = "Ho Chi Minh City";
        [SerializeField, Range(-90, 90)] private float latitude = 10.8231f;
        [SerializeField, Range(-180, 180)] private float longitude = 106.6297f;
        [SerializeField, Min(900)] private float refreshIntervalSeconds = 900;

        public string LocationName => locationName;
        public float Latitude => latitude;
        public float Longitude => longitude;
        public float RefreshIntervalSeconds => Mathf.Max(900, refreshIntervalSeconds);

        public bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(locationName) || float.IsNaN(latitude) || float.IsInfinity(latitude) || latitude < -90 || latitude > 90 ||
                float.IsNaN(longitude) || float.IsInfinity(longitude) || longitude < -180 || longitude > 180)
            {
                error = "Weather settings require a location name and valid coordinates.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
