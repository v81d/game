using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ParallaxScrolling : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f;

    private RectTransform tileA;
    private RectTransform tileB;
    private float tileWidth;
    private Vector2 startPos;
    private float offset;

    private void Start()
    {
        tileA = GetComponent<RectTransform>();

        // Force a layout rebuild so rect dimensions are up to date
        Canvas.ForceUpdateCanvases();

        // Measure actual rendered width via world-space corners,
        // then convert to parent-local distance so anchored offsets match exactly.
        Vector3[] corners = new Vector3[4];
        tileA.GetWorldCorners(corners);
        // corners: 0=bottom-left, 2=top-right
        float worldWidth = corners[2].x - corners[0].x;

        RectTransform parentRect = tileA.parent as RectTransform;
        if (parentRect != null)
        {
            tileWidth = worldWidth / parentRect.lossyScale.x;
        }
        else
        {
            tileWidth = worldWidth;
        }

        if (tileWidth <= 0f)
        {
            enabled = false;
            return;
        }

        startPos = tileA.anchoredPosition;

        // Instantiate creates a clone whose Awake runs immediately,
        // but all logic is in Start, so we can disable the clone's
        // script before its Start ever fires.
        GameObject clone = Instantiate(gameObject, transform.parent);
        ParallaxScrolling cloneScroller = clone.GetComponent<ParallaxScrolling>();
        if (cloneScroller != null)
        {
            cloneScroller.enabled = false;
        }

        tileB = clone.GetComponent<RectTransform>();
        // Copy exact size so clone matches original pixel-for-pixel
        tileB.sizeDelta = tileA.sizeDelta;
        tileB.pivot = tileA.pivot;
        tileB.anchorMin = tileA.anchorMin;
        tileB.anchorMax = tileA.anchorMax;
        tileB.anchoredPosition = new Vector2(startPos.x + tileWidth, startPos.y);
    }

    private void Update()
    {
        if (tileB == null)
        {
            return;
        }

        offset += scrollSpeed * Time.deltaTime;
        float mod = Mathf.Repeat(offset, tileWidth);

        tileA.anchoredPosition = new Vector2(startPos.x - mod, startPos.y);
        tileB.anchoredPosition = new Vector2(startPos.x - mod + tileWidth, startPos.y);
    }
}
