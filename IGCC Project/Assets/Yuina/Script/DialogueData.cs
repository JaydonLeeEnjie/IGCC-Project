using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string characterName;      // NPC��
    [TextArea(3, 5)]
    public string[] sentences;        // �y�[�W���Ƃ̕���
}
