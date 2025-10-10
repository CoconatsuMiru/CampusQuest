using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text usernameText;
    public TMP_Text levelText;
    public TMP_Text expText;
    public TMP_Text skillPointsText;

    private DatabaseReference dbReference;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (!string.IsNullOrEmpty(FirebaseAuthManager.LoggedInUserId))
        {
            StartCoroutine(LoadUserData(FirebaseAuthManager.LoggedInUserId));
        }
        else
        {
            Debug.LogError("❌ No logged in user ID found!");
        }
    }

    private IEnumerator LoadUserData(string userId)
    {
        var userDataTask = dbReference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(() => userDataTask.IsCompleted);

        if (userDataTask.Exception != null)
        {
            Debug.LogError("Failed to fetch user data: " + userDataTask.Exception);
            yield break;
        }

        if (userDataTask.Result == null || userDataTask.Result.Value == null)
        {
            Debug.LogWarning("No data found for this user!");
            yield break;
        }

        DataSnapshot snapshot = userDataTask.Result;

        string username = snapshot.Child("stat_01_username").Value?.ToString() ?? "Unknown";
        int level = int.Parse(snapshot.Child("stat_03_level").Value?.ToString() ?? "1");
        int exp = int.Parse(snapshot.Child("stat_04_exp").Value?.ToString() ?? "0");
        int skillPoints = int.Parse(snapshot.Child("stat_11_skillpoints").Value?.ToString() ?? "0");

        // 🧮 Calculate the required EXP dynamically
        int expToNext = level * 50;

        // 🖥️ Update the UI
        if (usernameText != null) usernameText.text = username;
        if (levelText != null) levelText.text = $"Level: {level}";
        if (expText != null) expText.text = $"Exp: {exp}/{expToNext}";
        if (skillPointsText != null) skillPointsText.text = $"Skill Points: {skillPoints}";

        Debug.Log($"✅ User data loaded! (Exp: {exp}/{expToNext})");
    }
}