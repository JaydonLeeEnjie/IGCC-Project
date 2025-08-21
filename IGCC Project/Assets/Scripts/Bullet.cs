using UnityEngine;
using UnityEngine.InputSystem;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 6f;

    [Header("Colors")]
    [SerializeField] private Color NormalColour;
    [SerializeField] private Color DontHitColour;
    [SerializeField] private Color WeakColour;
    [SerializeField] private Color RingColour;
    [SerializeField] private Color ClickColour;
    [SerializeField] private SpriteRenderer BulletSprite;

    [Header("Ring Settings")]
    [Tooltip("Ring resolves when min(localScale.x, localScale.y) <= this value.")]
    [SerializeField] private float ringTriggerScale;
    [Tooltip("If true: Space may be held; if false: Space must be pressed that frame.")]
    [SerializeField] private bool ringAllowHold = true;
    [SerializeField] private bool ringDebug = false;

    [Header("Click Settings")]
    [SerializeField] private float clickColliderSizeMultiplier = 1.5f; // Make click bullets easier to hit

    public BulletData.BulletType Type { get; private set; }
    public float Damage { get; private set; }

    private Vector3 velocity;   // non-ring
    private float shrinkSpeed;  // ring
    private bool ringResolved;  // ring: ensure single resolution
    private BattleManager bm;   // to apply damage when ring fails
    private PlayerHitBox playerHB; // for parry SFX
    private Camera mainCam;
    private Collider2D col2d;

    private void Awake()
    {
        bm = FindObjectOfType<BattleManager>();
        playerHB = FindObjectOfType<PlayerHitBox>();
        mainCam = Camera.main;
        col2d = GetComponent<Collider2D>();
    }

    public void Init(Vector3 worldDirection, float speed, float damage, BulletData.BulletType type, float ringtriggerscale)
    {
        Type = type;
        Damage = damage;
        ringTriggerScale = ringtriggerscale;

        // color per type
        switch (Type)
        {
            case BulletData.BulletType.Normal: BulletSprite.color = NormalColour; break;
            case BulletData.BulletType.DontHit: BulletSprite.color = DontHitColour; break;
            case BulletData.BulletType.Weak: BulletSprite.color = WeakColour; break;
            case BulletData.BulletType.Ring: BulletSprite.color = RingColour; break;
            case BulletData.BulletType.Click:
                BulletSprite.color = ClickColour;
                // Increase collider size for click bullets to make them easier to hit
                if (col2d is CircleCollider2D circleCollider)
                {
                    circleCollider.radius *= clickColliderSizeMultiplier;
                }
                else if (col2d is BoxCollider2D boxCollider)
                {
                    boxCollider.size *= clickColliderSizeMultiplier;
                }
                break;
        }

        if (Type == BulletData.BulletType.Ring)
        {
            shrinkSpeed = Mathf.Max(0f, speed); // scale-units/sec
            ringResolved = false;

            if (ringDebug)
                Debug.Log($"[Ring] Spawn @ scale={transform.localScale} threshold={ringTriggerScale}");

            if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds); // safety cap
            return; // rings don't translate
        }

        // Non-ring translation
        worldDirection.Normalize();
        transform.right = -worldDirection; // face travel dir (+X)
        velocity = worldDirection * -speed;

        if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
    }

    private void Update()
    {
        // CLICK bullet: delete on LMB if cursor overlaps this collider
        if (Type == BulletData.BulletType.Click)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && col2d != null)
            {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Ray ray = mainCam.ScreenPointToRay(screenPos);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                if (hit.collider != null && hit.collider == col2d)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (Type == BulletData.BulletType.Ring)
        {
            // shrink uniformly X/Y
            Vector3 s = transform.localScale;
            float ds = shrinkSpeed * Time.deltaTime;
            s.x = Mathf.Max(0f, s.x - ds);
            s.y = Mathf.Max(0f, s.y - ds);
            transform.localScale = s;

            // resolve at threshold once
            if (!ringResolved && Mathf.Min(s.x, s.y) <= ringTriggerScale)
            {
                ringResolved = true;

                bool spacePressed = Keyboard.current != null &&
                    (ringAllowHold ? Keyboard.current.spaceKey.isPressed
                                   : Keyboard.current.spaceKey.wasPressedThisFrame);

                if (ringDebug)
                    Debug.Log($"[Ring] Resolve @ scale={s} spacePressed={spacePressed} allowHold={ringAllowHold}");

                if (spacePressed)
                {
                    playerHB?.PlayParrySFX(transform.position);
                    Destroy(gameObject);
                }
                else
                {
                    bm?.TakeDamage(Damage);
                    Destroy(gameObject);
                }
            }
            return;
        }

        // Non-ring translate
        transform.position += velocity * Time.deltaTime;
    }
}