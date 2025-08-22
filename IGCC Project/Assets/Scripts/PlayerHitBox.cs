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
    [SerializeField] private float normalHitBoxSize = 1f;
    [SerializeField] private float largeHitBoxSize = 2f;
    [SerializeField] private float resizeSpeed = 20f;

    [Header("Hold (L2) Settings")]
    [Tooltip("Trigger value (0..1) above which L2 counts as 'held'.")]
    [SerializeField] private float l2Threshold = 0.5f;

    private int bulletLayer = -1;
    private InputAction jumpAction;
    private float parryTimer;

    private Vector3 baseScale;
    private float currentX;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col && !col.isTrigger)
            Debug.LogWarning("[PlayerHitBox] Collider2D is not set as Trigger.");

        bulletLayer = LayerMask.NameToLayer(bulletLayerName);

        baseScale = transform.localScale;
        currentX = baseScale.x;

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
            bool rightMouseHeld = Mouse.current != null && Mouse.current.rightButton.isPressed;
            float targetX = rightMouseHeld ? largeHitBoxSize : normalHitBoxSize;
            currentX = Mathf.MoveTowards(currentX, targetX, resizeSpeed * Time.unscaledDeltaTime);
            transform.localScale = new Vector3(currentX, baseScale.y, baseScale.z);
        }
        else
        {
            transform.localScale = new Vector3(normalHitBoxSize, baseScale.y, baseScale.z);
        }

        if (parryTimer > 0f)
            parryTimer -= Time.unscaledDeltaTime;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
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
                        Destroy(hitGO);
                        parryTimer = 0f;
                        audioSource?.PlayOneShot(ParrySound);
                    }
                    break;
                }

            case BulletData.BulletType.Weak:
                {
                    Destroy(hitGO);
                    audioSource?.PlayOneShot(ParrySound);
                    break;
                }

            case BulletData.BulletType.DontHit:
                {
                    battleManager?.TakeDamage(bullet.Damage);
                    Destroy(hitGO);
                    break;
                }

            case BulletData.BulletType.Hold:   // NEW
                {
                    // Only defend if L2 (left trigger) is held on the paired gamepad
                    if (IsL2Held())
                    {
                        Destroy(hitGO);                 // defended successfully
                        audioSource?.PlayOneShot(ParrySound);
                    }
                    else
                    {
                        battleManager?.TakeDamage(bullet.Damage); // not blocking  damage
                        Destroy(hitGO);
                    }
                    break;
                }
        }
    }

    private bool IsL2Held()
    {
        // Prefer the gamepad paired to this PlayerInput
        Gamepad gp = null;
        if (playerInput != null)
        {
            foreach (var dev in playerInput.devices)
            {
                if (dev is Gamepad g) { gp = g; break; }
            }
        }
        if (gp == null) gp = Gamepad.current; // fallback

        if (gp == null) return false; // no controller -> cannot block

        // leftTrigger returns 0..1; consider held if above threshold
        return gp.leftTrigger.ReadValue() >= l2Threshold;
    }

    private void OnValidate()
    {
        if (largeHitBoxSize < normalHitBoxSize)
            largeHitBoxSize = normalHitBoxSize;
        if (resizeSpeed < 0f)
            resizeSpeed = 0f;

        l2Threshold = Mathf.Clamp01(l2Threshold);
    }

    public void PlayParrySFX(Vector3 at)
    {
        if (ParrySound == null) return;
        if (audioSource != null) audioSource.PlayOneShot(ParrySound);
    }
}
