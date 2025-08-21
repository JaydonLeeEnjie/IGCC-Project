using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls; // for KeyControl/Key

[RequireComponent(typeof(Collider2D))]
public class PlayerHitBox : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ParrySound;

    [Header("Bullet Filtering")]
    [SerializeField] private string bulletLayerName = "Bullet";
    [SerializeField] private bool requireLayerMatch = true;

    [Header("Parry / Jump Window")]
    [SerializeField] private float parryBufferSeconds = 0.15f;
    [SerializeField] private bool allowHoldAtImpact = true;
    [SerializeField] private string jumpActionName = "Jump";

    [Header("HitBox Sizing (X only, localScale.x ABSOLUTE values)")]
    [SerializeField] private float normalHitBoxSize = 1f; // localScale.x when not holding RMB
    [SerializeField] private float largeHitBoxSize = 2f; // localScale.x when holding RMB
    [SerializeField] private float resizeSpeed = 20f;     // higher = snappier

    private int bulletLayer = -1;
    private InputAction jumpAction;
    private float parryTimer;

    private Vector3 baseScale;    // cached Y/Z to preserve
    private float currentX;       // interpolated localScale.x

    private void Awake()
    {
        // Collider sanity
        var col = GetComponent<Collider2D>();
        if (col && !col.isTrigger)
            Debug.LogWarning("[PlayerHitBox] Collider2D is not set as Trigger.");

        bulletLayer = LayerMask.NameToLayer(bulletLayerName);

        // Cache initial scale
        baseScale = transform.localScale;
        currentX = baseScale.x;

        // Optional: if you want the normal size to match the current object scale in editor
        if (Mathf.Approximately(normalHitBoxSize, 0f))
            normalHitBoxSize = baseScale.x;
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
        if (battleManager.inCombat)
        {
            // Right mouse ONLY controls enlargement of localScale.x
            bool rightMouseHeld = Mouse.current != null && Mouse.current.rightButton.isPressed;
            float targetX = rightMouseHeld ? largeHitBoxSize : normalHitBoxSize;

            // Smoothly lerp localScale.x
            currentX = Mathf.MoveTowards(currentX, targetX, resizeSpeed * Time.unscaledDeltaTime);
            transform.localScale = new Vector3(currentX, baseScale.y, baseScale.z);
        }
        else
        {
            transform.localScale = new Vector3(normalHitBoxSize , baseScale.y, baseScale.z);
        }

        // Spacebar-only parry buffer countdown
        if (parryTimer > 0f)
            parryTimer -= Time.unscaledDeltaTime;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        // Only accept keyboard Space as the source of the action
        if (ctx.control is KeyControl key && key.keyCode == Key.Space)
            parryTimer = parryBufferSeconds;
    }

    private void OnTriggerEnter2D(Collider2D other) { Handle(other); }
    private void OnTriggerStay2D(Collider2D other) { Handle(other); }

    private void Handle(Collider2D other)
    {
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
                    bool heldNow = allowHoldAtImpact
                                   && Keyboard.current != null
                                   && Keyboard.current.spaceKey.isPressed;

                    if (pressedRecently || heldNow)
                    {
                        Destroy(hitGO);   // parry success
                        parryTimer = 0f;  // consume buffer (optional)
                        audioSource.PlayOneShot(ParrySound);
                    }
                    break;
                }

            case BulletData.BulletType.Weak:
                {
                    Destroy(hitGO);
                    audioSource.PlayOneShot(ParrySound);
                    break;
                }

            case BulletData.BulletType.DontHit:
                {
                    battleManager?.TakeDamage(bullet.Damage);
                    Destroy(hitGO);
                    break;
                }
        }
    }

    private void OnValidate()
    {
        if (largeHitBoxSize < normalHitBoxSize)
            largeHitBoxSize = normalHitBoxSize;
        if (resizeSpeed < 0f)
            resizeSpeed = 0f;
    }
}
