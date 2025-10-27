 using UnityEngine;
using UnityEngine.InputSystem; // Required for new Input System
using UnityEngine.SceneManagement;
using System.Collections; // ✅ Needed for IEnumerator

public class TransitionToQuizButtons : MonoBehaviour
{
    [Header("Scene Settings")]
    public string targetSceneName = "TextJSONTest";

    private Camera mainCamera;
    private bool isTransitioning = false;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Prevent multiple triggers
        if (isTransitioning) return;

        // Handle mouse click (for Editor testing)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckClick(Mouse.current.position.ReadValue());
        }

        // Handle touch input (for mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    void CheckClick(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform)
            {
                StartCoroutine(TransitionToScene());
            }
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

        // ✅ Check if PlayerBossStats exists
        if (PlayerBossStats.Instance == null)
        {
            Debug.LogWarning("⚠ PlayerBossStats instance not found, loading scene anyway.");
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }

        // ✅ Wait until PlayerBossStats is fully loaded
        while (!PlayerBossStats.Instance.isLoaded)
        {
            Debug.Log("⏳ Waiting for PlayerBossStats to finish loading...");
            yield return null;
        }

        Debug.Log("✅ PlayerBossStats ready — loading scene now!");
        SceneManager.LoadScene(targetSceneName);
    }
}