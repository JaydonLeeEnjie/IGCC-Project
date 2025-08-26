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
        // �͂ߍ��݌�A���o���Ȃ��
        if (placed && shouldRotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (placed) return; // ���ɔz�u�ς݂Ȃ瓮�����Ȃ�
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
            // �������ʒu�ɃX�i�b�v
            transform.position = correctSlot.position;
            placed = true;

            // PuzzleManager �ɒʒm
            PuzzleManager.Instance.PiecePlacedCorrectly(this);
        }
        else
        {
            // �����ʒu�ɖ߂�
            transform.position = startPosition;
        }
    }

    // PuzzleManager ����Ă΂��
    public void StartRotating()
    {
        shouldRotate = true;
    }
}
