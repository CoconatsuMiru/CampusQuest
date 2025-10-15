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
    public TMP_Text feedbackText;

    private Coroutine fadeCoroutine;

    public void TrySignUp()
    {
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        feedbackText.alpha = 1f;
        feedbackText.text = "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowMessage("⚠ All fields are required!");
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

        bool success = LocalAuthManager.Instance.Register(username, email, password);

        if (success)
            ShowMessage("✅ Account created successfully!");
        else
            ShowMessage("⚠ Email already exists!");
    }

    private void ShowMessage(string message)
    {
        feedbackText.text = message;
        feedbackText.alpha = 1f;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutText(3f, 1f));
    }

    private IEnumerator FadeOutText(float delay, float duration)
    {
        yield return null; // ✅ wait one frame to ensure text renders on Android
        yield return new WaitForSeconds(delay);

        float startAlpha = feedbackText.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            feedbackText.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            yield return null;
        }

        feedbackText.text = "";
        feedbackText.alpha = 1f;
    }

    public void ClearFields()
    {
        usernameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
        confirmPasswordInput.text = "";
        feedbackText.text = "";
        feedbackText.alpha = 1f;
    }
}
