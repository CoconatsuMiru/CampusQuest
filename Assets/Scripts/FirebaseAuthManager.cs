using UnityEngine;
using Firebase;
using Firebase.Auth;
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

    // 🔹 Login Button calls this
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
            Debug.LogError(loginTask.Exception); // log actual exception

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

            References.userName = user.DisplayName; 
            
            SceneManager.LoadScene("SampleScene"); 
        }
    }

    // 🔹 Register Button calls this
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

                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

                    string failedMessage = "Profile update failed! Because ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail: failedMessage += "Email is invalid."; break;
                        default: failedMessage += "Unknown error."; break;
                    }

                    Debug.LogError(failedMessage);
                }
                else
                {
                    Debug.Log("Registration successful! Welcome " + user.DisplayName);
                }
            }
        }
    }
}
