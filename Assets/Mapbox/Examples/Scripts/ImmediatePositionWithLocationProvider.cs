namespace Mapbox.Examples
{
    using Mapbox.Unity.Location;
    using Mapbox.Unity.Map;
    using UnityEngine;
    using TMPro;   // ✅ Needed for UI text

    public class ImmediatePositionWithLocationProvider : MonoBehaviour
    {
        bool _isInitialized;

        ILocationProvider _locationProvider;
        ILocationProvider LocationProvider
        {
            get
            {
                if (_locationProvider == null)
                {
                    _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
                }

                return _locationProvider;
            }
        }

        Vector3 _targetPosition;

        [Header("UI Reference")]
        public TextMeshProUGUI locationText; // 👈 Assign in Inspector

        void Start()
        {
            // ✅ Request GPS permission on Android
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            }
#endif

            // ✅ iOS permissions are handled in Info.plist (set NSLocationWhenInUseUsageDescription)
            
            // Wait until map is initialized
            LocationProviderFactory.Instance.mapManager.OnInitialized += () => _isInitialized = true;
        }

        void LateUpdate()
        {
            if (_isInitialized)
            {
                var map = LocationProviderFactory.Instance.mapManager;

                // Get the current location
                var currentLocation = LocationProvider.CurrentLocation;

                // Check if we actually have a GPS fix
                if (currentLocation.IsLocationServiceEnabled && currentLocation.IsLocationUpdated)
                {
                    string formattedText = $"📍 Latitude: {currentLocation.LatitudeLongitude.x:F6}\n📍 Longitude: {currentLocation.LatitudeLongitude.y:F6}";
                    
                    if (locationText != null)
                        locationText.text = formattedText;

                    Debug.Log(formattedText);

                    // Move the GameObject
                    transform.localPosition = map.GeoToWorldPosition(currentLocation.LatitudeLongitude);
                }
                else
                {
                    // Show waiting message
                    if (locationText != null)
                        locationText.text = "📡 Waiting for GPS signal...";
                }
            }
        }
    }
}
