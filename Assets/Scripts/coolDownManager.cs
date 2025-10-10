using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CooldownManager : MonoBehaviour
{
    [Header("UI References")]
    public Button quizButton;
    public TMP_Text cooldownText;

    [Header("Settings")]
    [Tooltip("Cooldown duration in minutes")]
    public float cooldownMinutes = 2f;

    [Tooltip("Name of the quiz scene to load")]
    public string quizSceneName = "TextJSONTest"; // ✅ change if needed

    private const string LastQuizTimeKey = "LastQuizTime";
    private DateTime lastQuizTime;
    private bool isCooldownActive = false;

    void Start()
    {
        // 🔁 Load saved cooldown state
        if (PlayerPrefs.HasKey(LastQuizTimeKey))
        {
            string savedTime = PlayerPrefs.GetString(LastQuizTimeKey);
            if (DateTime.TryParse(savedTime, out lastQuizTime))
            {
                double minutesSinceLastQuiz = (DateTime.Now - lastQuizTime).TotalMinutes;

                if (minutesSinceLastQuiz < cooldownMinutes)
                {
                    isCooldownActive = true;
                    UpdateButtonState(false);
                }
                else
                {
                    // Cooldown done
                    isCooldownActive = false;
                    PlayerPrefs.DeleteKey(LastQuizTimeKey);
                    UpdateButtonState(true);
                }
            }
        }
        else
        {
            // First time — ready
            UpdateButtonState(true);
        }
    }

    void Update()
    {
        if (isCooldownActive)
        {
            double secondsLeft = (cooldownMinutes * 60) - (DateTime.Now - lastQuizTime).TotalSeconds;

            if (secondsLeft > 0)
            {
                int minutes = Mathf.FloorToInt((float)(secondsLeft / 60));
                int seconds = Mathf.FloorToInt((float)(secondsLeft % 60));
                cooldownText.text = $"{minutes:00}:{seconds:00}";
                quizButton.interactable = false;
            }
            else
            {
                // ✅ Cooldown done
                isCooldownActive = false;
                PlayerPrefs.DeleteKey(LastQuizTimeKey);
                UpdateButtonState(true);
            }
        }
    }

    public void OnQuizButtonClicked()
    {
        if (!isCooldownActive)
        {
            Debug.Log("🎯 Loading quiz scene...");
            SceneManager.LoadScene(quizSceneName); // ✅ Correct scene
        }
        else
        {
            Debug.Log("⏳ Cooldown still active!");
        }
    }

    // ✅ Called by the QuizManager when quiz ends
    public static void StartCooldown(float minutes)
    {
        if (!PlayerPrefs.HasKey(LastQuizTimeKey))
        {
            PlayerPrefs.SetString(LastQuizTimeKey, DateTime.Now.ToString());
            PlayerPrefs.Save();
        }
    }

    private void UpdateButtonState(bool ready)
    {
        if (ready)
        {
            cooldownText.text = "READY";
            quizButton.interactable = true;
        }
        else
        {
            quizButton.interactable = false;
        }
    }
}
