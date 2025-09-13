using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance; // ✅ Singleton reference

    public Animator transitionAnimator;
    public float startToIdleDelay = 1f;
    public float endDuration = 1f;

    void Awake()
    {
        // ✅ Set the static instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartSceneTransition(string sceneName)
    {
        StartCoroutine(PlayTransition(sceneName));
    }

    private IEnumerator PlayTransition(string sceneName)
    {
        transitionAnimator.SetTrigger("ToIdle");
        yield return new WaitForSeconds(startToIdleDelay);

        transitionAnimator.SetTrigger("ToEnd");
        yield return new WaitForSeconds(endDuration);

        SceneManager.LoadScene(sceneName);
    }
}
