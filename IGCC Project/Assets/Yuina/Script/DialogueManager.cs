using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   // �� �����ǉ�

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueUI;    // UI�̐e�I�u�W�F�N�g
    [SerializeField] private TMP_Text nameText;        // �L������
    [SerializeField] private TMP_Text dialogueText;    // ��b�e�L�X�g

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence;

    public static DialogueManager Instance; // �V���O���g��

    public bool IsDialogueActive => dialogueUI.activeSelf;  // ��bUI���\������Ă��邩

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
            // �\���r���Ȃ�S���\��
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
            yield return new WaitForSeconds(0.05f); // ���x������
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueUI.SetActive(false);
        PlayerController.Instance.SetCanMove(true);
    }
}
