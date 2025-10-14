using System.Collections;
using UnityEngine;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public static class LocationUtils
{
	public struct StartResult
	{
		public bool started;
		public string error;
	}

	public static IEnumerator EnsureLocationService(float desiredAccuracyInMeters = 10f, float updateDistanceInMeters = 1f, int timeoutSeconds = 20, System.Action<StartResult> onComplete = null)
	{
		#if PLATFORM_ANDROID
		if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
		{
			Permission.RequestUserPermission(Permission.FineLocation);
			// Give one frame for prompt; game should handle subsequent flow
			yield return null;
			if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
			{
				onComplete?.Invoke(new StartResult { started = false, error = "Location permission denied." });
				yield break;
			}
		}
		#endif

		if (!Input.location.isEnabledByUser)
		{
			onComplete?.Invoke(new StartResult { started = false, error = "Location services disabled by user." });
			yield break;
		}

		Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);

		int wait = timeoutSeconds;
		while (Input.location.status == LocationServiceStatus.Initializing && wait > 0)
		{
			yield return new WaitForSeconds(1);
			wait--;
		}

		if (wait <= 0)
		{
			onComplete?.Invoke(new StartResult { started = false, error = "Location service initialization timed out." });
			yield break;
		}

		if (Input.location.status == LocationServiceStatus.Failed)
		{
			onComplete?.Invoke(new StartResult { started = false, error = "Location service failed to start." });
			yield break;
		}

		onComplete?.Invoke(new StartResult { started = true, error = null });
	}

	public static bool TryGetLastLocation(out float latitude, out float longitude, out float horizontalAccuracy)
	{
		latitude = 0f;
		longitude = 0f;
		horizontalAccuracy = 0f;
		if (Input.location.status != LocationServiceStatus.Running)
		{
			return false;
		}
		var data = Input.location.lastData;
		latitude = data.latitude;
		longitude = data.longitude;
		horizontalAccuracy = data.horizontalAccuracy;
		return true;
	}
}


