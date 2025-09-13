using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button waypointAButton;
    public Button waypointBButton;

    public GameObject player;

    public Transform waypointA;
    public Transform waypointB;

    public LineRenderer pathLineRenderer;

    private Transform activeWaypoint;

    void Start()
    {
        waypointAButton.onClick.AddListener(OnWaypointAClicked);
        waypointBButton.onClick.AddListener(OnWaypointBClicked);

        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
        }
    }

    void Update()
    {
        // Continuously update the path line to the active waypoint
        if (activeWaypoint != null && player != null && pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 2;
            pathLineRenderer.SetPosition(0, player.transform.position);
            pathLineRenderer.SetPosition(1, activeWaypoint.position);
        }
    }

    public void OnWaypointAClicked()
    {
        ToggleWaypoint(waypointA);
    }

    public void OnWaypointBClicked()
    {
        ToggleWaypoint(waypointB);
    }

    private void ToggleWaypoint(Transform target)
    {
        if (target == null || player == null || pathLineRenderer == null)
        {
            Debug.LogWarning("Missing references in ButtonManager.");
            return;
        }

        if (activeWaypoint == target)
        {
            // If clicked again, clear the path
            ClearPath();
        }
        else
        {
            // Set new waypoint
            activeWaypoint = target;
        }
    }

    public void ClearPath()
    {
        activeWaypoint = null;
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
        }
    }
}
