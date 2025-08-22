using UnityEngine;
using UnityEngine.InputSystem; // for Keyboard/Mouse (already used)

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 6f;

    [Header("Colors")]
    [SerializeField] private Color NormalColour;
    [SerializeField] private Color DontHitColour;
    [SerializeField] private Color WeakColour;
    [SerializeField] private Color RingColour;
    [SerializeField] private Color ClickColour;
    [SerializeField] private Color HoldColour;               // NEW (optional)
    [SerializeField] private SpriteRenderer BulletSprite;

    [Header("Ring Settings")]
    [Tooltip("Ring resolves when min(localScale.x, localScale.y) <= this value.")]
    [SerializeField] private float ringTriggerScale;
    [Tooltip("If true: Space may be held; if false: Space must be pressed that frame.")]
    [SerializeField] private bool ringAllowHold = true;
    [SerializeField] private bool ringDebug = false;

    [Header("Click Settings")]
    [SerializeField] private float clickColliderSizeMultiplier = 1.5f;

    public BulletData.BulletType Type { get; private set; }
    public float Damage { get; private set; }

    private Vector3 velocity;
    private float shrinkSpeed;
    private bool ringResolved;
    private BattleManager bm;
    private PlayerHitBox playerHB;
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

        switch (Type)
        {
            case BulletData.BulletType.Normal: BulletSprite.color = NormalColour; break;
            case BulletData.BulletType.DontHit: BulletSprite.color = DontHitColour; break;
            case BulletData.BulletType.Weak: BulletSprite.color = WeakColour; break;
            case BulletData.BulletType.Ring: BulletSprite.color = RingColour; break;
            case BulletData.BulletType.Click:
                BulletSprite.color = ClickColour;
                if (col2d is CircleCollider2D circle) circle.radius *= clickColliderSizeMultiplier;
                else if (col2d is BoxCollider2D box) box.size *= clickColliderSizeMultiplier;
                break;
            case BulletData.BulletType.Hold: BulletSprite.color = HoldColour; break; // NEW
        }

        if (Type == BulletData.BulletType.Ring)
        {
            shrinkSpeed = Mathf.Max(0f, speed);
            ringResolved = false;
            if (ringDebug) Debug.Log($"[Ring] Spawn @ scale={transform.localScale} threshold={ringTriggerScale}");
            if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
            return;
        }

        worldDirection.Normalize();
        transform.right = -worldDirection;
        velocity = worldDirection * -speed;

        if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
    }

    private void Update()
    {
        // Click-to-destroy
        if (Type == BulletData.BulletType.Click)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && col2d != null)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Ray ray = mainCam.ScreenPointToRay(screenPos);
                var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
                if (hit.collider == col2d)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        // Ring shrink/resolve
        if (Type == BulletData.BulletType.Ring)
        {
            Vector3 s = transform.localScale;
            float ds = shrinkSpeed * Time.deltaTime;
            s.x = Mathf.Max(0f, s.x - ds);
            s.y = Mathf.Max(0f, s.y - ds);
            transform.localScale = s;

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

        // Normal/Weak/DontHit/Click/Hold movement
        transform.position += velocity * Time.deltaTime;
    }
}
