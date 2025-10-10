using UnityEngine;

public class StatPanelToggle : MonoBehaviour
{
    [Header("Assign your Stat Panel here")]
    public GameObject statPanel;

    private bool isOpen = false;

    public void TogglePanel()
    {
        if (statPanel == null)
        {
            Debug.LogWarning("⚠️ Stat Panel not assigned!");
            return;
        }

        isOpen = !isOpen;
        statPanel.SetActive(isOpen);
    }
}
