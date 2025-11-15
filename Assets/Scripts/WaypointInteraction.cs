using UnityEngine;
using UnityEngine.InputSystem;

public class WaypointButton : MonoBehaviour
{
    public int waypointID;
    public float interactionDistance = 20f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleClick();
    }

    void HandleClick()
    {
        // Mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckClick(Mouse.current.position.ReadValue());
        }

        // Touch
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    void CheckClick(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.transform == transform)
            {
                TryInteract();
            }
        }
    }

    public void TryInteract()
    {
        if (PlayerPosition.Instance == null)
        {
            Debug.LogError("No PlayerPosition.Instance found in scene!");
            return;
        }

        float dist = Vector3.Distance(
            PlayerPosition.Instance.transform.position,
            transform.position
        );

        if (dist > interactionDistance)
        {
            WaypointUIManager.Instance.ShowTooFarMessage();
            return;
        }

        WaypointUIManager.Instance.ShowUIForWaypoint(waypointID);
    }
}
