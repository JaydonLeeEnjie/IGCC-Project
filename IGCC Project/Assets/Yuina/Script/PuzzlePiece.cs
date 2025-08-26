using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Correct Slot (Assign in Inspector)")]
    public RectTransform correctSlot;

    [Header("Snap Distance")]
    public float snapDistance = 50f;

    [Header("Gear Rotation Speed (deg/sec, negative = reverse)")]
    public float rotationSpeed = 100f;

    private Vector3 startPosition;
    private Canvas canvas;
    private bool placed = false;
    private bool shouldRotate = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        // はめ込み後、演出中なら回す
        if (placed && shouldRotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (placed) return; // 既に配置済みなら動かせない
        startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (placed) return;
        transform.position += (Vector3)eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (placed) return;

        float distance = Vector3.Distance(transform.position, correctSlot.position);

        if (distance < snapDistance)
        {
            // 正しい位置にスナップ
            transform.position = correctSlot.position;
            placed = true;

            // PuzzleManager に通知
            PuzzleManager.Instance.PiecePlacedCorrectly(this);
        }
        else
        {
            // 初期位置に戻す
            transform.position = startPosition;
        }
    }

    // PuzzleManager から呼ばれる
    public void StartRotating()
    {
        shouldRotate = true;
    }
}
