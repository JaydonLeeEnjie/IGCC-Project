using UnityEngine;

public class Book : MonoBehaviour
{
    [Header("Correct Shelf ID (1 or 2)")]
    public int correctShelfId;

    [HideInInspector] public bool isPlaced = false; // �I�Ɏ��[�ς݂��ǂ���

    [SerializeField] private GameObject bookName;

    private void Start()
    {
        if (bookName != null)
            bookName.SetActive(false); // �����͔�\��
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlaced)
        {
            PlayerController.Instance.SetNearbyBook(this);
            if (bookName != null)
                bookName.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController.Instance.ClearNearbyBook(this);
            if (bookName != null)
                bookName.SetActive(false);
        }
    }
}
