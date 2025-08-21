// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 6f;

    public BulletData.BulletType Type { get; private set; }
    public float Damage { get; private set; }

    private Vector3 velocity;

    public void Init(Vector3 worldDirection, float speed, float damage, BulletData.BulletType type)
    {
        Type = type;
        Damage = damage;

        worldDirection.Normalize();
        transform.right = -worldDirection;          // face travel dir (+X)
        velocity = worldDirection * -speed;

        Destroy(gameObject, lifeSeconds);
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime; // world-space move
    }
}
