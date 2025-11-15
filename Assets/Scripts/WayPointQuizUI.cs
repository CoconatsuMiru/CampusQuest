using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaypointQuizUI : MonoBehaviour
{
    [Header("Waypoint UI")]
    public GameObject panelRoot;
    public Button yesButton;
    public Button noButton;
    public TMP_Text cooldownText;

    [Header("Target Quiz Scene")]
    public string quizSceneName = "TextJSONTest";

    void OnEnable()
    {
        UpdateUI();
    }

    void Update()
    {
        if (CooldownManager.Instance == null) return;

        if (CooldownManager.Instance.IsCooldownActive)
        {
            double sec = CooldownManager.Instance.GetRemainingSeconds();
            int m = Mathf.FloorToInt((float)(sec / 60));
            int s = Mathf.FloorToInt((float)(sec % 60));

            cooldownText.text = $"{m:00}:{s:00}";
            yesButton.interactable = false;
        }
        else
        {
            cooldownText.text = "READY";
            yesButton.interactable = true;
        }
    }

    void UpdateUI()
    {
        if (CooldownManager.Instance.IsCooldownActive)
        {
            yesButton.interactable = false;
        }
        else
        {
            yesButton.interactable = true;
        }
    }

    // YES button
    public void OnClickYes()
    {
        if (!CooldownManager.Instance.IsCooldownActive)
        {
            SceneManager.LoadScene(quizSceneName);
        }
    }

    // NO button
    public void OnClickNo()
    {
        panelRoot.SetActive(false);
    }
}
