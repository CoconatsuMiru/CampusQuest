using UnityEngine;
using UnityEngine.SceneManagement;

public class MapColliderDisabler : MonoBehaviour
{
    [Tooltip("Only enable colliders when inside this scene name")]
    public string mainSceneName = "SampleScene"; // change this to your map scene name

    private Collider[] mapColliders;

    void Awake()
    {
        // Find all colliders in the current scene (e.g., map markers, POIs, etc.)
        mapColliders = FindObjectsOfType<Collider>(true);
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene previous, Scene next)
    {
        bool enableColliders = next.name == mainSceneName;
        ToggleMapColliders(enableColliders);
    }

    private void ToggleMapColliders(bool enable)
    {
        if (mapColliders == null) return;

        foreach (var col in mapColliders)
        {
            if (col != null)
                col.enabled = enable;
        }

        Debug.Log($"[MapColliderDisabler] Colliders {(enable ? "ENABLED" : "DISABLED")} for scene: {SceneManager.GetActiveScene().name}");
    }
}

