using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Collections;

public class SkillPointManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text skillPointsText;
    public TMP_Text historyText;
    public TMP_Text scienceText;
    public TMP_Text artText;
    public TMP_Text englishText;
    public TMP_Text mathText;
    public TMP_Text musicText;

    [Header("Buttons")]
    public Button addHistoryBtn;
    public Button addScienceBtn;
    public Button addArtBtn;
    public Button addEnglishBtn;
    public Button addMathBtn;
    public Button addMusicBtn;

    private DatabaseReference dbReference;
    private string userId;

    private int availableSkillPoints;
    private int history, science, art, english, math, music;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ No logged in user!");
            return;
        }

        StartCoroutine(LoadStatsFromDB());

        addHistoryBtn.onClick.AddListener(() => SpendSkillPoint("stat_10_history", ref history, historyText));
        addScienceBtn.onClick.AddListener(() => SpendSkillPoint("stat_07_science", ref science, scienceText));
        addArtBtn.onClick.AddListener(() => SpendSkillPoint("stat_06_art", ref art, artText));
        addEnglishBtn.onClick.AddListener(() => SpendSkillPoint("stat_09_english", ref english, englishText));
        addMathBtn.onClick.AddListener(() => SpendSkillPoint("stat_08_math", ref math, mathText));
        addMusicBtn.onClick.AddListener(() => SpendSkillPoint("stat_05_music", ref music, musicText));
    }

    private IEnumerator LoadStatsFromDB()
    {
        var task = dbReference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("❌ Failed to load user data: " + task.Exception);
            yield break;
        }

        if (task.Result == null || task.Result.Value == null)
        {
            Debug.LogError("⚠️ No user data found!");
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        int SafeRead(string key)
        {
            if (!snapshot.HasChild(key) || snapshot.Child(key).Value == null)
            {
                Debug.LogWarning($"⚠️ Missing key: {key}, defaulting to 0");
                return 0;
            }
            return int.Parse(snapshot.Child(key).Value.ToString());
        }

        // 🔹 Load stats from Firebase
        availableSkillPoints = SafeRead("stat_11_skillpoints");
        history = SafeRead("stat_10_history");
        science = SafeRead("stat_07_science");
        art = SafeRead("stat_06_art");
        english = SafeRead("stat_09_english");
        math = SafeRead("stat_08_math");
        music = SafeRead("stat_05_music");

        // 🔹 Update UI
        UpdateUI();
        Debug.Log("✅ Skill stats loaded successfully!");
    }

    private void UpdateUI()
    {
        skillPointsText.text = $"Available Skill Points: {availableSkillPoints}";
        historyText.text = history.ToString();
        scienceText.text = science.ToString();
        artText.text = art.ToString();
        englishText.text = english.ToString();
        mathText.text = math.ToString();
        musicText.text = music.ToString();
    }

    private void SpendSkillPoint(string statKey, ref int statValue, TMP_Text statText)
    {
        if (availableSkillPoints <= 0)
        {
            Debug.LogWarning("⚠️ Not enough skill points!");
            return;
        }

        availableSkillPoints--;
        statValue++;

        // Update UI immediately
        statText.text = statValue.ToString();
        skillPointsText.text = $"Available Skill Points: {availableSkillPoints}";

        // 🔹 Update Firebase
        dbReference.Child("users").Child(userId).Child("stat_11_skillpoints").SetValueAsync(availableSkillPoints);
        dbReference.Child("users").Child(userId).Child(statKey).SetValueAsync(statValue);

        Debug.Log($"✅ Increased {statKey} to {statValue}. Remaining skill points: {availableSkillPoints}");
    }
}
