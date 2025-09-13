using UnityEngine;
using UnityEngine.InputSystem; // Required for new Input System
using UnityEngine.SceneManagement;

public class WaypointTapToScene : MonoBehaviour
{
    [Header("Scene Settings")]
    public string targetSceneName = "BossScene"; // Set the boss scene in Inspector

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
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
            // Only react if THIS waypoint was tapped
            if (hit.transform == transform)
            {
                Debug.Log($"[Waypoint {name}] tapped. Loading scene: {targetSceneName}");
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}
