using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // --- シングルトン追加 ---
    public static PlayerController Instance { get; private set; }

    private float m_speed = 5.0f;
    private InputAction m_moveAction;
    private SpriteRenderer m_spriteRenderer;

    private bool canMove = true;
    private DialogueTrigger currentTrigger;

    void Awake()
    {
        // シングルトンのセット
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        m_moveAction = InputSystem.actions.FindAction("Move");
        m_moveAction.Enable();

        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
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
            }
        }

        if (canMove)
        {
            var moveValue = m_moveAction.ReadValue<Vector2>();
            var move = new Vector3(moveValue.x, 0f, moveValue.y) * m_speed * Time.deltaTime;
            transform.Translate(move);

            if (moveValue.x > 0.01f)
            {
                m_spriteRenderer.flipX = true;
            }
            else if (moveValue.x < -0.01f)
            {
                m_spriteRenderer.flipX = false;
            }
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<DialogueTrigger>(out DialogueTrigger trigger))
        {
            currentTrigger = trigger;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<DialogueTrigger>(out DialogueTrigger trigger))
        {
            if (currentTrigger == trigger) currentTrigger = null;
        }
    }
}
