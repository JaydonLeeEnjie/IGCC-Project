using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private GameObject puzzleUI;
    [SerializeField] private GameObject exclamationMark;

    private bool playerInRange = false;

    void Start()
    {
        if (exclamationMark != null)
            exclamationMark.SetActive(false); // 最初は非表示
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (exclamationMark != null)
                exclamationMark.SetActive(true); // !マーク表示
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (exclamationMark != null)
                exclamationMark.SetActive(false); // !マーク非表示
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Return))
        {
            // ミニゲーム開始
            puzzleUI.SetActive(true);
            PuzzleManager.Instance.StartPuzzle();

            // プレイヤーの動きを止める
            PlayerController.Instance.SetCanMove(false);

            // !マークは消す
            if (exclamationMark != null)
                exclamationMark.SetActive(false);
        }
    }
}
