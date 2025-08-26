using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private GameObject puzzleUI;
    [SerializeField] private GameObject exclamationMark;

    private bool playerInRange = false;

    void Start()
    {
        if (exclamationMark != null)
            exclamationMark.SetActive(false); // �ŏ��͔�\��
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (exclamationMark != null)
                exclamationMark.SetActive(true); // !�}�[�N�\��
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (exclamationMark != null)
                exclamationMark.SetActive(false); // !�}�[�N��\��
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Return))
        {
            // �~�j�Q�[���J�n
            puzzleUI.SetActive(true);
            PuzzleManager.Instance.StartPuzzle();

            // �v���C���[�̓������~�߂�
            PlayerController.Instance.SetCanMove(false);

            // !�}�[�N�͏���
            if (exclamationMark != null)
                exclamationMark.SetActive(false);
        }
    }
}
