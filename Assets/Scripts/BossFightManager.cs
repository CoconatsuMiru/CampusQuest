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
    public string imagePath; // for compatibility, not used
    public string subject;
    public int expReward;
    public int seen;
    public int defeated;
}

[System.Serializable]
public class BossList
{
    public List<BossData> bosses;
}

[System.Serializable]
public class SubjectSpritePair
{
    public string subject;
    public Sprite sprite;
}

public class BossFightManager : MonoBehaviour
{
    public enum TierType { Low, Mid, High }

    [Header("Boss Tier Settings")]
    [Tooltip("Select which tier of bosses this scene uses.")]
    public TierType selectedTier = TierType.Low;

    [Header("Boss Data Source")]
    public TextAsset lowTierDataJSON;
    public TextAsset midTierDataJSON;
    public TextAsset highTierDataJSON;

    [Header("UI")]
    public Button fightButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI bossHPText;
    public Image bossImage;
    public Slider bossHPSlider;
    public Slider timerSlider;

    [Header("Scene Settings")]
    public string mainSceneName = "SampleScene";

    [Header("Timer Settings")]
    public float fightTimeLimit = 10f;

    [Header("Sprites by Subject")]
    public List<SubjectSpritePair> bossSprites;

    private BossList allBosses;
    private BossData currentBoss;
    private int currentBossIndex;
    private float timer;
    private int currentHP;
    private string persistentPath;

    void Start()
    {
        persistentPath = Path.Combine(Application.persistentDataPath, $"boss_data_{selectedTier.ToString().ToLower()}.json");

        LoadBossData();
        LoadRandomBoss();

        if (currentBoss != null)
        {
            currentHP = currentBoss.hp;
            currentBoss.seen = 1;
            SaveBossData();

            bossNameText.text = currentBoss.bossName;
            UpdateHPUI();

            if (bossHPSlider != null)
            {
                bossHPSlider.maxValue = currentBoss.hp;
                bossHPSlider.value = currentHP;
            }

            Sprite bossSprite = GetSpriteForSubject(currentBoss.subject);
            if (bossSprite != null)
                bossImage.sprite = bossSprite;
            else
                Debug.LogWarning($"⚠️ No sprite found for subject: {currentBoss.subject}");
        }

        if (fightButton != null)
        {
            fightButton.onClick.RemoveAllListeners();
            fightButton.onClick.AddListener(OnFight);
        }

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

    private void LoadBossData()
    {
        if (File.Exists(persistentPath))
        {
            string json = File.ReadAllText(persistentPath);
            allBosses = JsonUtility.FromJson<BossList>(json);
        }
        else
        {
            TextAsset selectedData = null;
            switch (selectedTier)
            {
                case TierType.Low: selectedData = lowTierDataJSON; break;
                case TierType.Mid: selectedData = midTierDataJSON; break;
                case TierType.High: selectedData = highTierDataJSON; break;
            }

            if (selectedData == null)
            {
                Debug.LogError($"❌ Missing JSON for {selectedTier} tier!");
                return;
            }

            allBosses = JsonUtility.FromJson<BossList>(selectedData.text);
            SaveBossData();
        }
    }

    private void SaveBossData()
    {
        string json = JsonUtility.ToJson(allBosses, true);
        File.WriteAllText(persistentPath, json);
        Debug.Log($"💾 Boss data saved to {persistentPath}");
    }

    private void LoadRandomBoss()
    {
        if (allBosses == null || allBosses.bosses == null || allBosses.bosses.Count == 0)
        {
            Debug.LogError("❌ No bosses found!");
            return;
        }

        currentBossIndex = Random.Range(0, allBosses.bosses.Count);
        currentBoss = allBosses.bosses[currentBossIndex];
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
        yield return null;

        var user = LocalAuthManager.Instance.currentUser;
        string subjectKey = currentBoss.subject.ToLower();

        if (!user.subjects.ContainsKey(subjectKey))
        {
            Debug.LogWarning($"⚠️ Unknown subject type: {currentBoss.subject}");
            yield break;
        }

        int baseDamage = user.subjects[subjectKey];
        float finalDamage = baseDamage * DamageBoostManager.Instance.globalDamageMultiplier;
        int damage = Mathf.RoundToInt(finalDamage);

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        UpdateHPUI();

        if (currentHP <= 0)
        {
            currentBoss.defeated = 1;
            SaveBossData();
            StartCoroutine(RewardExpCoroutine(currentBoss.expReward));
        }
    }

    private IEnumerator RewardExpCoroutine(int expGained)
    {
        yield return null;

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
            expToNext = 50 * currentLevel;
        }

        user.level = currentLevel;
        user.exp = currentExp;
        if (PlayerBossStats.Instance != null)
            PlayerBossStats.Instance.skillPoints = currentSkillPoints;

        LocalAuthManager.Instance.UpdateUserData();
        ReturnToMainScene(true);
    }

    private void ReturnToMainScene(bool success)
    {
        SceneManager.LoadScene(mainSceneName);
    }

    private void UpdateHPUI()
    {
        bossHPText.text = $"HP: {currentHP}/{currentBoss.hp}";
        bossHPSlider.value = currentHP;
    }

    private void UpdateTimerUI()
    {
        timerText.text = $"Time: {Mathf.Ceil(timer)}s";
        timerSlider.maxValue = fightTimeLimit;
        timerSlider.value = timer;
    }

    private Sprite GetSpriteForSubject(string subject)
    {
        foreach (var pair in bossSprites)
        {
            if (pair.subject.ToLower() == subject.ToLower())
                return pair.sprite;
        }
        return null;
    }
}
