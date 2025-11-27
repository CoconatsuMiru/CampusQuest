using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("Monster Achievement")]
    public int monstersDefeated = 0;
    public int monsterGoal = 5;

    [Header("Correct Answer Achievement")]
    public int correctAnswers = 0;
    public int correctAnswerGoal = 20;

    private void Awake()
    {
        // Persistent Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    // -------------------------
    //   PROGRESS ADD METHODS
    // -------------------------
    public void AddMonsterDefeat()
    {
        monstersDefeated++;
        SaveData();
    }

    public void AddCorrectAnswer()
    {
        correctAnswers++;
        SaveData();
    }

    // -------------------------
    //        CLAIM METHODS
    // -------------------------
    public void ClaimMonsterReward()
    {
        if (monstersDefeated >= monsterGoal)
        {
            monsterGoal += 5;
            SaveData();
        }
    }

    public void ClaimCorrectAnswerReward()
    {
        if (correctAnswers >= correctAnswerGoal)
        {
            correctAnswerGoal += 20;
            SaveData();
        }
    }

    // -------------------------
    //     SAVE & LOAD
    // -------------------------
    private void SaveData()
    {
        PlayerPrefs.SetInt("monstersDefeated", monstersDefeated);
        PlayerPrefs.SetInt("monsterGoal", monsterGoal);

        PlayerPrefs.SetInt("correctAnswers", correctAnswers);
        PlayerPrefs.SetInt("correctAnswerGoal", correctAnswerGoal);

        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        monstersDefeated = PlayerPrefs.GetInt("monstersDefeated", 0);
        monsterGoal = PlayerPrefs.GetInt("monsterGoal", 5);

        correctAnswers = PlayerPrefs.GetInt("correctAnswers", 0);
        correctAnswerGoal = PlayerPrefs.GetInt("correctAnswerGoal", 20);
    }
}
