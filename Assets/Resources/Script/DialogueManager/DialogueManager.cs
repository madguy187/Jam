using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] public float typingSpeed = 0.04f;

    private string[] lines;
    private int currentLine = 0;
    private System.Action onDialogueComplete;
    private bool isTyping = false; // To prevent starting next coroutine while typing

    public void StartDialogue(string[] dialogueLines, System.Action onComplete = null)
    {
        lines = dialogueLines;
        currentLine = 0;
        onDialogueComplete = onComplete;
        dialogueText.text = "";
        StopAllCoroutines();
        isTyping = false;
        gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in lines[currentLine])
        {
            dialogueText.text += c;
            if (AudioManager.instance != null) {
                AudioManager.instance.Play("Typing");
            }
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || dialogueText == null || lines == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = lines[currentLine];
                isTyping = false;
            }
            else
            {
                currentLine++;
                if (currentLine < lines.Length)
                {
                    StartCoroutine(TypeLine());
                }
                else
                {
                    onDialogueComplete?.Invoke();
                    gameObject.SetActive(false); // Hide or disable UI
                }
            }
        }
    }
}
