// ClockFace.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ClockFace : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private string bulletLayerName = "Bullet";
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip HitSound;

    [Header("Ring Exit Settings")]
    [Tooltip("If true: Space may be held at exit; if false: must be pressed this frame.")]
    [SerializeField] private bool ringAllowHold = true;

    [Header("Debug")]
    [SerializeField] private bool debugRing = true;
    [SerializeField] private bool debugRingStaySpam = false; // off by default

    private int bulletLayer;
    private readonly HashSet<Collider2D> ringsInside = new HashSet<Collider2D>();

    private void Awake()
    {
        // Ensure THIS collider is a trigger
        var col = GetComponent<Collider2D>();
        if (col && !col.isTrigger)
            Debug.LogWarning("[ClockFace] Collider2D should be set as Trigger.");

        bulletLayer = LayerMask.NameToLayer(bulletLayerName);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsBulletLayer(other)) return;

        var bullet = GetBulletOnRoot(other);
        if (!bullet || bullet.Type != BulletData.BulletType.Ring) return;

        // If we spawned already overlapping, Enter won't fire—treat first Stay as enter.
        if (ringsInside.Add(other) && debugRing)
            Debug.Log($"[ClockFace] Ring ENTER (synthetic)  t={Time.time:F3}  name='{other.name}'  scale={other.transform.localScale}");

        if (debugRing && debugRingStaySpam)
            Debug.Log($"[ClockFace] Ring STAY               t={Time.time:F3}  name='{other.name}'  scale={other.transform.localScale}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsBulletLayer(other)) return;

        var bullet = GetBulletOnRoot(other);
        if (!bullet) return;

        switch (bullet.Type)
        {
            case BulletData.BulletType.Normal:
            case BulletData.BulletType.Weak:
                battleManager?.TakeDamage(bullet.Damage);
                Destroy(GetRootGO(other));
                if (audioSource && HitSound) audioSource.PlayOneShot(HitSound);
                break;

            case BulletData.BulletType.DontHit:
                Destroy(GetRootGO(other));
                break;

            case BulletData.BulletType.Ring:
                ringsInside.Add(other);
                if (debugRing)
                    Debug.Log($"[ClockFace] Ring ENTER (event)     t={Time.time:F3}  name='{other.name}'  scale={other.transform.localScale}");
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsBulletLayer(other)) return;

        var bullet = GetBulletOnRoot(other);
        if (!bullet || bullet.Type != BulletData.BulletType.Ring) return;

        ringsInside.Remove(other);

        bool spacePressed = Keyboard.current != null &&
                            (ringAllowHold ? Keyboard.current.spaceKey.isPressed
                                           : Keyboard.current.spaceKey.wasPressedThisFrame);

        if (debugRing)
        {
            Debug.Log($"[ClockFace] Ring EXIT               t={Time.time:F3}  name='{other.name}'  scale={other.transform.localScale}  " +
                      $"spacePressed={spacePressed}  allowHold={ringAllowHold}", other);
        }

        var root = GetRootGO(other);

        if (spacePressed)
        {
            if (debugRing) Debug.Log($"[ClockFace] Ring parry SUCCESS  destroyed '{root.name}'.");
            Destroy(root);
        }
        else
        {
            if (debugRing) Debug.Log($"[ClockFace] Ring parry FAIL  take {bullet.Damage} dmg, destroy '{root.name}'.");
            battleManager?.TakeDamage(bullet.Damage);
            Destroy(root);
            if (audioSource && HitSound) audioSource.PlayOneShot(HitSound);
        }
    }

    // --- Helpers ---
    private static GameObject GetRootGO(Collider2D col)
    {
        var rb = col.attachedRigidbody;
        return rb ? rb.gameObject : col.gameObject;
    }

    private static Bullet GetBulletOnRoot(Collider2D col)
    {
        var go = GetRootGO(col);
        return go ? go.GetComponent<Bullet>() : null;
    }

    private bool IsBulletLayer(Collider2D col)
        => bulletLayer != -1 && GetRootGO(col).layer == bulletLayer;
}
