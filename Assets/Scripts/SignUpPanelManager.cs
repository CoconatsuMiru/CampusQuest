using UnityEngine;
using TMPro;
using System.Collections;

public class SignUpPanelManager : MonoBehaviour
{
    [Header("Sign Up Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("UI References")]
    public TMP_Text errorText;

    private Coroutine fadeCoroutine;

    public void TrySignUp()
    {
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        errorText.alpha = 1f;
        errorText.text = "";

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(confirmPassword))
        {
            ShowMessage("All fields must be filled in!");
            return;
        }

        if (password.Length < 6)
        {
            ShowMessage("⚠ Password must be at least 6 characters.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowMessage("⚠ Passwords do not match!");
            return;
        }

        ShowMessage("✅ Account Created (UI test only)");
    }

    private void ShowMessage(string message)
    {
        errorText.text = message;
        fadeCoroutine = StartCoroutine(FadeOutText(3f, 1f));
    }

    private IEnumerator FadeOutText(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float startAlpha = errorText.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            errorText.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        errorText.text = "";
        errorText.alpha = 1f;
    }

    // 🚀 Call this when closing the panel
    public void ClearFields()
    {
        usernameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
        confirmPasswordInput.text = "";
        errorText.text = "";
        errorText.alpha = 1f; // reset in case it faded last time
    }
}
