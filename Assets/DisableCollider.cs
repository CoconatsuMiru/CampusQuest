using UnityEngine;
using UnityEngine.SceneManagement;

public class DisableCollider : MonoBehaviour
{
    [Tooltip("Name of the scene where this collider should stay enabled")]
    public string mainSceneName = "SampleScene";

    private Collider myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider == null)
        {
            Debug.LogWarning($"[DisableColliderOutsideMainScene] No Collider found on {gameObject.name}");
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene previous, Scene next)
    {
        if (myCollider == null) return;

        bool shouldEnable = next.name == mainSceneName;
        myCollider.enabled = shouldEnable;

        Debug.Log($"[DisableColliderOutsideMainScene] Collider {(shouldEnable ? "ENABLED" : "DISABLED")} on {gameObject.name} (Scene: {next.name})");
    }
}

