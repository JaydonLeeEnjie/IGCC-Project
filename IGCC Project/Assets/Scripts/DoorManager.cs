using UnityEngine;
using UnityEngine.UI;

public class DoorManager : MonoBehaviour
{
    [SerializeField] public bool HasBoss;
    [SerializeField] public string BossSceneName;
    [SerializeField] public bool PuzzleCompleted;
    [SerializeField] public bool HasAreaText;
    [SerializeField] public Image AreaText;
    [SerializeField] public Image BossText;
    [SerializeField] public float AreaTextTiming;
    [SerializeField] public float BossTextTiming;
    [SerializeField] private Transform TeleportLocation;
    [SerializeField] private bool WillTeleport;
}
