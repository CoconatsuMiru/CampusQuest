using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveManager : MonoBehaviour
{
    public static PlayerSaveManager Instance;

    private Vector3 savedPosition;
    private string savedScene;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved data from PlayerPrefs (optional persistence)
        if (PlayerPrefs.HasKey("posX"))
        {
            float x = PlayerPrefs.GetFloat("posX");
            float y = PlayerPrefs.GetFloat("posY");
            float z = PlayerPrefs.GetFloat("posZ");
            savedPosition = new Vector3(x, y, z);

            Debug.Log($"[PlayerSaveManager] Loaded saved position {savedPosition}");
        }

        if (PlayerPrefs.HasKey("scene"))
        {
            savedScene = PlayerPrefs.GetString("scene");
            Debug.Log($"[PlayerSaveManager] Loaded saved scene {savedScene}");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Save the player's position and scene.
    /// </summary>
    public void SavePlayerLocation(Vector3 position, string sceneName)
    {
        savedPosition = position;
        savedScene = sceneName;

        PlayerPrefs.SetFloat("posX", position.x);
        PlayerPrefs.SetFloat("posY", position.y);
        PlayerPrefs.SetFloat("posZ", position.z);
        PlayerPrefs.SetString("scene", sceneName);
        PlayerPrefs.Save();

        Debug.Log($"[PlayerSaveManager] SAVED {position} in scene {sceneName}");
    }

    /// <summary>
    /// Called automatically when a new scene loads.
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PlayerSaveManager] Scene loaded: {scene.name}, savedScene={savedScene}");

        if (scene.name == savedScene)
        {
            StartCoroutine(RestoreNextFrame(scene));
        }
    }

    /// <summary>
    /// Waits one frame so the Player is fully spawned before moving them.
    /// </summary>
    private System.Collections.IEnumerator RestoreNextFrame(Scene scene)
    {
        yield return null; // wait one frame

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = savedPosition;
            Debug.Log($"[PlayerSaveManager] RESTORED player to {savedPosition} in {scene.name}");
        }
        else
        {
            Debug.LogWarning("[PlayerSaveManager] Player not found in scene!");
        }
    }
}
