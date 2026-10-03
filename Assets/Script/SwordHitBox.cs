using System.Collections.Generic;
using UnityEngine;

public class SwordHitBox : MonoBehaviour
{
    [SerializeField] private int damage = 20;

    private Collider hitCollider;

    private HashSet<Enemy> hitEnemies =
        new HashSet<Enemy>();

    private void Awake()
    {
        hitCollider = GetComponent<Collider>();

        if (hitCollider == null)
        {
            Debug.LogError(
                "SwordHitBox: Không tìm thấy Collider!"
            );

            return;
        }

        DisableHitBox();
    }

    public void EnableHitBox()
    {
        if (hitCollider == null)
            return;

        hitEnemies.Clear();

        hitCollider.enabled = true;

        Debug.Log("=== SWORD HITBOX ENABLED ===");
    }

    public void DisableHitBox()
    {
        if (hitCollider == null)
            return;

        hitCollider.enabled = false;

        hitEnemies.Clear();

        Debug.Log("=== SWORD HITBOX DISABLED ===");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "Sword HitBox detected: " +
            other.name
        );

        Enemy enemy =
            other.GetComponentInParent<Enemy>();

        if (enemy == null)
        {
            Debug.Log(
                "Collider " +
                other.name +
                " không có Enemy ở Parent"
            );

            return;
        }

        if (hitEnemies.Contains(enemy))
        {
            return;
        }

        hitEnemies.Add(enemy);

        Debug.Log(
            "Sword hit " +
            enemy.name +
            " for " +
            damage +
            " damage"
        );

        enemy.TakeDamage(damage);
    }
}