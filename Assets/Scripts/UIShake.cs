using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIShake : MonoBehaviour, IPointerDownHandler
{
    private IEnumerator newShakeCoroutine;
    private Vector2 initialPosition;
    private bool coroutineAllowed = true;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
    }

    // Detect tap/click on UI
    public void OnPointerDown(PointerEventData eventData)
    {
        if (coroutineAllowed)
        {
            StartShaking();
        }
    }

    private void StartShaking()
    {
        coroutineAllowed = false;
        newShakeCoroutine = ShakeCoroutine();
        StartCoroutine(newShakeCoroutine);
        Invoke(nameof(StopShaking), 0.5f);
    }

    private void StopShaking()
    {
        StopCoroutine(newShakeCoroutine);
        rectTransform.anchoredPosition = initialPosition;
        coroutineAllowed = true;
    }

    private IEnumerator ShakeCoroutine()
    {
        while (true)
        {
            float offsetX = Random.Range(-10f, 10f); // pixels
            float offsetY = Random.Range(-10f, 10f); // pixels

            rectTransform.anchoredPosition = initialPosition + new Vector2(offsetX, offsetY);

            yield return new WaitForSeconds(0.01f);

            rectTransform.anchoredPosition = initialPosition;
        }
    }
}

