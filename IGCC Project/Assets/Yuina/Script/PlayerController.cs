using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private float speed = 5.0f;
    private InputAction moveAction;
    private SpriteRenderer spriteRenderer;

    private bool canMove = true;
    private DialogueTrigger currentTrigger;

    // --- 本関連 ---
    private Book currentHeldBook = null;
    private Book nearbyBook = null;
    private Bookshelf nearbyShelf = null;
    [SerializeField]private Animator animator;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction.Enable();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            // --- ダイアログ ---
            if (currentTrigger != null && currentTrigger.IsPlayerInRange())
            {
                if (!DialogueManager.Instance.IsDialogueActive)
                {
                    SetCanMove(false);
                    DialogueManager.Instance.StartDialogue(currentTrigger.dialogue);
                }
                else
                {
                    DialogueManager.Instance.DisplayNextSentence();
                }
                return;
            }

            // --- 本を拾う ---
            if (currentHeldBook == null && nearbyBook != null && !nearbyBook.isPlaced)
            {
                PickUpBook(nearbyBook);
                return;
            }

            // --- 本を棚に置く ---
            if (currentHeldBook != null && nearbyShelf != null)
            {
                if (nearbyShelf.TryPlaceBook(currentHeldBook))
                {
                    currentHeldBook = null;
                }
                return;
            }
        }

        if (canMove)
        {
            var moveValue = moveAction.ReadValue<Vector2>();
            var move = new Vector3(moveValue.x, 0f, moveValue.y) * speed * Time.deltaTime;
            transform.Translate(move);


            if (moveValue.x > 0.01f) spriteRenderer.flipX = true;
            else if (moveValue.x < -0.01f) spriteRenderer.flipX = false;

        
        }
        var MoveValue = moveAction.ReadValue<Vector2>();
        if (MoveValue.x != 0 || MoveValue.y != 0)
        {
            animator.SetBool("IsWalking", true);
        }
        else if(MoveValue.x == 0 && MoveValue.y == 0)
        {
            animator.SetBool("IsWalking", false);
        }


    }

    public void SetCanMove(bool value) => canMove = value;

    // --- 本を持つ処理 ---
    private void PickUpBook(Book book)
    {
        currentHeldBook = book;
        book.transform.SetParent(transform);
        book.transform.localPosition = new Vector3(0, 2, 0); // 頭上に浮かせる
        book.transform.localRotation = Quaternion.identity;
    }

    public void SetNearbyBook(Book book) => nearbyBook = book;
    public void ClearNearbyBook(Book book)
    {
        if (nearbyBook == book) nearbyBook = null;
    }
        public void SetNearbyShelf(Bookshelf shelf)
    {
        nearbyShelf = shelf;
    }

    public void ClearNearbyShelf(Bookshelf shelf)
    {
        if (nearbyShelf == shelf)
            nearbyShelf = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<DialogueTrigger>(out DialogueTrigger trigger))
        {
            currentTrigger = trigger;
        }

        if (other.TryGetComponent<Bookshelf>(out Bookshelf shelf))
        {
            nearbyShelf = shelf;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<DialogueTrigger>(out DialogueTrigger trigger))
        {
            if (currentTrigger == trigger) currentTrigger = null;
        }

        if (other.TryGetComponent<Bookshelf>(out Bookshelf shelf))
        {
            if (nearbyShelf == shelf) nearbyShelf = null;
        }
    }
}
