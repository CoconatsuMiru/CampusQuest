using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class BossSpriteData
{
    public string subject;
    public Sprite sprite;
}

public class MonsterDexManager : MonoBehaviour
{
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

    [Header("Boss Data Source (Drag JSON File Here)")]
    [SerializeField] private TextAsset bossJSONFile;

    private BossList bossList;
    private int currentIndex = 0;

    void Start()
    {
        if (bossJSONFile == null)
        {
            Debug.LogError("❌ No boss JSON file assigned in " + gameObject.name);
            return;
        }

        LoadBossData();
        ShowBoss(currentIndex);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PreviousPage);
    }

    private void LoadBossData()
    {
        bossList = JsonUtility.FromJson<BossList>(bossJSONFile.text);
        if (bossList == null || bossList.bosses == null || bossList.bosses.Count == 0)
        {
            Debug.LogError($"❌ Failed to load boss data from {bossJSONFile.name}");
        }
    }

    private void ShowBoss(int index)
    {
        if (bossList == null || bossList.bosses == null || bossList.bosses.Count == 0) return;

        index = Mathf.Clamp(index, 0, bossList.bosses.Count - 1);
        currentIndex = index;

        var boss = bossList.bosses[index];
        bool known = boss.seen == 1 && boss.defeated == 1; // ✅ both must be true

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
