using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class BossData
{
    public string bossName;
    public int hp;
    public string imagePath;
    public string subject;
    public int expReward;
}

[System.Serializable]
public class BossList
{
    public List<BossData> bosses;
}

public class BossFightManager : MonoBehaviour
{
    [Header("Boss Settings")]
    private BossData currentBoss;

    [Header("Boss Data Source")]
    public TextAsset bossDataJSON;

    [Header("UI")]
    public Button fightButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI bossHPText;
    public Image bossImage;

    [Header("Sliders")]
    public Slider bossHPSlider;
    public Slider timerSlider;

    [Header("Scene Settings")]
    public string mainSceneName = "MainScene";

    [Header("Timer Settings")]
    public float fightTimeLimit = 10f;
    private float timer;
    private int currentHP;

    void Start()
    {
        // ✅ Load random boss
        LoadRandomBoss();

        if (currentBoss != null)
        {
            Debug.Log($"🧠 A wild {currentBoss.bossName} appeared ({currentBoss.subject}) with {currentBoss.hp} HP!");
            currentHP = currentBoss.hp;

            // 🧍 Boss UI
            if (bossNameText != null)
                bossNameText.text = currentBoss.bossName;

            UpdateHPUI();

            // ✅ Initialize HP Slider
            if (bossHPSlider != null)
            {
                bossHPSlider.maxValue = currentBoss.hp;
                bossHPSlider.value = currentHP;
            }

            // ✅ Boss image
            if (!string.IsNullOrEmpty(currentBoss.imagePath) && File.Exists(currentBoss.imagePath))
            {
                Texture2D tex = LoadTexture(currentBoss.imagePath);
                bossImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        // 🕹️ Button setup
        if (fightButton != null)
        {
            fightButton.onClick.RemoveAllListeners();
            fightButton.onClick.AddListener(OnFight);
        }

        // ⏳ Timer setup
        timer = fightTimeLimit;
        UpdateTimerUI();
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            Debug.Log($"⏰ Time’s up! You failed to defeat {currentBoss?.bossName ?? "the boss"}");
            ReturnToMainScene(false);
        }
    }

    private void UpdateHPUI()
    {
        if (bossHPText != null)
            bossHPText.text = $"HP: {currentHP}/{currentBoss.hp}";

        if (bossHPSlider != null)
            bossHPSlider.value = currentHP;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.Ceil(timer)}s";

        if (timerSlider != null)
        {
            timerSlider.maxValue = fightTimeLimit;
            timerSlider.value = timer;
        }
    }

    public void OnFight()
    {
        if (currentBoss == null)
        {
            Debug.LogError("❌ No boss data found!");
            return;
        }

        if (LocalAuthManager.Instance == null || LocalAuthManager.Instance.currentUser == null)
        {
            Debug.LogError("❌ No logged in user found!");
            return;
        }

        StartCoroutine(HandleFightDamage());
    }

    private IEnumerator HandleFightDamage()
    {
        yield return null; // simulate async step for consistency

        var user = LocalAuthManager.Instance.currentUser;
        string subjectKey = currentBoss.subject.ToLower();

        if (!user.subjects.ContainsKey(subjectKey))
        {
            Debug.LogWarning($"⚠️ Unknown subject type: {currentBoss.subject}");
            yield break;
        }

        int baseDamage = user.subjects[subjectKey];

        // ✅ Apply damage multiplier from DamageBoostManager
        float finalDamage = baseDamage * DamageBoostManager.Instance.globalDamageMultiplier;
        int damage = Mathf.RoundToInt(finalDamage);

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"💥 You hit {currentBoss.bossName}! {damage} damage dealt (base {baseDamage}, x{DamageBoostManager.Instance.globalDamageMultiplier}). HP left: {currentHP}");

        UpdateHPUI();

        if (currentHP <= 0)
        {
            Debug.Log($"✅ {currentBoss.bossName} defeated!");
            StartCoroutine(RewardExpCoroutine(currentBoss.expReward));
        }
    }

    private IEnumerator RewardExpCoroutine(int expGained)
    {
        yield return null; // simulate async step

        var user = LocalAuthManager.Instance.currentUser;

        int currentExp = user.exp;
        int currentLevel = user.level;
        int currentSkillPoints = PlayerBossStats.Instance != null ? PlayerBossStats.Instance.skillPoints : 0;

        currentExp += expGained;
        int expToNext = 50 * currentLevel;

        while (currentExp >= expToNext)
        {
            currentExp -= expToNext;
            currentLevel++;
            currentSkillPoints += 5;
            Debug.Log($"🎉 Level Up! Now Level {currentLevel} (+5 Skill Points)");
            expToNext = 50 * currentLevel;
        }

        // ✅ Update user stats
        user.level = currentLevel;
        user.exp = currentExp;
        if (PlayerBossStats.Instance != null)
            PlayerBossStats.Instance.skillPoints = currentSkillPoints;

        LocalAuthManager.Instance.UpdateUserData();
        Debug.Log($"🏆 Gained {expGained} EXP! Level: {currentLevel}, Skill Points: {currentSkillPoints}");

        ReturnToMainScene(true);
    }

    void ReturnToMainScene(bool success)
    {
        SceneManager.LoadScene(mainSceneName);
    }

    void LoadRandomBoss()
    {
        if (bossDataJSON == null)
        {
            Debug.LogError("❌ No bossDataJSON assigned in Inspector!");
            return;
        }

        BossList allBosses = JsonUtility.FromJson<BossList>(bossDataJSON.text);

        if (allBosses == null || allBosses.bosses == null || allBosses.bosses.Count == 0)
        {
            Debug.LogError("❌ No bosses found in provided boss_data.json!");
            return;
        }

        int randomIndex = Random.Range(0, allBosses.bosses.Count);
        currentBoss = allBosses.bosses[randomIndex];
    }

    Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }
}
