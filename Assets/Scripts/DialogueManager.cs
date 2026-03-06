using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField]
    private float typingDelay = 0.03f;

    [SerializeField]
    private TMP_Text dialogueSpeaker;

    [SerializeField]
    private TMP_Text dialogueText;

    [SerializeField]
    private TMP_Text dialogueHint;

    [SerializeField]
    private string defaultHint;

    [SerializeField]
    private float defaultAdvanceDelay;

    private Queue<DialogueLine[]> dialogueQueue = new Queue<DialogueLine[]>();
    private DialogueLine[] currentLines;
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

        gameObject.SetActive(false);
    }

    private void BeginDialogue(DialogueLine[] lines)
    {
        currentLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;

        gameObject.SetActive(true);
        ShowCurrentLine();
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        // Queue the next dialogue if there is already one running
        if (isDialogueActive || isTyping)
        {
            dialogueQueue.Enqueue(lines);
            return;
        }

        BeginDialogue(lines);
    }

    private void Update()
    {
        if (!isDialogueActive)
            return;

        bool advancePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (advancePressed)
        {
            if (isTyping)
                FinishTyping();
            else
                AdvanceLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));
    }

    public IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;

        dialogueSpeaker.text = line.speaker;
        dialogueText.text = "";
        dialogueHint.text = "";

        dialogueText.color = line.dialogueColor;

        foreach (char c in line.dialogue)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingDelay);
        }

        isTyping = false;
        dialogueHint.text = string.IsNullOrEmpty(line.hint) ? defaultHint : line.hint;

        if (!line.advanceManually)
        {
            yield return new WaitForSeconds(
                line.advanceDelay <= 0f ? defaultAdvanceDelay : line.advanceDelay
            );

            if (currentLineIndex >= currentLines.Length - 1)
                EndDialogue();
            else
                AdvanceLine();
        }
    }

    public void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLines[currentLineIndex].dialogue;
        dialogueHint.text = string.IsNullOrEmpty(currentLines[currentLineIndex].hint)
            ? defaultHint
            : currentLines[currentLineIndex].hint;
        isTyping = false;
    }

    private void AdvanceLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
            ShowCurrentLine();
        else
            EndDialogue();
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        gameObject.SetActive(false);

        if (dialogueQueue.Count > 0)
        {
            BeginDialogue(dialogueQueue.Dequeue());
        }
    }

    public bool IsDialogueActive() => isDialogueActive;
}
