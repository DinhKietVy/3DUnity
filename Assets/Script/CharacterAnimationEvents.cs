using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    private PlayerMovement player;
    private Enemy enemy;

    private void Awake()
    {
        player = GetComponentInParent<PlayerMovement>();
        enemy = GetComponentInParent<Enemy>();
    }

    // =========================================================
    // ATTACK
    // =========================================================

    public void StopAttack()
    {
        if (enemy != null)
        {
            enemy.StopAttack();
            return;
        }

        if (player != null)
        {
            player.StopAttack();
            return;
        }
    }

    // =========================================================
    // HIT
    // =========================================================

    public void StopHit()
    {
        if (enemy != null)
        {
            enemy.StopHit();
            return;
        }

        if (player != null)
        {
            player.StopHit();
            return;
        }
    }

    // =========================================================
    // DEAL DAMAGE
    // =========================================================

    public void DealDamage()
    {
        if (enemy != null)
        {
            enemy.DealDamage();
            return;
        }

        if (player != null)
        {
            player.DealDamage();
            return;
        }
    }

    // =========================================================
    // DEATH
    // =========================================================

    public void FinishDeath()
    {
        if (enemy != null)
        {
            enemy.FinishDeath();
            return;
        }

        if (player != null)
        {
            player.FinishDeath();
            return;
        }
    }

    // =========================================================
    // SWORD HIT BOX
    // =========================================================

    public void EnableSwordHitBox()
    {
        if (enemy != null)
        {
            enemy.EnableSwordHitBox();
            return;
        }

        if (player != null)
        {
            player.EnableSwordHitBox();
            return;
        }
    }

    public void DisableSwordHitBox()
    {
        if (enemy != null)
        {
            enemy.DisableSwordHitBox();
            return;
        }

        if (player != null)
        {
            player.DisableSwordHitBox();
            return;
        }
    }

    // =========================================================
    // JUMP
    // =========================================================

    public void StartJump()
    {
        if (enemy != null)
        {
            enemy.StartJump();
            return;
        }

        if (player != null)
        {
            player.StartJump();
            return;
        }
    }

    public void StopJump()
    {
        if (enemy != null)
        {
            enemy.StopJump();
            return;
        }

        if (player != null)
        {
            player.StopJump();
            return;
        }
    }
}