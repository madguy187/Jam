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

    public void StartDialogue(string[] dialogueLines, System.Action onComplete = null)
    {
        lines = dialogueLines;
        currentLine = 0;
        onDialogueComplete = onComplete;
        dialogueText.text = "";
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        dialogueText.text = "";
        foreach (char c in lines[currentLine])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (dialogueText.text == lines[currentLine])
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
            else
            {
                StopAllCoroutines();
                dialogueText.text = lines[currentLine];
            }
        }
    }
}
