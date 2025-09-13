using TMPro;
using UnityEngine;
using UnityEngine.UI;  // Add this to work with UI elements

public class LocationDisplay : MonoBehaviour
{
    public TMP_Text locationText;  // Reference to the UI Text element where you'll display coordinates

    void Start()
    {
        // Check if location services are enabled by the user
        if (Input.location.isEnabledByUser)
        {
            // Start the location service
            Input.location.Start();
        }
        else
        {
            Debug.Log("Location services not enabled by the user.");
        }
    }

    void Update()
    {
        // Check if the location service is running and valid
        if (Input.location.status == LocationServiceStatus.Running)
        {
            // Get the current latitude and longitude
            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;

            // Update the UI text with the latitude and longitude
            locationText.text = "Latitude: " + latitude.ToString("F6") + "\nLongitude: " + longitude.ToString("F6");
        }
    }
}
