using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag ALL your tutorial panels here in order (Panel1, Panel2, Panel3, etc.)")]
    public GameObject[] tutorialPanels;

    [Header("Buttons")]
    [Tooltip("Button that opens the tutorial (the '?' icon)")]
    public Button helpButton;
    [Tooltip("The 'Got It' button that closes the tutorial (on last panel)")]
    public Button gotItButton;

    private int currentPanelIndex = 0;

    private void Start()
    {
        // Hide all tutorial panels at start
        foreach (var panel in tutorialPanels)
        {
            panel.SetActive(false);
        }

        if (helpButton != null)
            helpButton.onClick.AddListener(OpenTutorial);

        if (gotItButton != null)
            gotItButton.onClick.AddListener(CloseTutorial);

        // Hide gotIt button initially
        if (gotItButton != null)
            gotItButton.gameObject.SetActive(false);
    }

    public void OpenTutorial()
    {
        currentPanelIndex = 0;
        ShowPanel(currentPanelIndex);
    }

    public void CloseTutorial()
    {
        foreach (var panel in tutorialPanels)
        {
            panel.SetActive(false);
        }
        if (gotItButton != null)
            gotItButton.gameObject.SetActive(false);
    }

    public void NextPanel()
    {
        if (currentPanelIndex < tutorialPanels.Length - 1)
        {
            currentPanelIndex++;
            ShowPanel(currentPanelIndex);
        }
    }

    public void PreviousPanel()
    {
        if (currentPanelIndex > 0)
        {
            currentPanelIndex--;
            ShowPanel(currentPanelIndex);
        }
    }

    private void ShowPanel(int index)
    {
        for (int i = 0; i < tutorialPanels.Length; i++)
        {
            tutorialPanels[i].SetActive(i == index);
        }

        // 🔍 Try to find buttons inside the current panel
        Button nextButton = tutorialPanels[index].GetComponentInChildren<Button>(true);
        Button[] panelButtons = tutorialPanels[index].GetComponentsInChildren<Button>(true);

        // Reset all buttons in the panel (hide by default)
        foreach (Button b in panelButtons)
        {
            b.gameObject.SetActive(false);
        }

        // Enable the correct buttons based on the panel index
        if (index == 0)
        {
            // Panel 1 → only "Next"
            Button next = FindButtonInPanel(tutorialPanels[index], "Next");
            if (next != null)
            {
                next.gameObject.SetActive(true);
                next.onClick.RemoveAllListeners();
                next.onClick.AddListener(NextPanel);
            }
        }
        else if (index == tutorialPanels.Length - 1)
        {
            // Panel 3 → only "Previous" + "Got It"
            Button prev = FindButtonInPanel(tutorialPanels[index], "Previous");
            if (prev != null)
            {
                prev.gameObject.SetActive(true);
                prev.onClick.RemoveAllListeners();
                prev.onClick.AddListener(PreviousPanel);
            }

            if (gotItButton != null)
                gotItButton.gameObject.SetActive(true);
        }
        else
        {
            // Panel 2 → "Previous" + "Next"
            Button next = FindButtonInPanel(tutorialPanels[index], "Next");
            Button prev = FindButtonInPanel(tutorialPanels[index], "Previous");

            if (prev != null)
            {
                prev.gameObject.SetActive(true);
                prev.onClick.RemoveAllListeners();
                prev.onClick.AddListener(PreviousPanel);
            }

            if (next != null)
            {
                next.gameObject.SetActive(true);
                next.onClick.RemoveAllListeners();
                next.onClick.AddListener(NextPanel);
            }

            if (gotItButton != null)
                gotItButton.gameObject.SetActive(false);
        }
    }

    private Button FindButtonInPanel(GameObject panel, string buttonName)
    {
        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.name.ToLower().Contains(buttonName.ToLower()))
                return btn;
        }
        return null;
    }
}
