using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizManagerScript : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionText;
    public Button answerButtonA;
    public Button answerButtonB;
    public Button answerButtonC;
    public Button answerButtonD;
    public TMP_Text scoreText;
    public TMP_Text rewardText;
    public GameObject correctPanel;
    public GameObject endQuizPanel;
    public Button continueButton;

    [Header("Extra UI (Question Number & Timer)")]
    public TMP_Text questionNumberText;
    public TMP_Text timeLeftText;

    [Header("Difficulty UI")]
    public TMP_Text difficultyText;

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

    private float timePerQuestion = 30f;
    private float currentTimeLeft;
    private bool isTimerRunning = false;

    void Start()
    {
        LoadQuestions();
        StartQuiz();

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);
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
        score = 0;
        currentQuestionIndex = 0;
        UpdateScoreUI();
        ShowQuestion(currentQuestionIndex);
        endQuizPanel.SetActive(false);
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

        if (difficultyText != null)
            difficultyText.text = currentQuestion.difficulty.ToUpperInvariant();

        List<string> shuffledAnswers = new List<string>(currentQuestion.answers);
        shuffledAnswers = shuffledAnswers.OrderBy(a => Random.value).ToList();

        answerButtonA.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[0];
        answerButtonB.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[1];
        answerButtonC.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[2];
        answerButtonD.GetComponentInChildren<TMP_Text>().text = shuffledAnswers[3];

        correctPanel.SetActive(false);
        EnableButtons();

        if (questionNumberText != null)
            questionNumberText.text = $"Question #{index + 1}";

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
        if (!isTimerRunning) return;

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

            // ✅ Achievement Integration
            AchievementManager.Instance.AddCorrectAnswer();

            questionText.text = "✅ Correct!";
            correctPanel.SetActive(true);
            correctPanel.GetComponentInChildren<TMP_Text>().text = "Correct!";
            if (correctSound) audioSource.PlayOneShot(correctSound);
        }
        else
        {
            questionText.text = "❌ Wrong!";
            correctPanel.SetActive(true);
            correctPanel.GetComponentInChildren<TMP_Text>().text = "Wrong!";
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
        DisableButtons();
        correctPanel.SetActive(false);

        endQuizPanel.SetActive(true);

        if (scoreText != null)
        {
            scoreText.text = $"You answered {score}/{MaxQuestionCount} questions correctly";
        }

        if (rewardText != null)
        {
            float rewardMultiplier = 1f;
            switch (score)
            {
                case 1: rewardMultiplier = 1.1f; break;
                case 2: rewardMultiplier = 1.2f; break;
                case 3: rewardMultiplier = 1.3f; break;
                case 4: rewardMultiplier = 1.4f; break;
                case 5: rewardMultiplier = 1.5f; break;
            }

            float rewardDuration = 120f;
            rewardText.text = $"Your reward is {rewardMultiplier}x damage buff for {rewardDuration / 60} minutes.";
        }
    }

    public void OnContinueButtonClicked()
    {
        Debug.Log("Continue button clicked. Returning to main scene...");
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
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    [System.Serializable]
    public class Question
    {
        public string question;
        public string[] answers;
        public string correctAnswer;
        public int enabled = 1;
        public string difficulty;
    }

    [System.Serializable]
    public class QuizData
    {
        public List<Question> questions;
    }
}
