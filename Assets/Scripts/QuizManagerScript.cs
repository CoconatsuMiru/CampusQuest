using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Firebase.Database;

public class QuizManagerScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizPanel;
    public TMP_Text questionText;
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public TMP_Text questionProgressText;
    public TMP_Text scoreText;

    [Header("XP & Level UI (optional inside quiz)")]
    public TMP_Text levelText;
    public TMP_Text xpText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    [Header("Quiz Configuration")]
    public TextAsset quizJSON; // ✅ Drag your JSON file here (per subject)
    public string subjectStatKey = "stat_06_art"; // ✅ Example: stat_06_art, stat_08_math, etc.

    private List<Question> questions = new List<Question>();
    private Question currentQuestion;
    private string selectedCorrectAnswer;

    private int score = 0;
    private int currentQuestionIndex = 0;

    private const int MaxQuestionCount = 5;
    private DatabaseReference dbReference;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadQuestions();
        StartQuiz();
        UpdateXPUI();
    }

    void LoadQuestions()
    {
        if (quizJSON == null)
        {
            Debug.LogError("❌ No quiz JSON file assigned in the inspector!");
            return;
        }

        QuizData quizData = JsonUtility.FromJson<QuizData>(quizJSON.text);
        if (quizData != null && quizData.questions != null)
        {
            var enabledQuestions = quizData.questions.Where(q => q.enabled == 1);
            questions.AddRange(enabledQuestions);
        }

        // ✅ Randomize once only
        questions = questions.OrderBy(q => Random.value).Take(MaxQuestionCount).ToList();
    }

    public void StartQuiz()
    {
        if (questions.Count > 0)
        {
            quizPanel.SetActive(true);
            score = 0;
            currentQuestionIndex = 0;
            UpdateScoreUI();
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.LogError("❌ No questions available.");
            quizPanel.SetActive(false);
        }
    }

    void ShowQuestion(int index)
    {
        if (index >= questions.Count)
        {
            EndQuiz();
            return;
        }

        currentQuestion = questions[index];
        questionText.text = currentQuestion.question;
        selectedCorrectAnswer = currentQuestion.correctAnswer;

        List<string> shuffledAnswers = new List<string>(currentQuestion.answers);
        shuffledAnswers = shuffledAnswers.OrderBy(a => Random.value).ToList();

        answerButton1.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[0];
        answerButton2.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[1];
        answerButton3.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[2];

        // ✅ Sequential question number display
        questionProgressText.text = $"{index + 1}/{MaxQuestionCount}";

        // Re-enable buttons each round
        answerButton1.interactable = true;
        answerButton2.interactable = true;
        answerButton3.interactable = true;
    }

    public void OnAnswerButtonClicked(int buttonIndex)
    {
        string selectedAnswer = "";

        if (buttonIndex == 0) selectedAnswer = answerButton1.GetComponentInChildren<TMP_Text>().text;
        if (buttonIndex == 1) selectedAnswer = answerButton2.GetComponentInChildren<TMP_Text>().text;
        if (buttonIndex == 2) selectedAnswer = answerButton3.GetComponentInChildren<TMP_Text>().text;

        // ✅ Disable buttons temporarily to prevent double-clicks
        DisableButtons();

        if (selectedAnswer == selectedCorrectAnswer)
        {
            score++;
            UpdateScoreUI();
            if (correctSound != null && audioSource != null)
                audioSource.PlayOneShot(correctSound);
        }
        else
        {
            if (incorrectSound != null && audioSource != null)
                audioSource.PlayOneShot(incorrectSound);
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        // ✅ Move sequentially
        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
            ShowQuestion(currentQuestionIndex);
        else
            EndQuiz();
    }

    void EndQuiz()
    {
        Debug.Log($"🎉 Quiz complete! Final score: {score}/{MaxQuestionCount}");
        quizPanel.SetActive(true);
        DisableButtons();

        // ✅ Update Firebase stat for this subject
        StartCoroutine(UpdateFirebaseStat(score));

        // ✅ Return to main menu
        StartCoroutine(WaitAndLoadMainScene(3f));
    }

    IEnumerator UpdateFirebaseStat(int amount)
    {
        string userId = FirebaseAuthManager.LoggedInUserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ Cannot update stat — no logged-in user ID found!");
            yield break;
        }

        var statRef = dbReference.Child("users").Child(userId).Child(subjectStatKey);
        var statTask = statRef.GetValueAsync();

        yield return new WaitUntil(() => statTask.IsCompleted);

        if (statTask.Exception != null)
        {
            Debug.LogError("⚠️ Failed to fetch stat: " + statTask.Exception);
            yield break;
        }

        int currentStatValue = 0;
        if (statTask.Result.Value != null)
            int.TryParse(statTask.Result.Value.ToString(), out currentStatValue);

        int newStatValue = currentStatValue + amount;
        yield return statRef.SetValueAsync(newStatValue);

        Debug.Log($"✅ Updated {subjectStatKey}: {currentStatValue} → {newStatValue}");
    }

    void DisableButtons()
    {
        answerButton1.interactable = false;
        answerButton2.interactable = false;
        answerButton3.interactable = false;
    }

    IEnumerator WaitAndLoadMainScene(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene("SampleScene");
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Score: {score}";
    }

    void UpdateXPUI()
    {
        if (levelText != null) levelText.text = "Level: --";
        if (xpText != null) xpText.text = "XP: --";
    }

    [System.Serializable]
    public class Question
    {
        public string question;
        public string[] answers;
        public string correctAnswer;
        public int enabled = 1;
    }

    [System.Serializable]
    public class QuizData
    {
        public List<Question> questions;
    }
}
