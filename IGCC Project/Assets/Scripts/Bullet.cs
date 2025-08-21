using UnityEngine;
using UnityEngine.InputSystem; // for Keyboard

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 6f;

    [Header("Colors")]
    [SerializeField] private Color NormalColour;
    [SerializeField] private Color DontHitColour;
    [SerializeField] private Color WeakColour;
    [SerializeField] private Color RingColour;
    [SerializeField] private SpriteRenderer BulletSprite;

    [Header("Ring Settings")]
    [Tooltip("Ring resolves when min(localScale.x, localScale.y) <= this value.")]
    [SerializeField] private float ringTriggerScale;
    [Tooltip("If true: Space may be held; if false: Space must be pressed that frame.")]
    [SerializeField] private bool ringAllowHold = true;
    [SerializeField] private bool ringDebug = false;

    public BulletData.BulletType Type { get; private set; }
    public float Damage { get; private set; }

    private Vector3 velocity;   // non-ring
    private float shrinkSpeed;  // ring
    private bool ringResolved;  // ring: ensure single resolution
    private BattleManager bm;   // to apply damage when ring fails
    private PlayerHitBox playerHB;   // NEW: to play parry SFX

    private void Awake()
    {
        bm = FindObjectOfType<BattleManager>();
        playerHB = FindObjectOfType<PlayerHitBox>(); // cache once; or expose as [SerializeField] if you prefer
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
        }

        if (Type == BulletData.BulletType.Ring)
        {
            // interpret 'speed' as scale-units per second
            shrinkSpeed = Mathf.Max(0f, speed);
            ringResolved = false;

            if (ringDebug)
                Debug.Log($"[Ring] Spawn @ scale={transform.localScale} threshold={ringTriggerScale}");

            if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds); // safety cap
            return; // rings don't translate
        }

        // Non-ring movement toward clock
        worldDirection.Normalize();
        transform.right = -worldDirection; // face travel dir (+X)
        velocity = worldDirection * -speed;

        if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
    }

    private void Update()
    {
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
                    // success
                    playerHB?.PlayParrySFX(transform.position);
                    Destroy(gameObject);
                }
                else
                {
                    // fail: damage then destroy
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
