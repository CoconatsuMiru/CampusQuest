using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signUpPanel;

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }

    public void ShowSignUpPanel()
    {
        signUpPanel.SetActive(true);
        loginPanel.SetActive(false);
    }

    public void CloseAllPanels()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
    }
}
