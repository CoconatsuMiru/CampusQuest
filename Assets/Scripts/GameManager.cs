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

    private DatabaseReference dbReference;

    public static object Instance { get; internal set; }

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (!string.IsNullOrEmpty(FirebaseAuthManager.LoggedInUserId))
        {
            StartCoroutine(LoadUserData(FirebaseAuthManager.LoggedInUserId));
        }
        else
        {
            Debug.LogError("No logged in user ID found!");
        }
    }

    private IEnumerator LoadUserData(string userId)
    {
        var userDataTask = dbReference.Child("users").Child(userId).GetValueAsync();

        yield return new WaitUntil(() => userDataTask.IsCompleted);

        if (userDataTask.Exception != null)
        {
            Debug.LogError("Failed to fetch user data: " + userDataTask.Exception);
        }
        else if (userDataTask.Result.Value == null)
        {
            Debug.LogWarning("No data found for this user!");
        }
        else
        {
            DataSnapshot snapshot = userDataTask.Result;

            string username = snapshot.Child("stat_01_username").Value.ToString();
            string level = snapshot.Child("stat_03_level").Value.ToString();
            string exp = snapshot.Child("stat_04_exp").Value.ToString();

            // 🔹 Update UI
            if (usernameText != null) usernameText.text = username;
            if (levelText != null) levelText.text = "Level: " + level;
            if (expText != null) expText.text = "Exp: " + exp;

            Debug.Log("User data loaded successfully!");
        }
    }
}
