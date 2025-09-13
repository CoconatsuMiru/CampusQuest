using System.Collections;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class HardcodeLocation : MonoBehaviour
{
    public AbstractMap map;
    public Transform playerMarker;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f); // Let the map init

        Vector2d hardcodedLocation = new Vector2d(37.779238, -122.419359); // SF
        map.UpdateMap(hardcodedLocation);

        yield return new WaitForSeconds(0.5f); // Wait for map to finish updating

        Vector3 worldPosition = map.GeoToWorldPosition(hardcodedLocation, true);
        playerMarker.position = worldPosition;

        Debug.Log("Map updated and player moved.");
    }
}
