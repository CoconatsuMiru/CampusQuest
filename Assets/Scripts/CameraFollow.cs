using UnityEngine;
using UnityEngine.InputSystem;  // New Input System namespace

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float height = 20f;
    public float smoothSpeed = 0.125f;

    public float zoomSpeed = 0.5f;
    public float minZoom = 10f;
    public float maxZoom = 40f;

    public float panSpeed = 0.5f;

    private Vector3 dragOrigin;
    private bool isPanning = false;

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(target.position.x, height, target.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void HandleZoom()
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count >= 2)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];

            if (touch0.isInProgress && touch1.isInProgress)
            {
                Vector2 prevTouch0 = touch0.startPosition.ReadValue();
                Vector2 prevTouch1 = touch1.startPosition.ReadValue();
                Vector2 currTouch0 = touch0.position.ReadValue();
                Vector2 currTouch1 = touch1.position.ReadValue();

                float prevDistance = Vector2.Distance(prevTouch0, prevTouch1);
                float currDistance = Vector2.Distance(currTouch0, currTouch1);

                float delta = prevDistance - currDistance;
                height += delta * zoomSpeed * Time.deltaTime;
            }
        }
        else if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            height -= scroll * zoomSpeed * Time.deltaTime * 100f;
        }

        height = Mathf.Clamp(height, minZoom, maxZoom);
    }

    void HandlePan()
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count == 1)
        {
            var touch = Touchscreen.current.touches[0];

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 delta = touch.delta.ReadValue();
                Vector3 move = new Vector3(-delta.x * panSpeed * Time.deltaTime, 0, -delta.y * panSpeed * Time.deltaTime);
                target.position += move;
            }
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                dragOrigin = Mouse.current.position.ReadValue();
                isPanning = true;
            }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                isPanning = false;
            }

            if (isPanning)
            {
                Vector2 difference = Mouse.current.position.ReadValue() - (Vector2)dragOrigin;
                Vector3 move = new Vector3(-difference.x * panSpeed * Time.deltaTime, 0, -difference.y * panSpeed * Time.deltaTime);
                target.position += move;
                dragOrigin = Mouse.current.position.ReadValue();
            }
        }
    }
}
