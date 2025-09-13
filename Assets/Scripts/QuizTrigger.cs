using UnityEngine;

public class QuizTrigger : MonoBehaviour
{
    public QuizManagerScript quizManager;
    private CanvasGroup quizCanvasGroup;  // Reference to the CanvasGroup

    private void Start()
    {
        // Get the CanvasGroup from the quizPanel to control raycasts
        if (quizManager != null)
        {
            quizCanvasGroup = quizManager.quizPanel.GetComponent<CanvasGroup>();
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked on object!");

        if (quizManager != null)
        {
            // Ensure the CanvasGroup blocks raycasts when the quiz panel is active
            if (quizCanvasGroup != null)
            {
                quizCanvasGroup.blocksRaycasts = false;  // Disable raycast blocking temporarily
            }

            // Start the quiz when clicked
            quizManager.StartQuiz();

            // Optional: Set a delay to re-enable raycasts once the panel is up (if needed)
            // You can adjust the time as needed to let the panel appear
            Invoke("EnableRaycasts", 0.5f);
        }
        else
        {
            Debug.LogError("QuizManager is not assigned.");
        }
    }

    private void EnableRaycasts()
    {
        if (quizCanvasGroup != null)
        {
            quizCanvasGroup.blocksRaycasts = true;  // Re-enable raycasting after starting the quiz
        }
    }
}
