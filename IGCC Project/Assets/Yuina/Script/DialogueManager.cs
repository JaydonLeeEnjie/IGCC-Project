using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   // ← これを追加

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueUI;    // UIの親オブジェクト
    [SerializeField] private TMP_Text nameText;        // キャラ名
    [SerializeField] private TMP_Text dialogueText;    // 会話テキスト

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence;

    public static DialogueManager Instance; // シングルトン

    public bool IsDialogueActive => dialogueUI.activeSelf;  // 会話UIが表示されているか

    private void Awake()
    {
        if (Instance == null) Instance = this;
        sentences = new Queue<string>();
        dialogueUI.SetActive(false);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        dialogueUI.SetActive(true);
        nameText.text = dialogue.characterName;
        sentences.Clear();

        foreach (var sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            // 表示途中なら全文表示
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // 速度調整可
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueUI.SetActive(false);
        PlayerController.Instance.SetCanMove(true);
    }
}
