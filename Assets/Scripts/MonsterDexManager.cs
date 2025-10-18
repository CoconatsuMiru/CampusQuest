using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

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

    private BossList bossList;
    private int currentIndex = 0;
    private string persistentPath;

    void Start()
    {
        persistentPath = Path.Combine(Application.persistentDataPath, "boss_data.json");

        LoadBossData();
        ShowBoss(currentIndex);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PreviousPage);
    }

    private void LoadBossData()
    {
        if (!File.Exists(persistentPath))
        {
            Debug.LogError("❌ boss_data.json not found in persistentDataPath!");
            return;
        }

        string json = File.ReadAllText(persistentPath);
        bossList = JsonUtility.FromJson<BossList>(json);
    }

    private void ShowBoss(int index)
    {
        if (bossList == null || bossList.bosses == null || bossList.bosses.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, bossList.bosses.Count - 1);
        currentIndex = index;

        var boss = bossList.bosses[index];
        bool known = boss.seen == 1 || boss.defeated == 1;

        if (!known)
        {
            bossImage.sprite = unknownSprite;
            bossNameText.text = "???";
            subjectText.text = "???";
            hpText.text = "???";
        }
        else
        {
            Sprite sprite = LoadSprite(boss.imagePath);
            bossImage.sprite = sprite != null ? sprite : unknownSprite;
            bossNameText.text = boss.bossName;
            subjectText.text = boss.subject;
            hpText.text = $"HP: {boss.hp}";
        }

        if (pageNumberText != null)
            pageNumberText.text = $"{index + 1} / {bossList.bosses.Count}";
    }

    private Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return unknownSprite;
        if (!File.Exists(path)) return unknownSprite;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    public void NextPage()
    {
        if (currentIndex < bossList.bosses.Count - 1)
        {
            LoadBossData(); // reload in real time
            ShowBoss(currentIndex + 1);
        }
    }

    public void PreviousPage()
    {
        if (currentIndex > 0)
        {
            LoadBossData(); // reload in real time
            ShowBoss(currentIndex - 1);
        }
    }
}
