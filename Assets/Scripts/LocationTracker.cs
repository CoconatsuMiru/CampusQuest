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
        bool done = false;
        string errorMsg = null;
        yield return LocationUtils.EnsureLocationService(10f, 1f, 20, (result) =>
        {
            done = true;
            if (!result.started)
            {
                errorMsg = result.error;
            }
        });

        if (!done || !string.IsNullOrEmpty(errorMsg))
        {
            locationText.text = string.IsNullOrEmpty(errorMsg) ? "Location service failed." : errorMsg;
            yield break;
        }

        StartCoroutine(TrackLocation());
    }

    IEnumerator TrackLocation()
    {
        while (true)
        {
            float currentLat;
            float currentLon;
            float acc;
            if (!LocationUtils.TryGetLastLocation(out currentLat, out currentLon, out acc))
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (currentLat != lastLat || currentLon != lastLon)
            {
                lastLat = currentLat;
                lastLon = currentLon;
                locationText.text = $"Lat: {currentLat:F6}\nLon: {currentLon:F6}\nAcc: ±{acc:F1} m";
            }

            yield return new WaitForSeconds(0.1f); // Check 10 times per second
        }
    }
}
