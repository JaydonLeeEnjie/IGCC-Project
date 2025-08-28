using UnityEngine;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Puzzle UI Root")]
    [SerializeField] private GameObject puzzleUI;  // パズルUIの親 (Canvasごと)

    [Header("Puzzle Board (slots parent)")]
    [SerializeField] private GameObject puzzleBoard; // スロットが入っている親

    [Header("Puzzle Trigger Cube (trigger object)")]
    [SerializeField] private GameObject puzzleTriggerCube; // ミニゲーム開始用の箱

    [Header("Door Reference")]
    [SerializeField] private DoorManager doorManager;   // クリアしたら開く扉

    private int totalPieces;     // 全パーツ数
    private int placedPieces;    // はまったパーツ数
    private PuzzlePiece[] allPieces; // 全PuzzlePieceの参照

    private bool puzzleCleared = false; // クリア済みかどうか

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

        // 最初はUIを非表示にしておく
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }
    }

    /// <summary>
    /// パズル開始
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
            Debug.LogError("Puzzle UI がアサインされていません！");
            return;
        }

        puzzleUI.SetActive(true);
        PlayerController.Instance.SetCanMove(false);

        // 子オブジェクトから全 PuzzlePiece を数える
        allPieces = puzzleUI.GetComponentsInChildren<PuzzlePiece>();
        totalPieces = allPieces.Length;
        placedPieces = 0;

        Debug.Log("Puzzle Started! Pieces: " + totalPieces);

    }

    /// <summary>
    /// パーツが正しく置かれた時に呼ぶ
    /// </summary>
    public void PiecePlacedCorrectly(PuzzlePiece piece)
    {
        placedPieces++;
        Debug.Log("Piece Placed! (" + placedPieces + "/" + totalPieces + ")");

        if (placedPieces >= totalPieces)
        {
            // 全部揃ったら演出開始
            StartCoroutine(FinishWithAnimation());

            // パズルクリアをDoorManagerに通知
            if (doorManager != null)
            {
                doorManager.PuzzleCompleted = true;
                Debug.Log("Puzzle Completed! Door can now be opened.");
            }
            else
            {
                Debug.LogWarning("DoorManagerがPuzzleManagerに設定されていません！");
            }

        }
    }

    /// <summary>
    /// 歯車演出 → パズル終了
    /// </summary>
    private IEnumerator FinishWithAnimation()
    {
        Debug.Log("All pieces placed! Starting gear animation...");

        // スロット画像を非表示にする（穴を消す）
        if (puzzleBoard != null)
            puzzleBoard.SetActive(false);

        // 全ての歯車を回転開始
        foreach (var piece in allPieces)
        {
            piece.StartRotating();
        }

        // 2秒間待つ
        yield return new WaitForSeconds(2f);

        EndPuzzle();
    }

    /// <summary>
    /// パズル終了
    /// </summary>
    public void EndPuzzle()
    {
        Debug.Log("Puzzle Completed!");

        if (puzzleUI != null)
            puzzleUI.SetActive(false);

        PlayerController.Instance.SetCanMove(true);

        // クリア済みにする
        puzzleCleared = true;

        // puzzleTriggerCube を消す（二度目のプレイを防止）
        if (puzzleTriggerCube != null)
            puzzleTriggerCube.SetActive(false);
    }
}
