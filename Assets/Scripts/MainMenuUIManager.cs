using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;   // ✅ new reference for the main menu
    public GameObject loginPanel;
    public GameObject signUpPanel;

    void Start()
    {
        // Safety check: if no panel is active, default to main menu
        if (!mainMenuPanel.activeSelf && !loginPanel.activeSelf && !signUpPanel.activeSelf)
        {
            ShowMainMenu();
        }
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
    }

    public void ShowLoginPanel()
    {
        mainMenuPanel.SetActive(false);
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }

    public void ShowSignUpPanel()
    {
        mainMenuPanel.SetActive(false);
        signUpPanel.SetActive(true);
        loginPanel.SetActive(false);
    }

    public void CloseAllPanels()
    {
        // Instead of hiding everything, always fall back to Main Menu
        ShowMainMenu();
    }
}
