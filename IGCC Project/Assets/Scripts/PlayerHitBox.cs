using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PlayerHitBox : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private PlayerInput playerInput;

    [Header("Bullet Filtering")]
    [SerializeField] private string bulletLayerName = "Bullet";
    [SerializeField] private bool requireLayerMatch = true;

    [Header("Parry / Jump Window")]
    [Tooltip("How long after pressing Jump the parry (bullet destroy) window stays active.")]
    [SerializeField] private float parryBufferSeconds = 0.15f;
    [Tooltip("Also allow if Jump is currently held down at impact.")]
    [SerializeField] private bool allowHoldAtImpact = true;
    [SerializeField] private string jumpActionName = "Jump";

    private int bulletLayer = -1;
    private InputAction jumpAction;
    private float parryTimer; // counts down from parryBufferSeconds after a Jump press

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col && !col.isTrigger)
            Debug.LogWarning("[PlayerHitBox] Collider2D is not set as Trigger.");

        bulletLayer = LayerMask.NameToLayer(bulletLayerName);
    }

    private void OnEnable()
    {
        if (playerInput && playerInput.actions != null)
        {
            jumpAction = playerInput.actions.FindAction(jumpActionName, throwIfNotFound: false);
            jumpAction?.Enable();
            if (jumpAction != null)
                jumpAction.performed += OnJumpPerformed;
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.Disable();
        }
    }

    private void Update()
    {
        if (parryTimer > 0f)
            parryTimer -= Time.unscaledDeltaTime; // UI-style timing; use Time.deltaTime if you prefer
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        parryTimer = parryBufferSeconds;
    }

    private void OnTriggerEnter2D(Collider2D other) { Handle(other); }
    private void OnTriggerStay2D(Collider2D other) { Handle(other); } // covers spawn-inside cases

    private void Handle(Collider2D other)
    {
        // Resolve the bullet's root (in case collider is on a child)
        var rb = other.attachedRigidbody;
        var hitGO = rb ? rb.gameObject : other.gameObject;

        if (requireLayerMatch && bulletLayer != -1 && hitGO.layer != bulletLayer)
            return;

        if (!hitGO.TryGetComponent<Bullet>(out var bullet))
            return;

        switch (bullet.Type)
        {
            case BulletData.BulletType.Normal:
                {
                    bool pressedRecently = parryTimer > 0f;
                    bool heldNow = allowHoldAtImpact && jumpAction != null && jumpAction.IsPressed();

                    if (pressedRecently || heldNow)
                    {
                        // Successful parry of NORMAL bullet: destroy it.
                        Destroy(hitGO);
                        parryTimer = 0f; // consume the buffer (optional)
                    }
                    // else: do nothing here (let other colliders/logic handle it if relevant)
                    break;
                }

            case BulletData.BulletType.DontHit:
                {
                    // This variant damages the player on contact
                    battleManager?.TakeDamage(bullet.Damage);
                    Destroy(hitGO);
                    break;
                }
        }
    }
}
