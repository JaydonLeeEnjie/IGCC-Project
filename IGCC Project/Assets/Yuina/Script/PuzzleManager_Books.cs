using UnityEngine;
using System.Collections;
using TMPro;

public class PuzzleManager_Books : MonoBehaviour
{
    public static PuzzleManager_Books Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI completeText;  // TextMeshPro�p

    [Header("Shelves Root")]
    [SerializeField] private GameObject shelves;

    [Header("Door Reference")]
    [SerializeField] private DoorManager doorManager;   // �N���A������J����

    private int totalBooks;
    private int placedBooks;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (completeText != null)
            completeText.gameObject.SetActive(false);
    }

    void Start()
    {
        totalBooks = FindObjectsByType<Book>(FindObjectsSortMode.None).Length;
        placedBooks = 0;

        if (completeText != null)
            completeText.gameObject.SetActive(false);
    }

    /// <summary>
    /// �{�����������[���ꂽ���ɌĂ�
    /// </summary>
    public void BookPlaced()
    {
        placedBooks++;
        Debug.Log($"Book placed! {placedBooks}/{totalBooks}");

        if (placedBooks >= totalBooks)
        {
            StartCoroutine(ShowCompleteText());

            // �p�Y���N���A��DoorManager�ɒʒm
            if (doorManager != null)
            {
                doorManager.PuzzleCompleted = true;
                Debug.Log("Puzzle Completed! Door can now be opened.");
            }
            else
            {
                Debug.LogWarning("DoorManager��PuzzleManager_Books�ɐݒ肳��Ă��܂���I");
            }
        }
    }

    private IEnumerator ShowCompleteText()
    {
        completeText.gameObject.SetActive(true);
        Color c = completeText.color;
        c.a = 1f;
        completeText.color = c;

        yield return new WaitForSeconds(1f);

        float duration = 1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, time / duration);
            completeText.color = c;
            yield return null;
        }

        completeText.gameObject.SetActive(false);
        Debug.Log("Books Puzzle Completed!");
    }
}
