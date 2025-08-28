using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorManager : MonoBehaviour
{
    [Header("Boss / Puzzle Settings")]
    [SerializeField] public bool HasBoss;
    [SerializeField] public string BossSceneName;
    [SerializeField] public bool PuzzleCompleted;

    [Header("UI Settings")]
    [SerializeField] public bool HasAreaText;
    [SerializeField] public Image AreaText;
    [SerializeField] public Image BossText;
    [SerializeField] public float AreaTextTiming = 2f;
    [SerializeField] public float BossTextTiming = 2f;

    [Header("Teleport Settings")]
    [SerializeField] private Transform TeleportLocation;
    [SerializeField] private bool WillTeleport;

    private bool playerInside = false;
    private GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = false;
            player = null;
        }
    }

    private void Update()
    {
        if (playerInside && PuzzleCompleted && Input.GetKeyDown(KeyCode.E))
        {
            if (HasBoss)
                StartCoroutine(HandleBossSequence());
            else
                StartCoroutine(HandleAreaSequence());
        }
    }

    private IEnumerator HandleBossSequence()
    {
        // Show boss text
        if (BossText != null)
        {
            BossText.gameObject.SetActive(true);
            yield return new WaitForSeconds(BossTextTiming);
            BossText.gameObject.SetActive(false);
        }

        // Save current scene and player position
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetFloat("PlayerPosX", player.transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", player.transform.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", player.transform.position.z);

        // Load boss scene
        SceneManager.LoadScene(BossSceneName);
    }

    private IEnumerator HandleAreaSequence()
    {
        // Show area text (only if enabled in inspector)
        if (HasAreaText && AreaText != null)
        {
            AreaText.gameObject.SetActive(true);
            yield return new WaitForSeconds(AreaTextTiming);
            AreaText.gameObject.SetActive(false);
        }

        // Teleport player if enabled
        if (WillTeleport && player != null && TeleportLocation != null)
        {
            player.transform.position = TeleportLocation.position;
            player.transform.rotation = TeleportLocation.rotation;
        }
    }

    // Call this after defeating boss and returning to saved scene
    public IEnumerator HandleReturnFromBoss(GameObject returningPlayer)
    {
        // Teleport player to new location
        if (WillTeleport && TeleportLocation != null)
        {
            returningPlayer.transform.position = TeleportLocation.position;
            returningPlayer.transform.rotation = TeleportLocation.rotation;
        }

        // Show area text afterwards
        if (HasAreaText && AreaText != null)
        {
            AreaText.gameObject.SetActive(true);
            yield return new WaitForSeconds(AreaTextTiming);
            AreaText.gameObject.SetActive(false);
        }
    }
}