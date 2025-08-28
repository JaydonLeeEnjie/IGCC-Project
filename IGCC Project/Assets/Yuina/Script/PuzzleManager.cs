using UnityEngine;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Puzzle UI Root")]
    [SerializeField] private GameObject puzzleUI;  // �p�Y��UI�̐e (Canvas����)

    [Header("Puzzle Board (slots parent)")]
    [SerializeField] private GameObject puzzleBoard; // �X���b�g�������Ă���e

    [Header("Puzzle Trigger Cube (trigger object)")]
    [SerializeField] private GameObject puzzleTriggerCube; // �~�j�Q�[���J�n�p�̔�

    [Header("Door Reference")]
    [SerializeField] private DoorManager doorManager;   // �N���A������J����

    private int totalPieces;     // �S�p�[�c��
    private int placedPieces;    // �͂܂����p�[�c��
    private PuzzlePiece[] allPieces; // �SPuzzlePiece�̎Q��

    private bool puzzleCleared = false; // �N���A�ς݂��ǂ���

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // �ŏ���UI���\���ɂ��Ă���
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }
    }

    /// <summary>
    /// �p�Y���J�n
    /// </summary>
    public void StartPuzzle()
    {
        if (puzzleCleared)
        {
            Debug.Log("Puzzle already cleared. Cannot play again.");
            return;
        }

        if (puzzleUI == null)
        {
            Debug.LogError("Puzzle UI ���A�T�C������Ă��܂���I");
            return;
        }

        puzzleUI.SetActive(true);
        PlayerController.Instance.SetCanMove(false);

        // �q�I�u�W�F�N�g����S PuzzlePiece �𐔂���
        allPieces = puzzleUI.GetComponentsInChildren<PuzzlePiece>();
        totalPieces = allPieces.Length;
        placedPieces = 0;

        Debug.Log("Puzzle Started! Pieces: " + totalPieces);

    }

    /// <summary>
    /// �p�[�c���������u���ꂽ���ɌĂ�
    /// </summary>
    public void PiecePlacedCorrectly(PuzzlePiece piece)
    {
        placedPieces++;
        Debug.Log("Piece Placed! (" + placedPieces + "/" + totalPieces + ")");

        if (placedPieces >= totalPieces)
        {
            // �S���������牉�o�J�n
            StartCoroutine(FinishWithAnimation());

            // �p�Y���N���A��DoorManager�ɒʒm
            if (doorManager != null)
            {
                doorManager.PuzzleCompleted = true;
                Debug.Log("Puzzle Completed! Door can now be opened.");
            }
            else
            {
                Debug.LogWarning("DoorManager��PuzzleManager�ɐݒ肳��Ă��܂���I");
            }

        }
    }

    /// <summary>
    /// ���ԉ��o �� �p�Y���I��
    /// </summary>
    private IEnumerator FinishWithAnimation()
    {
        Debug.Log("All pieces placed! Starting gear animation...");

        // �X���b�g�摜���\���ɂ���i���������j
        if (puzzleBoard != null)
            puzzleBoard.SetActive(false);

        // �S�Ă̎��Ԃ���]�J�n
        foreach (var piece in allPieces)
        {
            piece.StartRotating();
        }

        // 2�b�ԑ҂�
        yield return new WaitForSeconds(2f);

        EndPuzzle();
    }

    /// <summary>
    /// �p�Y���I��
    /// </summary>
    public void EndPuzzle()
    {
        Debug.Log("Puzzle Completed!");

        if (puzzleUI != null)
            puzzleUI.SetActive(false);

        PlayerController.Instance.SetCanMove(true);

        // �N���A�ς݂ɂ���
        puzzleCleared = true;

        // puzzleTriggerCube �������i��x�ڂ̃v���C��h�~�j
        if (puzzleTriggerCube != null)
            puzzleTriggerCube.SetActive(false);
    }
}
