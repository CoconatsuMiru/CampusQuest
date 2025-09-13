using UnityEngine;
using UnityEngine.InputSystem;

public class WayPointClicker : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Use Input System to check for mouse clicks
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
                foreach (Node node in allNodes)
                {
                    if (hit.transform == node.transform)
                    {
                        Debug.Log($"Clicked on {node.name}");
                        // Implement additional behavior for clicked waypoint
                        break;
                    }
                }
            }
        }
    }
}
