using System.Collections;
using UnityEngine;

public class GPSMovement : MonoBehaviour
{
    public MapboxTileManager tileManager;

    private Vector2 lastGPSPosition;
    private bool isReady = false;

    public float movementScale = 100000f;
    public float minMoveThreshold = 0.00001f;

    IEnumerator Start()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("GPS is not enabled by the user.");
            yield break;
        }

        Input.location.Start(1f, 0.1f);
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogError("Unable to start GPS.");
            yield break;
        }

        Debug.Log("GPS started.");
        lastGPSPosition = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
        isReady = true;

        tileManager.UpdateMap(lastGPSPosition.x, lastGPSPosition.y);
    }

    void Update()
    {
        if (!isReady || Input.location.status != LocationServiceStatus.Running)
            return;

        Vector2 currentGPS = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
        Vector2 delta = currentGPS - lastGPSPosition;

        if (delta.magnitude < minMoveThreshold)
            return;

        Vector3 movement = new Vector3(delta.x, 0, delta.y) * movementScale;
        transform.position += movement;

        lastGPSPosition = currentGPS;

        tileManager.UpdateMap(currentGPS.x, currentGPS.y);
    }
}
