using UnityEngine;
using UnityEngine.Events;

public class TriggerScript : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";

    [Header("Dialogue")]
    [TextArea]
    [SerializeField] private string[] dialogueLines;

    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            if (dialogueLines != null && dialogueLines.Length > 0)
            {
                DialogueManager.Instance.StartDialogue(dialogueLines);
            }

            onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            onTriggerExit?.Invoke();
        }
    }
}
