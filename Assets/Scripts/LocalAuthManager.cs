using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class LocalAuthManager : MonoBehaviour
{
    public static LocalAuthManager Instance;

    private string filePath;
    private UserList userList;

    [System.Serializable]
    public class UserData
    {
        public string username;
        public string email;
        public string password;
        public int level = 1;
        public int exp = 0;
        public Dictionary<string, int> subjects = new Dictionary<string, int>()
        {
            {"math", 1},
            {"science", 1},
            {"english", 1},
            {"history", 1},
            {"art", 1},
            {"music", 1}
        };
    }

    [System.Serializable]
    public class UserList
    {
        public List<UserData> users = new List<UserData>();
    }

    public UserData currentUser;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ✅ Use persistent path instead of Application.dataPath
            // This ensures data stays saved across app restarts (on Android too)
            filePath = Path.Combine(Application.persistentDataPath, "users.json");
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadData()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("users.json not found. Creating new...");
            userList = new UserList();
            SaveData();
        }
        else
        {
            try
            {
                string json = File.ReadAllText(filePath);
                userList = JsonUtility.FromJson<UserList>(json);
                if (userList == null)
                {
                    userList = new UserList();
                    Debug.LogWarning("⚠️ users.json was empty or invalid. Recreated file.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("❌ Failed to load users.json: " + ex.Message);
                userList = new UserList();
            }
        }
    }

    private void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(userList, true);
            File.WriteAllText(filePath, json);
            Debug.Log("💾 Saved user data to: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Failed to save users.json: " + ex.Message);
        }
    }

    public bool Register(string username, string email, string password)
    {
        if (userList.users.Any(u => u.email == email))
        {
            Debug.LogWarning("⚠️ Email already exists.");
            return false;
        }

        UserData newUser = new UserData
        {
            username = username,
            email = email,
            password = password
        };

        userList.users.Add(newUser);
        SaveData();

        Debug.Log("✅ User registered successfully!");
        return true;
    }

    public bool Login(string email, string password)
    {
        var user = userList.users.FirstOrDefault(u => u.email == email && u.password == password);
        if (user != null)
        {
            currentUser = user;
            Debug.Log("✅ Logged in as " + user.username);
            return true;
        }

        Debug.LogWarning("❌ Login failed: invalid credentials.");
        return false;
    }

    public void UpdateUserData()
    {
        if (currentUser == null) return;
        SaveData();
    }

    public void DeleteAllUsers()
    {
        userList = new UserList();
        SaveData();
        Debug.Log("🗑️ All local users deleted.");
    }
}
