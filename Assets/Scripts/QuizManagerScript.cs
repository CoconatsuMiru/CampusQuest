using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Extra UI (Question Number & Timer)")]
    public TMP_Text questionNumberText;  // <-- Drag your “Question #” TMP here
    public TMP_Text timeLeftText;        // <-- Drag your “Time Left” TMP here

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

    // Timer variables
    private float timePerQuestion = 30f;
    private float currentTimeLeft;
    private bool isTimerRunning = false;

    void Start()
    {
        LoadQuestions();
        StartQuiz();
        UpdateXPUI();
        CooldownManager.Instance.StartCooldown();
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

        // Update UI for question number
        if (questionNumberText != null)
            questionNumberText.text = $"Question #{index + 1}";

        // Reset and start the timer
        currentTimeLeft = timePerQuestion;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTimeLeft -= Time.deltaTime;
            if (currentTimeLeft <= 0f)
            {
                currentTimeLeft = 0f;
                isTimerRunning = false;
                TimeOut();
            }

            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timeLeftText != null)
        {
            int displayTime = Mathf.CeilToInt(currentTimeLeft);
            timeLeftText.text = $"Time left: {displayTime}s";
        }
    }

    void TimeOut()
    {
        DisableButtons();
        correctPanel.SetActive(true);
        correctPanel.GetComponentInChildren<TMP_Text>().text = "⏰ Time’s up!";
        if (incorrectSound) audioSource.PlayOneShot(incorrectSound);

        currentQuestionIndex++;
        StartCoroutine(NextQuestionWithDelay());
    }

    public void OnAnswerButtonClicked(int buttonIndex)
    {
        if (!isTimerRunning) return; // prevent clicking after timeout

        isTimerRunning = false;
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
        isTimerRunning = false;
        correctPanel.SetActive(true);
        correctPanel.GetComponentInChildren<TMP_Text>().text = $"🎉 Quiz Complete!\nScore: {score}/{MaxQuestionCount}";
        DisableButtons();

        if (score > 0)
        {
            float boostMultiplier = 1f;

            switch (score)
            {
                case 1: boostMultiplier = 1.1f; break;
                case 2: boostMultiplier = 1.2f; break;
                case 3: boostMultiplier = 1.3f; break;
                case 4: boostMultiplier = 1.4f; break;
                case 5: boostMultiplier = 1.5f; break;
            }

            float boostDuration = 300f; // 5 minutes

            if (DamageBoostManager.Instance != null)
            {
                DamageBoostManager.Instance.ApplyGlobalDamageBoost(boostMultiplier, boostDuration);
                Debug.Log($"✅ Applied {boostMultiplier}x damage boost for {boostDuration}s (score: {score})");
            }
            else
            {
                Debug.LogWarning("⚠️ DamageBoostManager not found — boost not applied.");
            }
        }

        StartCoroutine(WaitAndLoadMainScene(3f));
    }

    IEnumerator WaitAndLoadMainScene(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene("SampleScene");
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
