using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Mapbox.Examples
{
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
#if UNITY_EDITOR
                // Force mock provider for editor testing
                if (_locationProvider == null)
                {
                    Debug.Log("[GPS] 🧭 Using TransformLocationProvider (mock GPS in Editor)");
                    _locationProvider = LocationProviderFactory.Instance.TransformLocationProvider;
                }
#else
                if (_locationProvider == null)
                {
                    Debug.Log("[GPS] 📱 Using DefaultLocationProvider (real GPS)");
                    _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
                }
#endif
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
            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log($"[GPS] ✅ Persistent GPS object created: {gameObject.name}");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SampleScene")
            {
                _isActiveInSampleScene = true;
                InitializeForSampleScene();
            }
            else
            {
                _isActiveInSampleScene = false;
            }
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
                else
                    Debug.LogWarning("[GPS] ⚠️ 'CurrentLocationTxt' not found in scene!");
            }

            // ✅ Force provider initialization now
            var provider = LocationProvider;
            provider.OnLocationUpdated += OnLocationUpdated;
        }

        private void OnLocationUpdated(Unity.Location.Location location)
        {
            Debug.Log($"[GPS] 📍 Location updated: {location.LatitudeLongitude}");
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

            if (currentLocation.IsLocationServiceEnabled)
            {
                // Some mock providers never set IsLocationUpdated to true, so we simulate it.
                if (!currentLocation.IsLocationUpdated && Application.isEditor)
                {
                    currentLocation.IsLocationUpdated = true;
                }

                if (currentLocation.IsLocationUpdated)
                {
                    string formattedText = $"📍 Lat: {currentLocation.LatitudeLongitude.x:F6}\n📍 Lng: {currentLocation.LatitudeLongitude.y:F6}";
                    if (locationText != null)
                        locationText.text = formattedText;

                    transform.localPosition = _map.GeoToWorldPosition(currentLocation.LatitudeLongitude);
                }
            }
            else
            {
                if (locationText != null)
                    locationText.text = "📡 Waiting for GPS signal...";
            }
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
