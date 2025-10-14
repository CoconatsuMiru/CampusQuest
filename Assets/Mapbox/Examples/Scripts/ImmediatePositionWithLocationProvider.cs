namespace Mapbox.Examples
{
    using Mapbox.Unity.Location;
    using Mapbox.Unity.Map;
    using UnityEngine;
    using TMPro;
    using UnityEngine.SceneManagement;

    public class ImmediatePositionWithLocationProvider : MonoBehaviour
    {
        private static ImmediatePositionWithLocationProvider _instance;
        private bool _isInitialized;
        private bool _isActiveInSampleScene;
        private ILocationProvider _locationProvider;
        private AbstractMap _map;

        [Header("UI Reference")]
        public TextMeshProUGUI locationText;

        private ILocationProvider LocationProvider
        {
            get
            {
                if (_locationProvider == null)
                    _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
                return _locationProvider;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);


            Debug.Log($"[GPS] ✅ Persistent GPS object created: {gameObject.name}");
        }



        private void InitializeForSampleScene()
        {
            Debug.Log("[GPS] 🔄 Scene is SampleScene — initializing GPS and map...");

            _map = FindObjectOfType<AbstractMap>();
            if (_map != null)
            {
                _map.OnInitialized += () =>
                {
                    _isInitialized = true;
                    Debug.Log("[GPS] 🗺️ Map successfully linked to GPS!");
                };
            }
            else
            {
                Debug.LogWarning("[GPS] ⚠️ No AbstractMap found in SampleScene!");
            }

            if (locationText == null)
            {
                var foundText = GameObject.Find("CurrentLocationTxt");
                if (foundText != null)
                    locationText = foundText.GetComponent<TextMeshProUGUI>();
            }
        }

        void Start()
        {
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
#endif

            if (SceneManager.GetActiveScene().name == "SampleScene")
                InitializeForSampleScene();
        }

        void LateUpdate()
        {
            if (!_isActiveInSampleScene || !_isInitialized || _map == null)
                return;

            var currentLocation = LocationProvider.CurrentLocation;

            if (currentLocation.IsLocationServiceEnabled && currentLocation.IsLocationUpdated)
            {
                string formattedText = $"📍 Lat: {currentLocation.LatitudeLongitude.x:F6}\n📍 Lng: {currentLocation.LatitudeLongitude.y:F6}";
                if (locationText != null)
                    locationText.text = formattedText;

                transform.localPosition = _map.GeoToWorldPosition(currentLocation.LatitudeLongitude);
            }
            else
            {
                if (locationText != null)
                    locationText.text = "📡 Waiting for GPS signal...";
            }
        }
    }
}
