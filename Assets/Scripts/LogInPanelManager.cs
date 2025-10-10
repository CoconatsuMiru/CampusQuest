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

    void OnEnable()
    {
        // Always clear fields when panel opens
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

        // ✅ For now, just simulate a successful login
        errorText.text = "Login successful! (UI test only)";
        StartCoroutine(ClearMessageAfterDelay());
    }

    void ShowError(string message)
    {
        errorText.text = message;
        StopAllCoroutines(); // stop old timer if still running
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
