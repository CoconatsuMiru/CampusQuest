using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class BossSpriteData
{
    public string subject;
    public Sprite sprite;
}

public class MonsterDexManager : MonoBehaviour
{
    public enum TierType { Low, Mid, High }

    [Header("Tier Selection")]
    [SerializeField] private TierType selectedTier = TierType.Low;

    [Header("Boss Data Files")]
    [SerializeField] private TextAsset lowTierDataJSON;
    [SerializeField] private TextAsset midTierDataJSON;
    [SerializeField] private TextAsset highTierDataJSON;

    [Header("UI References")]
    [SerializeField] private Image bossImage;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text subjectText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text pageNumberText;
    [SerializeField] private Sprite unknownSprite;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Boss Sprites by Subject")]
    [SerializeField] private List<BossSpriteData> bossSprites = new List<BossSpriteData>();

    private BossList bossList;
    private int currentIndex = 0;
    private string persistentPath;

    void Start()
    {
        // ✅ Determine which tier file to use
        TextAsset selectedFile = null;
        switch (selectedTier)
        {
            case TierType.Low: selectedFile = lowTierDataJSON; break;
            case TierType.Mid: selectedFile = midTierDataJSON; break;
            case TierType.High: selectedFile = highTierDataJSON; break;
        }

        if (selectedFile == null)
        {
            Debug.LogError($"❌ No JSON file assigned for {selectedTier} tier!");
            return;
        }

        // ✅ Path for persistent version (updated by BossFightManager)
        persistentPath = Path.Combine(Application.persistentDataPath, $"boss_data_{selectedTier.ToString().ToLower()}.json");

        LoadBossData(selectedFile);
        ShowBoss(currentIndex);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PreviousPage);
    }

    private void LoadBossData(TextAsset selectedFile)
    {
        if (File.Exists(persistentPath))
        {
            string json = File.ReadAllText(persistentPath);
            bossList = JsonUtility.FromJson<BossList>(json);
            Debug.Log($"📖 Loaded updated data from {persistentPath}");
        }
        else
        {
            bossList = JsonUtility.FromJson<BossList>(selectedFile.text);
            Debug.Log($"📘 Loaded default data from {selectedFile.name}");
        }

        if (bossList == null || bossList.bosses == null || bossList.bosses.Count == 0)
        {
            Debug.LogError($"❌ Failed to load boss data for {selectedTier}");
        }
    }

    private void ShowBoss(int index)
    {
        if (bossList == null || bossList.bosses == null || bossList.bosses.Count == 0) return;

        index = Mathf.Clamp(index, 0, bossList.bosses.Count - 1);
        currentIndex = index;

        var boss = bossList.bosses[index];
        bool known = boss.seen == 1 || boss.defeated == 1; // ✅ unlocked if seen OR defeated

        if (!known)
        {
            bossImage.sprite = unknownSprite;
            bossNameText.text = "???";
            subjectText.text = "???";
            hpText.text = "???";
        }
        else
        {
            Sprite sprite = GetSpriteForSubject(boss.subject);
            bossImage.sprite = sprite != null ? sprite : unknownSprite;
            bossNameText.text = boss.bossName;
            subjectText.text = boss.subject;
            hpText.text = $"HP: {boss.hp}";
        }

        if (pageNumberText != null)
            pageNumberText.text = $"{index + 1} / {bossList.bosses.Count}";
    }

    private Sprite GetSpriteForSubject(string subject)
    {
        foreach (var entry in bossSprites)
        {
            if (entry.subject.ToLower() == subject.ToLower())
                return entry.sprite;
        }
        return unknownSprite;
    }

    public void NextPage()
    {
        if (bossList == null || bossList.bosses.Count == 0) return;
        if (currentIndex < bossList.bosses.Count - 1)
            ShowBoss(currentIndex + 1);
    }

    public void PreviousPage()
    {
        if (bossList == null || bossList.bosses.Count == 0) return;
        if (currentIndex > 0)
            ShowBoss(currentIndex - 1);
    }
}
