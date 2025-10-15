using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginPanelManager : MonoBehaviour
{
    [Header("Login Fields")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("UI References")]
    public TMP_Text feedbackText;

    private Coroutine fadeCoroutine;

    void OnEnable()
    {
        ClearFields();
    }

    public void TryLogin()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        feedbackText.alpha = 1f;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("⚠ All fields must be filled!");
            return;
        }

        bool success = LocalAuthManager.Instance.Login(email, password);

        if (success)
        {
            ShowMessage("✅ Login successful!");
            SceneManager.LoadScene("SampleScene"); // replace with your main scene
        }
        else
        {
            ShowMessage("❌ Invalid credentials!");
        }
    }

    private void ShowMessage(string message)
    {
        feedbackText.text = message;
        fadeCoroutine = StartCoroutine(FadeOutText(3f, 1f));
    }

    private IEnumerator FadeOutText(float delay, float duration)
    {
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
        emailInput.text = "";
        passwordInput.text = "";
        feedbackText.text = "";
    }
}
