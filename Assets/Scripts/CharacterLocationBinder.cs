using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Mapbox.Utils;

public class CharacterLocationBinder : MonoBehaviour
{
    [SerializeField] AbstractMap map;
    [SerializeField] GameObject playerPrefab;

    private GameObject playerInstance;
    private ILocationProvider _locationProvider;

    void Start()
    {
        // Get the location provider from Mapbox
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

        // Subscribe to updates
        _locationProvider.OnLocationUpdated += OnLocationUpdated;

        // Spawn the player initially at origin
        playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    }

    private void OnLocationUpdated(Location location)
    {
        // Convert GPS coords to map world position
        Vector2d latLong = location.LatitudeLongitude;
        Vector3 mapPos = map.GeoToWorldPosition(latLong, true);

        playerInstance.transform.position = mapPos;
    }
}
