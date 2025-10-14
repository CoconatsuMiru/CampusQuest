using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public AbstractMap map;
    public ILocationProvider locationProvider;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        map = GetComponent<AbstractMap>();
        locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene previous, Scene next)
    {
        if (next.name == "SampleScene")
        {
            EnableMapColliders();
        }
        else
        {
            DisableMapColliders();
        }
    }

    private void DisableMapColliders()
    {
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

        Debug.Log("[MapManager] 🚫 Disabled map colliders (quiz or non-main scene)");
    }

    private void EnableMapColliders()
    {
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            col.enabled = true;
        }

        Debug.Log("[MapManager] ✅ Re-enabled map colliders for SampleScene");
    }
}
