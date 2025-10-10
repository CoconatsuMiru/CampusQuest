using UnityEngine;
using UnityEngine.InputSystem; // ✅ New Input System

public class WayPointToggle : MonoBehaviour
{
    [Header("Assign your Waypoint Panel here")]
    public GameObject wayPointPanel;

    [Header("Proximity Settings")]
    public Transform player;          // ✅ Drag your Player here
    public float activationDistance = 3f;  // ✅ Distance to allow click

    private Camera mainCamera;
    private bool isOpen = false;
    private bool isPlayerNearby = false;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            isPlayerNearby = distance <= activationDistance;
        }

        // Handle mouse click (Editor / PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckClick(Mouse.current.position.ReadValue());
        }

        // Handle touch input (Mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void CheckClick(Vector2 screenPosition)
    {
        // ✅ Only clickable when player is nearby
        if (!isPlayerNearby)
        {
            Debug.Log("🚫 Too far from waypoint to interact!");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform)
            {
                TogglePanel();
            }
        }
    }

    private void TogglePanel()
    {
        if (wayPointPanel == null)
        {
            Debug.LogWarning("⚠️ Waypoint Panel not assigned!");
            return;
        }

        isOpen = !isOpen;
        wayPointPanel.SetActive(isOpen);
        Debug.Log(isOpen ? "📍 Waypoint panel opened!" : "📍 Waypoint panel closed!");
    }

    void OnDrawGizmosSelected()
    {
        // ✅ Draw a yellow sphere in the Editor to visualize range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}
