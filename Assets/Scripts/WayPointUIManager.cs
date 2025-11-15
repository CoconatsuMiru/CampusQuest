using UnityEngine;
using TMPro;

public class WaypointUIManager : MonoBehaviour
{
    public static WaypointUIManager Instance;

    [Header("Waypoint UI Groups")]
    public WaypointUI[] waypointUIs;  // list of UI sets for each waypoint

    [Header("General UI")]
    public GameObject tooFarPanel;     // shown when player is too far

    void Awake()
    {
        Instance = this;
    }

    // Called when player is too far
    public void ShowTooFarMessage()
    {
        tooFarPanel.SetActive(true);
        CancelInvoke(nameof(HideTooFarMessage));
        Invoke(nameof(HideTooFarMessage), 2f); // auto-hide after 2 sec
    }

    private void HideTooFarMessage()
    {
        tooFarPanel.SetActive(false);
    }

    // Called when clicking a waypoint and player is close enough
    public void ShowUIForWaypoint(int waypointID)
    {
        // Hide all panels first
        foreach (var ui in waypointUIs)
        {
            ui.confirmationPanel.SetActive(false);
            ui.enteredZoneMessage.gameObject.SetActive(false);
        }

        // Show the correct one
        if (waypointID < waypointUIs.Length)
        {
            WaypointUI ui = waypointUIs[waypointID];

            ui.confirmationPanel.SetActive(true);
            ui.enteredZoneMessage.gameObject.SetActive(true);

            // auto-hide the zone welcome message
            CancelInvoke(nameof(HideWelcome));
            Invoke(nameof(HideWelcome), 2f);
        }
    }

    void HideWelcome()
    {
        foreach (var ui in waypointUIs)
            ui.enteredZoneMessage.gameObject.SetActive(false);
    }
}

[System.Serializable]
public struct WaypointUI   // <<< using struct so Unity NEVER treats it as a component
{
    public GameObject confirmationPanel;
    public TMP_Text enteredZoneMessage;
}
