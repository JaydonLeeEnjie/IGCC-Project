using UnityEngine;

public class BookTrigger : MonoBehaviour
{
    public GameObject exclamationMark; // !É}Å[ÉN

    private bool playerInRange = false;
    private Book book;

    void Start()
    {
        exclamationMark.SetActive(false);
        book = GetComponent<Book>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !book.isPlaced)
        {
            playerInRange = true;
            exclamationMark.SetActive(true);
            PlayerController.Instance.SetNearbyBook(book);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            exclamationMark.SetActive(false);
            PlayerController.Instance.ClearNearbyBook(book);
        }
    }
}
