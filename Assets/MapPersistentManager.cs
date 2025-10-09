using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using UnityEngine.SceneManagement;

public class MapPersistentManager : MonoBehaviour
{
    private static MapPersistentManager _instance;

    void Awake()
    {
        // ✅ Only enforce singleton during actual play mode
        if (Application.isPlaying)
        {
            if (_instance != null && _instance != this)
            {
                Debug.Log("[MapPersistentManager] Duplicate runtime map detected — destroying this one.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        var mapManager = GetComponent<AbstractMap>();
        if (mapManager != null)
        {
            LocationProviderFactory.Instance.mapManager = mapManager;
            Debug.Log("[MapPersistentManager] ✅ Map marked as persistent and linked to LocationProviderFactory.");
        }
        else
        {
            Debug.LogWarning("[MapPersistentManager] ⚠️ No AbstractMap component found on this GameObject!");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var mapManager = GetComponent<AbstractMap>();

        if (mapManager != null)
        {
            LocationProviderFactory.Instance.mapManager = mapManager;
            Debug.Log($"[MapPersistentManager] 🔄 Scene changed to '{scene.name}', map still active.");
        }
    }

#if UNITY_EDITOR
    // ✅ Reset static instance when exiting play mode in the Editor
    [UnityEditor.InitializeOnLoadMethod]
    private static void ResetOnExitPlaymode()
    {
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                _instance = null;
            }
        };
    }
#endif
}
