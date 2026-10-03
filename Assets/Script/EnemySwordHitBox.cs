using System.Collections.Generic;
using UnityEngine;

public class EnemySwordHitBox : MonoBehaviour
{
    [SerializeField] private int damage = 20;

    private Collider hitCollider;

    // Lưu những Player đã bị đánh trong lần Attack hiện tại
    private HashSet<PlayerMovement> hitPlayers =
        new HashSet<PlayerMovement>();

    private void Awake()
    {
        hitCollider = GetComponent<Collider>();

        if (hitCollider == null)
        {
            Debug.LogError(
                "EnemySwordHitBox: Không tìm thấy Collider!"
            );

            return;
        }

        DisableHitBox();
    }

    // =========================================================
    // ENABLE HIT BOX
    // =========================================================

    public void EnableHitBox()
    {
        if (hitCollider == null)
            return;

        // Cho phép Player bị đánh lại ở Attack tiếp theo
        hitPlayers.Clear();

        hitCollider.enabled = true;

        Debug.Log("=== ENEMY SWORD HITBOX ENABLED ===");
    }

    // =========================================================
    // DISABLE HIT BOX
    // =========================================================

    public void DisableHitBox()
    {
        if (hitCollider == null)
            return;

        hitCollider.enabled = false;

        hitPlayers.Clear();

        Debug.Log("=== ENEMY SWORD HITBOX DISABLED ===");
    }

    // =========================================================
    // TRIGGER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "Enemy Sword HitBox detected: " +
            other.name
        );

        // =====================================================
        // CHỈ NHẬN OBJECT CÓ TAG PLAYER
        // =====================================================

        if (!other.CompareTag("Player"))
        {
            Debug.Log(
                "Collider " +
                other.name +
                " không có tag Player"
            );

            return;
        }

        // =====================================================
        // TÌM PLAYER MOVEMENT
        // =====================================================

        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            Debug.Log(
                "Collider " +
                other.name +
                " có tag Player nhưng không tìm thấy PlayerMovement"
            );

            return;
        }

        // =====================================================
        // PLAYER ĐÃ BỊ ĐÁNH TRONG ATTACK NÀY
        // =====================================================

        if (hitPlayers.Contains(player))
        {
            return;
        }

        hitPlayers.Add(player);

        // =====================================================
        // GÂY DAMAGE
        // =====================================================

        Debug.Log(
            "Enemy sword hit Player " +
            player.name +
            " for " +
            damage +
            " damage"
        );

        player.TakeDamage(damage);
    }
}