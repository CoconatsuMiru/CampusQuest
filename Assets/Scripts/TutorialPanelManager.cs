using UnityEngine;

public class TutorialPanelManager : MonoBehaviour
{
    [Header("References")]
    public GameObject tutorialPanel; // 👈 Drag your Tutorial Panel here in the Inspector

    [Header("Buttons")]
    public GameObject helpButton; // 👈 The "?" button in your UI
    public GameObject gotItButton; // 👈 The "G!" button inside the tutorial panel

    private void Start()
    {
        // Hide tutorial panel on start
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Add listeners to both buttons
        if (helpButton != null)
            helpButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenTutorial);

        if (gotItButton != null)
            gotItButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CloseTutorial);
    }

    // Opens the tutorial panel
    public void OpenTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    // Closes the tutorial panel
    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
}
