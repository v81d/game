using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TMPPointerEffects
    : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
{
    [SerializeField]
    private Color hoverColor;

    [SerializeField]
    private Color activeColor;

    private TMP_Text tmpText;
    private Color startingColor;
    private bool isHovering = false;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        startingColor = tmpText.color;
    }

    public void OnPointerEnter(PointerEventData _) // hovering over text
    {
        isHovering = true;
        tmpText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData _) // not hovering over text anymore
    {
        isHovering = false;
        tmpText.color = startingColor;
    }

    public void OnPointerDown(PointerEventData _) // text is active
    {
        tmpText.color = activeColor;
    }

    public void OnPointerUp(PointerEventData _) // text is not active anymore
    {
        tmpText.color = isHovering ? hoverColor : startingColor;
    }
}
