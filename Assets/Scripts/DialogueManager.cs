using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.03f;

    private TMP_Text dialogueText;
    private string[] currentLines;
    private int currentLineIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        dialogueText = GetComponentInChildren<TMP_Text>();
        gameObject.SetActive(false);
    }

    public void StartDialogue(string[] lines)
    {
        if (isDialogueActive) return;

        currentLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;

        gameObject.SetActive(true);
        ShowCurrentLine();
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        bool advancePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (advancePressed)
        {
            if (isTyping)
            {
                FinishTyping();
            }
            else
            {
                AdvanceLine();
            }
        }
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));
    }

    public IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLines[currentLineIndex];
        isTyping = false;
    }

    private void AdvanceLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        gameObject.SetActive(false);
    }

    public bool IsDialogueActive() => isDialogueActive;
}
