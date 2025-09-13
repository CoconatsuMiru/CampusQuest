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

    private List<Question> questions = new List<Question>();
    private Question currentQuestion;
    private string selectedCorrectAnswer;

    private int score = 0;
    private int currentQuestionIndex = 0;

    private const int MaxQuestionCount = 5;
    private string[] subjects = { "artQuestions_100", "mathQuestions_100", "scienceQuestions_100" };

    void Start()
    {
        LoadAllSubjects();
        StartQuiz();
        UpdateXPUI(); 
    }

    void LoadAllSubjects()
    {
        foreach (var subject in subjects)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(subject);
            if (jsonFile != null)
            {
                QuizData quizData = JsonUtility.FromJson<QuizData>(jsonFile.text);
                if (quizData != null && quizData.questions != null)
                {
                    var enabledQuestions = quizData.questions.Where(q => q.enabled == 1);
                    questions.AddRange(enabledQuestions);
                }
            }
        }

        questions = questions.OrderBy(q => Random.value).Take(MaxQuestionCount).ToList();
    }

    public void StartQuiz()
    {
        if (questions != null && questions.Count > 0)
        {
            quizPanel.SetActive(true);
            score = 0;
            UpdateScoreUI();
            currentQuestionIndex = 0;
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.LogError("No questions available.");
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

        questionProgressText.text = $"{MaxQuestionCount - questions.Count + 1}/{MaxQuestionCount}";
    }

    public void OnAnswerButtonClicked(int buttonIndex)
    {
        string selectedAnswer = "";

        if (buttonIndex == 0) selectedAnswer = answerButton1.GetComponentInChildren<TMP_Text>().text;
        if (buttonIndex == 1) selectedAnswer = answerButton2.GetComponentInChildren<TMP_Text>().text;
        if (buttonIndex == 2) selectedAnswer = answerButton3.GetComponentInChildren<TMP_Text>().text;

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

        questions.RemoveAt(currentQuestionIndex);

        if (questions.Count > 0)
        {
            currentQuestionIndex = Random.Range(0, questions.Count);
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            EndQuiz();
        }
    }

void EndQuiz()
{
    Debug.Log($"🎉 Quiz complete! Final score: {score}/{MaxQuestionCount}");

    // === AWARD XP BASED ON CORRECT ANSWERS ===
    if (GameManager.Instance != null)
        GameManager.Instance.AddXP(score);

    quizPanel.SetActive(true);
    DisableButtons();
    StartCoroutine(WaitAndLoadMainScene(3f));
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

    // ==== XP SYSTEM (PlayerPrefs for persistence) ====
    void AddXP(int amount)
    {
        int savedXP = PlayerPrefs.GetInt("CurrentXP", 0);
        int savedLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        int savedXpToNext = PlayerPrefs.GetInt("XpToNextLevel", 10);

        savedXP += amount;

        while (savedXP >= savedXpToNext)
        {
            savedXP -= savedXpToNext;
            savedLevel++;
            savedXpToNext += 5; 
        }

        PlayerPrefs.SetInt("CurrentXP", savedXP);
        PlayerPrefs.SetInt("PlayerLevel", savedLevel);
        PlayerPrefs.SetInt("XpToNextLevel", savedXpToNext);
        PlayerPrefs.Save();

        UpdateXPUI();
    }

    void UpdateXPUI()
    {
        int savedXP = PlayerPrefs.GetInt("CurrentXP", 0);
        int savedLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        int savedXpToNext = PlayerPrefs.GetInt("XpToNextLevel", 10);

        if (levelText != null)
            levelText.text = $"Level: {savedLevel}";
        if (xpText != null)
            xpText.text = $"XP: {savedXP}/{savedXpToNext}";
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
