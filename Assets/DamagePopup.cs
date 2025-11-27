using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 50f;
    public float fadeSpeed = 1f;

    private TextMeshProUGUI text;
    private Color originalColor;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalColor = text.color;
    }

    public void Setup(int damage)
    {
        text.text = damage.ToString();
    }

    void Update()
    {
        // Move upward
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // Fade out
        Color c = text.color;
        c.a -= fadeSpeed * Time.deltaTime;
        text.color = c;

        // Destroy when fully transparent
        if (text.color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}

