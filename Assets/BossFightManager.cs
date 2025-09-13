using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BossFightManager : MonoBehaviour
{
    [Header("Boss Settings")]
    public string bossID; // must match PoissonForWaypoints bossID
    public string monsterName = "Slime";
    public int monsterHP = 5;

    [Header("UI")]
    public Button fightButton;
    public Button runButton;
    public TextMeshProUGUI timerText;

    [Header("Scene Settings")]
    public string mainSceneName = "MainScene";

    [Header("Timer Settings")]
    public float fightTimeLimit = 10f;
    private float timer;

    void Start()
    {
        Debug.Log("A wild " + monsterName + " appeared with " + monsterHP + " HP!");

        fightButton.onClick.AddListener(OnFight);
        runButton.onClick.AddListener(OnRun);

        timer = fightTimeLimit;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timerText != null)
                timerText.text = "Time: " + Mathf.Ceil(timer).ToString();
        }
        else
        {
            Debug.Log("Time’s up! You failed to defeat " + monsterName);
            ReturnToMainScene(false);
        }
    }

    void OnFight()
    {
        monsterHP--;
        Debug.Log("You hit " + monsterName + "! HP left: " + monsterHP);

        if (monsterHP <= 0)
        {
            Debug.Log(monsterName + " defeated!");
            ReturnToMainScene(true);
        }
    }

    void OnRun()
    {
        Debug.Log("You ran away from " + monsterName);
        ReturnToMainScene(false);
    }

    /// <summary>
    /// Handles cleanup + boss unlock + player save before going back to MainScene
    /// </summary>
    void ReturnToMainScene(bool success)
    {
        // ✅ mark boss defeated if player won
        if (success && !string.IsNullOrEmpty(bossID))
        {
            if (BossManager.Instance == null)
                new GameObject("BossManager").AddComponent<BossManager>();

            BossManager.Instance.MarkBossDefeated(bossID);
        }

        // ✅ save player position before leaving
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && PlayerSaveManager.Instance != null)
        {
            PlayerSaveManager.Instance.SavePlayerLocation(
                player.transform.position,
                mainSceneName
            );
        }

        // ✅ finally return to MainScene
        SceneManager.LoadScene(mainSceneName);
    }
}
