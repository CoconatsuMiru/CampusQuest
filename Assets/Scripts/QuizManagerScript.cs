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
    public TMP_Text questionText;
    public Button answerButtonA;
    public Button answerButtonB;
    public Button answerButtonC;
    public Button answerButtonD;
    public TMP_Text scoreText;
    public GameObject correctPanel;

    [Header("XP & Level UI (optional)")]
    public TMP_Text levelText;
    public TMP_Text xpText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    [Header("Quiz Configuration")]
    public TextAsset quizJSON;

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
            Debug.LogError("❌ No quiz JSON file assigned!");
            return;
        }

        QuizData quizData = JsonUtility.FromJson<QuizData>(quizJSON.text);
        if (quizData != null && quizData.questions != null)
        {
            var enabledQuestions = quizData.questions.Where(q => q.enabled == 1);
            questions.AddRange(enabledQuestions);
        }

        questions = questions.OrderBy(q => Random.value).Take(MaxQuestionCount).ToList();
    }

    public void StartQuiz()
    {
        if (questions.Count > 0)
        {
            score = 0;
            currentQuestionIndex = 0;
            UpdateScoreUI();
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.LogError("❌ No questions available.");
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

        answerButtonA.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[0];
        answerButtonB.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[1];
        answerButtonC.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[2];
        answerButtonD.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[3];

        correctPanel.SetActive(false);
        EnableButtons();
    }

    public void OnAnswerButtonClicked(int buttonIndex)
    {
        string selectedAnswer = "";

        switch (buttonIndex)
        {
            case 0: selectedAnswer = answerButtonA.GetComponentInChildren<TMP_Text>().text; break;
            case 1: selectedAnswer = answerButtonB.GetComponentInChildren<TMP_Text>().text; break;
            case 2: selectedAnswer = answerButtonC.GetComponentInChildren<TMP_Text>().text; break;
            case 3: selectedAnswer = answerButtonD.GetComponentInChildren<TMP_Text>().text; break;
        }

        DisableButtons();

        if (selectedAnswer == selectedCorrectAnswer)
        {
            score++;
            UpdateScoreUI();
            correctPanel.SetActive(true);
            correctPanel.GetComponentInChildren<TMP_Text>().text = "✅ Correct!";
            if (correctSound) audioSource.PlayOneShot(correctSound);
        }
        else
        {
            correctPanel.SetActive(true);
            correctPanel.GetComponentInChildren<TMP_Text>().text = "❌ Wrong!";
            if (incorrectSound) audioSource.PlayOneShot(incorrectSound);
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        currentQuestionIndex++;
        StartCoroutine(NextQuestionWithDelay());
    }

    IEnumerator NextQuestionWithDelay()
    {
        yield return new WaitForSeconds(1.5f);

        if (currentQuestionIndex < questions.Count)
            ShowQuestion(currentQuestionIndex);
        else
            EndQuiz();
    }

    void EndQuiz()
    {
        correctPanel.SetActive(true);
        correctPanel.GetComponentInChildren<TMP_Text>().text = $"🎉 Quiz Complete!\nScore: {score}/{MaxQuestionCount}";
        DisableButtons();

        // ✅ If player perfected the quiz, give reward (+1 to all stats)
        if (score == MaxQuestionCount)
        {
            Debug.Log("🏅 Perfect score! Granting +1 to all stats...");
            StartCoroutine(UpdateAllStatsReward());
        }

        StartCoroutine(WaitAndLoadMainScene(3f));
    }

    IEnumerator UpdateAllStatsReward()
    {
        string userId = FirebaseAuthManager.LoggedInUserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ Cannot update stats — no logged-in user ID found!");
            yield break;
        }

        string[] statKeys = {
            "stat_01_math",
            "stat_02_science",
            "stat_03_english",
            "stat_04_art",
            "stat_05_music",
            "stat_06_history"
        };

        foreach (string key in statKeys)
        {
            var statRef = dbReference.Child("users").Child(userId).Child(key);
            var statTask = statRef.GetValueAsync();
            yield return new WaitUntil(() => statTask.IsCompleted);

            if (statTask.Exception != null)
            {
                Debug.LogError("⚠️ Failed to fetch stat: " + statTask.Exception);
                continue;
            }

            int currentValue = 0;
            if (statTask.Result.Value != null)
                int.TryParse(statTask.Result.Value.ToString(), out currentValue);

            int newValue = currentValue + 1;
            yield return statRef.SetValueAsync(newValue);
            Debug.Log($"✅ Updated {key}: {currentValue} → {newValue}");
        }
    }

   IEnumerator WaitAndLoadMainScene(float delayTime)
{
    yield return new WaitForSeconds(delayTime);

    // ✅ Start cooldown AFTER finishing quiz and before returning
    CooldownManager.StartCooldown(2f);

    SceneManager.LoadScene("SampleScene"); // your main/base scene
}


    void EnableButtons()
    {
        answerButtonA.interactable = true;
        answerButtonB.interactable = true;
        answerButtonC.interactable = true;
        answerButtonD.interactable = true;
    }

    void DisableButtons()
    {
        answerButtonA.interactable = false;
        answerButtonB.interactable = false;
        answerButtonC.interactable = false;
        answerButtonD.interactable = false;
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
