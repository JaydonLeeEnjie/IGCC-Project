using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogue;
    public GameObject exclamationMark;

    private bool playerInRange = false;

    void Start()
    {
        exclamationMark.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected!");
            playerInRange = true;

            if (exclamationMark != null)
                exclamationMark.SetActive(true);
            else
                Debug.LogWarning("ExclamationMark がアサインされていません！");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            exclamationMark.SetActive(false);
        }
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
}
