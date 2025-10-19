using UnityEngine;
using UnityEngine.UI;

public class MonsterDexUIManager : MonoBehaviour
{
    [Header("Main Dex Panel")]
    public Button mainDexButton;      // 📖 Button to open/close the main Dex
    public GameObject mainDexPanel;   // 🗂️ Main Dex panel

    [Header("Tier Buttons and Panels")]
    public Button lowTierButton;      // 🟢 Low Tier button
    public GameObject lowTierPanel;   // 🟢 Low Tier panel

    public Button midTierButton;      // 🟡 Mid Tier button
    public GameObject midTierPanel;   // 🟡 Mid Tier panel

    public Button highTierButton;     // 🔴 High Tier button
    public GameObject highTierPanel;  // 🔴 High Tier panel

    private void Start()
    {
        // Hide all panels at start
        HideAllPanels();

        // 🔹 Hook up main Dex button
        if (mainDexButton != null)
            mainDexButton.onClick.AddListener(ToggleMainDex);

        // 🔹 Hook up tier buttons
        if (lowTierButton != null)
            lowTierButton.onClick.AddListener(() => OpenTierPanel(lowTierPanel));
        if (midTierButton != null)
            midTierButton.onClick.AddListener(() => OpenTierPanel(midTierPanel));
        if (highTierButton != null)
            highTierButton.onClick.AddListener(() => OpenTierPanel(highTierPanel));
    }

    private void HideAllPanels()
    {
        if (mainDexPanel != null) mainDexPanel.SetActive(false);
        if (lowTierPanel != null) lowTierPanel.SetActive(false);
        if (midTierPanel != null) midTierPanel.SetActive(false);
        if (highTierPanel != null) highTierPanel.SetActive(false);
    }

    private bool AnyTierOpen()
    {
        return (lowTierPanel != null && lowTierPanel.activeSelf) ||
               (midTierPanel != null && midTierPanel.activeSelf) ||
               (highTierPanel != null && highTierPanel.activeSelf);
    }

    // 🔸 Toggles the Main Dex
    private void ToggleMainDex()
    {
        // If any tier panel is open → close everything
        if (AnyTierOpen())
        {
            HideAllPanels();
            Debug.Log("📕 Closed all panels (tier panel was open)");
            return;
        }

        // Otherwise toggle main Dex normally
        bool isActive = mainDexPanel.activeSelf;
        HideAllPanels();
        mainDexPanel.SetActive(!isActive);
        Debug.Log($"📖 Main Dex toggled: {!isActive}");
    }

    // 🔸 Opens a specific Tier panel and closes the main Dex
    private void OpenTierPanel(GameObject panelToOpen)
    {
        if (panelToOpen == null) return;

        HideAllPanels();
        panelToOpen.SetActive(true);
        Debug.Log($"🎨 Opened {panelToOpen.name}");
    }
}
