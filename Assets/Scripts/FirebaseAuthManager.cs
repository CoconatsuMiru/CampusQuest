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

    // 🔹 Static so other scripts/scenes can easily access
    public static string LoggedInUserId;
    public static string LoggedInUserName;

    [Space]
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;

    [Space]
    [Header("Registration")]
    public TMP_InputField usernameRegistrationField;
    public TMP_InputField emailRegistrationField;
    public TMP_InputField passwordRegistrationField;
    public TMP_InputField confirmPasswordRegistrationField;

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
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    // ---------- LOGIN ----------
    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! Because ";
            switch (authError)
            {
                case AuthError.InvalidEmail: failedMessage += "Email is invalid."; break;
                case AuthError.WrongPassword: failedMessage += "Wrong Password."; break;
                case AuthError.MissingEmail: failedMessage += "Email is missing."; break;
                case AuthError.MissingPassword: failedMessage += "Password is missing."; break;
                default: failedMessage += "Unknown error."; break;
            }

            Debug.LogError(failedMessage);
        }
        else
        {
            user = loginTask.Result.User;
            Debug.Log($"Welcome back {user?.DisplayName ?? email}!");

            // 🔹 Store for global access
            LoggedInUserId = user.UserId;
            LoggedInUserName = user.DisplayName ?? email;

            // Optional reference
            References.userName = user.DisplayName;

            SceneManager.LoadScene("SampleScene");
        }
    }

    // ---------- REGISTRATION ----------
    public void Register()
    {
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
            Debug.LogError("Username is empty");
        }
        else if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Email is empty");
        }
        else if (password != confirmPassword)
        {
            Debug.LogError("Passwords do not match");
        }
        else if (password.Length < 6)
        {
            Debug.LogError("Password must be at least 6 characters");
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Registration failed! Because ";
                switch (authError)
                {
                    case AuthError.InvalidEmail: failedMessage += "Email is invalid."; break;
                    case AuthError.MissingEmail: failedMessage += "Email is missing."; break;
                    case AuthError.MissingPassword: failedMessage += "Password is missing."; break;
                    case AuthError.WeakPassword: failedMessage += "Password is too weak."; break;
                    case AuthError.EmailAlreadyInUse: failedMessage += "Email already in use."; break;
                    default: failedMessage += "Unknown error."; break;
                }

                Debug.LogError(failedMessage);
            }
            else
            {
                user = registerTask.Result.User;

                // Update display name
                UserProfile userProfile = new UserProfile { DisplayName = name };
                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    user.DeleteAsync(); // rollback account
                    Debug.LogError(updateProfileTask.Exception);
                }
                else
                {
                    Debug.Log("Registration successful! Welcome " + user.DisplayName);

                    // Save user to Realtime Database
                    WriteNewUser(user.UserId, name, email);
                }
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
                Debug.Log("User successfully written to DB!");
            if (task.IsFaulted)
                Debug.LogError("Write failed: " + task.Exception);
        });
    }
}
