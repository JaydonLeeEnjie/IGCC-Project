using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string characterName;      // NPC–¼
    [TextArea(3, 5)]
    public string[] sentences;        // ƒy[ƒW‚²‚Æ‚Ì•¶Í
}
