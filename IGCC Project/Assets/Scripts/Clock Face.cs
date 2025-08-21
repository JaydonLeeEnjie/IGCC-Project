using UnityEngine;

public class ClockFace : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private string bulletLayerName = "Bullet"; // set to your Bullet layer name

    private int bulletLayer;

    private void Awake()
    {
        bulletLayer = LayerMask.NameToLayer(bulletLayerName);
    }

    private void OnTriggerEnter2D(Collider2D other)   // Use OnTriggerEnter2D if you're using 2D physics
    {
        if (other.gameObject.layer != bulletLayer) return;

        var bullet = other.GetComponent<Bullet>();
        if (!bullet) return;

        switch (bullet.Type)
        {
            case BulletData.BulletType.Normal:
                if (battleManager != null)
                {
                    battleManager.TakeDamage(bullet.Damage);
                }
                Destroy(other.gameObject);
                break;

            case BulletData.BulletType.DontHit:
                Destroy(other.gameObject);
                break;
        }
    }
}
