using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField]
    private float typingSpeed = 0.05f;

    private TMP_Text dialogueText;

    private Queue<string[]> dialogueQueue = new Queue<string[]>();
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

    private void BeginDialogue(string[] lines)
    {
        currentLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;

        gameObject.SetActive(true);
        ShowCurrentLine();
    }

    public void StartDialogue(string[] lines)
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

        if (currentLineIndex >= currentLines.Length - 1)
        {
            yield return new WaitForSeconds(2f);
            EndDialogue();
        }
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
