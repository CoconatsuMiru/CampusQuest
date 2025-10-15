using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    private DatabaseReference dbReference;

    public static string LoggedInUserId;
    public static string LoggedInUserName;

    private bool firebaseReady = false;

    [Space]
    [Header("Login Fields")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;

    [Space]
    [Header("Registration Fields")]
    public TMP_InputField usernameRegistrationField;
    public TMP_InputField emailRegistrationField;
    public TMP_InputField passwordRegistrationField;
    public TMP_InputField confirmPasswordRegistrationField;

    [Space]
    [Header("UI Feedback")]
    public TMP_Text statusText;

    // ---------- USER DATA CLASS ----------
    [System.Serializable]
    public class UserDataFlat
    {
        public string stat_01_username;
        public string stat_02_email;
        public int stat_03_level = 1;
        public int stat_04_exp = 0;
        public int stat_05_music = 1;
        public int stat_06_art = 1;
        public int stat_07_science = 1;
        public int stat_08_math = 1;
        public int stat_09_english = 1;
        public int stat_10_history = 1;
    }

    // ---------- UNITY METHODS ----------
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log("🔄 Checking Firebase dependencies...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                firebaseReady = true;
            }
            else
            {
                Debug.LogError("❌ Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        Debug.Log("✅ Firebase initialized successfully!");
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            user = auth.CurrentUser;

            if (signedIn)
            {
                LoggedInUserId = user.UserId;
                LoggedInUserName = user.DisplayName ?? user.Email;
                Debug.Log("✅ Signed in: " + LoggedInUserName);

                // Auto-login if already signed in and not in main scene
                if (SceneManager.GetActiveScene().name != "SampleScene")
                    SceneManager.LoadScene("SampleScene");
            }
            else if (user == null)
            {
                Debug.Log("👋 Signed out");
            }
        }
    }

    // ---------- LOGIN ----------
    public void Login()
    {
        if (!firebaseReady)
        {
            if (statusText) statusText.text = "Firebase is still initializing...";
            Debug.LogWarning("⚠️ Firebase not ready yet!");
            return;
        }

        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        if (statusText) statusText.text = "Logging in...";
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            HandleAuthError(loginTask.Exception, "Login Failed!");
        }
        else
        {
            user = loginTask.Result.User;
            LoggedInUserId = user.UserId;
            LoggedInUserName = user.DisplayName ?? email;

            Debug.Log($"✅ Welcome back {LoggedInUserName}!");
            if (statusText) statusText.text = "Login successful!";

            SceneManager.LoadScene("SampleScene");
        }
    }

    // ---------- REGISTRATION ----------
    public void Register()
    {
        if (!firebaseReady)
        {
            if (statusText) statusText.text = "Firebase is still initializing...";
            Debug.LogWarning("⚠️ Firebase not ready yet!");
            return;
        }

        StartCoroutine(RegisterAsync(
            usernameRegistrationField.text,
            emailRegistrationField.text,
            passwordRegistrationField.text,
            confirmPasswordRegistrationField.text
        ));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name))
        {
            if (statusText) statusText.text = "Username is empty!";
            yield break;
        }
        if (string.IsNullOrEmpty(email))
        {
            if (statusText) statusText.text = "Email is empty!";
            yield break;
        }
        if (password != confirmPassword)
        {
            if (statusText) statusText.text = "Passwords do not match!";
            yield break;
        }
        if (password.Length < 6)
        {
            if (statusText) statusText.text = "Password must be at least 6 characters!";
            yield break;
        }

        if (statusText) statusText.text = "Creating account...";

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            HandleAuthError(registerTask.Exception, "Registration failed!");
        }
        else
        {
            user = registerTask.Result.User;

            // Update display name
            UserProfile profile = new UserProfile { DisplayName = name };
            var updateTask = user.UpdateUserProfileAsync(profile);
            yield return new WaitUntil(() => updateTask.IsCompleted);

            if (updateTask.Exception != null)
            {
                Debug.LogError(updateTask.Exception);
                user.DeleteAsync();
                if (statusText) statusText.text = "Profile setup failed.";
            }
            else
            {
                Debug.Log("✅ Registration successful! Welcome " + user.DisplayName);
                if (statusText) statusText.text = "Registration successful!";
                WriteNewUser(user.UserId, name, email);
            }
        }
    }

    // ---------- SAVE TO DATABASE ----------
    private void WriteNewUser(string userId, string username, string email)
    {
        UserDataFlat newUser = new UserDataFlat
        {
            stat_01_username = username,
            stat_02_email = email
        };

        string json = JsonUtility.ToJson(newUser);
        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
                Debug.Log("✅ User written to DB!");
            if (task.IsFaulted)
                Debug.LogError("❌ Write failed: " + task.Exception);
        });
    }

    // ---------- AUTH ERROR HANDLER ----------
    private void HandleAuthError(System.AggregateException exception, string prefix)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = prefix + " ";
        switch (errorCode)
        {
            case AuthError.InvalidEmail: message += "Invalid email."; break;
            case AuthError.WrongPassword: message += "Wrong password."; break;
            case AuthError.MissingEmail: message += "Email missing."; break;
            case AuthError.MissingPassword: message += "Password missing."; break;
            case AuthError.EmailAlreadyInUse: message += "Email already in use."; break;
            case AuthError.WeakPassword: message += "Weak password."; break;
            default: message += "Unknown error."; break;
        }

        Debug.LogError(message);
        if (statusText) statusText.text = message;
    }
}
