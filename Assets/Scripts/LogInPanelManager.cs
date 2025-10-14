using UnityEngine;
using TMPro;
using System.Collections;

public class LoginPanelManager : MonoBehaviour
{
    [Header("Login Fields")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("UI References")]
    public TMP_Text errorText;

    [Header("Firebase Reference")]
    public FirebaseAuthManager firebaseAuth; // Drag your FirebaseAuthManager object here

    void OnEnable()
    {
        ClearFields();
    }

    public void TryLogin()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        errorText.text = "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("All fields must be filled in!");
            return;
        }

        // ✅ Pass values to Firebase login fields and trigger login
        firebaseAuth.emailLoginField.text = email;
        firebaseAuth.passwordLoginField.text = password;
        firebaseAuth.Login();
    }

    void ShowError(string message)
    {
        errorText.text = message;
        StopAllCoroutines();
        StartCoroutine(ClearMessageAfterDelay());
    }

    IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        errorText.text = "";
    }

    public void ClearFields()
    {
        emailInput.text = "";
        passwordInput.text = "";
    }
}
