using UnityEngine;
using TMPro;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;

public class MainGameUIWithGPS : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelText;
    public TMP_Text xpText;
    public TMP_Text locationText;

    private ILocationProvider _locationProvider;
    private AbstractMap _map;
    private bool _isInitialized;

    void Start()
    {
        // Subscribe to GameManager updates
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsChanged += UpdateXPUI;
            UpdateXPUI();
        }

        // Initialize Mapbox
        if (LocationProviderFactory.Instance != null)
        {
            _map = LocationProviderFactory.Instance.mapManager;
            _map.OnInitialized += () => _isInitialized = true;

            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }

        // Ask for GPS permission on Android
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
        }
#endif
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStatsChanged -= UpdateXPUI;
    }

    void LateUpdate()
    {
        if (_isInitialized && _locationProvider != null)
        {
            var currentLocation = _locationProvider.CurrentLocation;
            double lat = currentLocation.LatitudeLongitude.x;
            double lng = currentLocation.LatitudeLongitude.y;

            // Update location text smoothly
            if (locationText != null)
            {
                locationText.text = $"📍 Lat: {lat:F6}\nLng: {lng:F6}";
            }
        }
        else if (locationText != null)
        {
            locationText.text = "📡 Locating...";
        }
    }

    private void UpdateXPUI()
    {
        if (GameManager.Instance == null) return;

        var gm = GameManager.Instance;

        if (levelText != null)
            levelText.text = $"Level: {gm.PlayerLevel}";

        if (xpText != null)
            xpText.text = $"XP: {gm.CurrentXP}/{gm.XpToNextLevel}";
    }
}
