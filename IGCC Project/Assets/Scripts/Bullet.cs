// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 6f;
    [SerializeField] private Color NormalColour;
    [SerializeField] private Color DontHitColour;
    [SerializeField] private Color WeakColour;
    [SerializeField] private Color RingColour;
    [SerializeField] private SpriteRenderer BulletSprite;

    public BulletData.BulletType Type { get; private set; }
    public float Damage { get; private set; }

    private Vector3 velocity;
    private float shrinkSpeed;
    private Vector3 startScale;

    public void Init(Vector3 worldDirection, float speed, float damage, BulletData.BulletType type)
    {
        Type = type;
        Damage = damage;
        switch (Type)
        {
            case BulletData.BulletType.Normal:
                BulletSprite.color = NormalColour;
                break;

            case BulletData.BulletType.DontHit:
                BulletSprite.color = DontHitColour;
                break;
            case BulletData.BulletType.Weak:
                BulletSprite.color = WeakColour;
                break;
            case BulletData.BulletType.Ring:
                BulletSprite.color = RingColour; 
                break;
        }
        if (Type == BulletData.BulletType.Ring)
        {
            // Rings don't translate; they shrink using 'speed' (units of localScale per second)
            shrinkSpeed = Mathf.Max(0f, speed);
            startScale = transform.localScale;
            // lifeSeconds is still a safeguard in case something goes wrong
            Destroy(gameObject, lifeSeconds);
            return;
        }

        // Non-ring: move toward clock (your original logic, facing -direction)
        worldDirection.Normalize();
        transform.right = -worldDirection;   // face travel dir (+X)
        velocity = worldDirection * -speed;

        Destroy(gameObject, lifeSeconds);
    }

    private void Update()
    {
        if (Type == BulletData.BulletType.Ring)
        {
            // Shrink X and Y uniformly with 'speed' per second
            Vector3 s = transform.localScale;
            float d = shrinkSpeed * Time.deltaTime;
            s.x = Mathf.Max(0f, s.x - d);
            s.y = Mathf.Max(0f, s.y - d);
            transform.localScale = s;


            return;
        }

        // Non-ring movement
        transform.position += velocity * Time.deltaTime;
    }
}
