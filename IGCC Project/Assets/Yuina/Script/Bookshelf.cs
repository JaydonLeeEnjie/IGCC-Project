using UnityEngine;

public class Bookshelf : MonoBehaviour
{
    [Header("Shelf ID (1 or 2)")]
    public int shelfId;

    [Header("Slot Points (Slot1~3)")]
    [SerializeField] private Transform[] slotPoints;

    [SerializeField] private GameObject bookshelfName;

    private int currentIndex = 0;

    private void Start()
    {
        if (bookshelfName != null)
            bookshelfName.SetActive(false); // �����͔�\��
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController.Instance.SetNearbyShelf(this);
            if (bookshelfName != null)
                bookshelfName.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController.Instance.ClearNearbyShelf(this);
            if (bookshelfName != null)
                bookshelfName.SetActive(false);
        }
    }


    public bool TryPlaceBook(Book book)
    {
        // �Ԉ�����I�͋���
        if (book.correctShelfId != shelfId) return false;

        // �󂫂��Ȃ���΋���
        if (currentIndex >= slotPoints.Length) return false;

        // �z�u����
        Transform slot = slotPoints[currentIndex];
        book.transform.SetParent(transform);
        book.transform.position = slot.position;
        book.transform.rotation = slot.rotation;
        book.isPlaced = true;

        currentIndex++;

        PuzzleManager_Books.Instance.BookPlaced();
        return true;
    }
}
