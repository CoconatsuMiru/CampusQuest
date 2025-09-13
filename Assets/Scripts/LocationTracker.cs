using UnityEngine;
using TMPro;
using System.Collections;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class LocationTracker : MonoBehaviour
{
    public TextMeshProUGUI locationText;
    private float lastLat = 0f;
    private float lastLon = 0f;

    IEnumerator Start()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return null;
        }
#endif

        if (!Input.location.isEnabledByUser)
        {
            locationText.text = "Location services not enabled.";
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            locationText.text = "Location service timed out.";
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            locationText.text = "Unable to determine device location.";
            yield break;
        }

        // Start updating in coroutine
        StartCoroutine(TrackLocation());
    }

    IEnumerator TrackLocation()
    {
        while (true)
        {
            float currentLat = Input.location.lastData.latitude;
            float currentLon = Input.location.lastData.longitude;

            if (currentLat != lastLat || currentLon != lastLon)
            {
                lastLat = currentLat;
                lastLon = currentLon;
                locationText.text = $"Lat: {currentLat:F6}\nLon: {currentLon:F6}";
            }

            yield return new WaitForSeconds(0.1f); // Check 10 times per second
        }
    }
}
