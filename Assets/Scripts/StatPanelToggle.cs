using UnityEngine;
using UnityEngine.UI;

public class StatsPanelToggle : MonoBehaviour
{
    [Header("🎯 Assign your Button here")]
    public Button toggleButton;

    [Header("🪄 Assign your Stats Panel here")]
    public GameObject statsPanel;

    private bool isOpen = false;

    private void Start()
    {
        if (toggleButton == null)
        {
            Debug.LogError("❌ Toggle Button not assigned!");
            return;
        }

        if (statsPanel == null)
        {
            Debug.LogError("❌ Stats Panel not assigned!");
            return;
        }

        // 🔹 Ensure panel starts hidden
        statsPanel.SetActive(false);

        // 🔹 Automatically hook up the button
        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(TogglePanel);
    }

    private void TogglePanel()
    {
        isOpen = !isOpen;
        statsPanel.SetActive(isOpen);

        Debug.Log(isOpen ? "📖 Stats Panel opened!" : "❌ Stats Panel closed!");
    }
}
