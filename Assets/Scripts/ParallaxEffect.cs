using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ParallaxEffect : MonoBehaviour
{
    public float offsetMultiplier = 50f;
    public float smoothTime = 0.2f;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 velocity;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // -1 <= offset <= 1
        float offsetX = (mousePos.x - Screen.width / 2f) / (Screen.width / 2f);
        float offsetY = (mousePos.y - Screen.height / 2f) / (Screen.height / 2f);

        Vector2 targetPosition =
            startPosition + new Vector2(offsetX * offsetMultiplier, offsetY * offsetMultiplier);

        float halfWidth = rectTransform.rect.width * rectTransform.lossyScale.x / 2f;
        float halfHeight = rectTransform.rect.height * rectTransform.lossyScale.y / 2f;

        // Clamp to stop image from escaping view
        float clampX = Mathf.Max(halfWidth - Screen.width / 2f, 0);
        float clampY = Mathf.Max(halfHeight - Screen.height / 2f, 0);

        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            startPosition.x - clampX,
            startPosition.x + clampX
        );
        targetPosition.y = Mathf.Clamp(
            targetPosition.y,
            startPosition.y - clampY,
            startPosition.y + clampY
        );

        rectTransform.anchoredPosition = Vector2.SmoothDamp(
            rectTransform.anchoredPosition,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
