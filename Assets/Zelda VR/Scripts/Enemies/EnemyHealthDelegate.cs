﻿using UnityEngine;

public class EnemyHealthDelegate : MonoBehaviour
{
    const float PUSH_BACK_POWER = 5;


    public EnemyAnimation enemyAnim;
    public bool flashWhenDamaged = true;


    HealthController _healthController;


    void Awake()
    {
        _healthController = GetComponent<HealthController>();

        _healthController.DamageTaken += DamageTaken;
        _healthController.TempInvincibilityActivation += TempInvincibilityActivation;
        _healthController.Death += Death;
    }


    void DamageTaken(HealthController healthController, ref uint damageAmount, GameObject damageDealer)
    {
        Weapon_Melee_Sword sword = damageDealer.GetComponent<Weapon_Melee_Sword>();
        bool wasHitBySword = (sword != null);

        bool wasHitBySilverArrow = damageDealer.name.Contains("SilverArrow_Projectile");


        EnemyAI_Zol zol = GetComponent<EnemyAI_Zol>();
        if (zol != null)
        {
            if (damageAmount == 1)
            {
                zol.SpawnGels();
                healthController.Kill(damageDealer, true);
            }
        }

        EnemyAI_Dodongo dodongo = GetComponent<EnemyAI_Dodongo>();
        if (dodongo != null)
        {
            // Player can one-hit kill Dodongo when it stunned after eating a bomb
            if(dodongo.StunnedByBomb)
            {
                healthController.Kill(damageDealer, true);
            }
            else
            {
                damageAmount = 0;
                PlayShieldSound();
            }
        }

        EnemyAI_Gohma gohma = GetComponent<EnemyAI_Gohma>();
        if (gohma != null)
        {
            if (gohma.IsEyeClosed)
            {
                damageAmount = 0;
                PlayShieldSound();
            }
        }

        EnemyAI_Gannon gannon = GetComponent<EnemyAI_Gannon>();
        if (gannon != null)
        {
            if (wasHitBySword)
            {
                ZeldaHaptics.Instance.RumbleSimple_Right();
                SendMessage("OnHitWithSword", sword, SendMessageOptions.RequireReceiver);
            }
            else if (wasHitBySilverArrow)
            {
                ZeldaHaptics.Instance.RumbleSimple_Left();
                SendMessage("OnHitWithSilverArrow", SendMessageOptions.RequireReceiver);
            }

            damageAmount = 0;
        }

        if (damageAmount > 0)
        {
            if (GetComponent<Enemy>().pushBackOnhit)
            {
                if (DoesWeaponApplyPushbackForce(damageDealer))
                {
                    Vector3 direction = transform.position - damageDealer.transform.position;
                    direction.y = 0;
                    direction.Normalize();

                    Vector3 force = direction * PUSH_BACK_POWER;
                    GetComponent<Enemy>().Push(force);
                }
            }

            if (wasHitBySword)
            {
                ZeldaHaptics.Instance.RumbleSimple_Right();
            }
        }
    }

    void TempInvincibilityActivation(HealthController healthController, bool didActivate)
    {
        if (flashWhenDamaged && enemyAnim != null)
        {
            enemyAnim.ActivateFlash(didActivate);
        }
    }

    void Death(HealthController healthController, GameObject killer)
    {
        EnemyAI_Moldorm moldorm = GetComponent<EnemyAI_Moldorm>();
        if (moldorm != null)
        {
            OnMoldormDeath(moldorm);
            return;
        }

        EnemyAI_Vire vire = GetComponent<EnemyAI_Vire>();
        if (vire != null)
        {
            if (killer.name != "MagicSword_Weapon")
            {
                vire.SpawnKeese();
            }
        }

        EnemyAI_GleeokHead gleeokHead = GetComponent<EnemyAI_GleeokHead>();
        if (gleeokHead != null)
        {
            gleeokHead.gleeok.SendMessage("OnHeadDied", gleeokHead, SendMessageOptions.RequireReceiver);
        }

        EnemyAI_PatraSmall smallPatra = GetComponent<EnemyAI_PatraSmall>();
        if (smallPatra != null)
        {
            smallPatra.ParentPatra.SendMessage("OnSmallPatraDied", smallPatra, SendMessageOptions.RequireReceiver);
        }

        EnemyAI_DigdoggerSmall smallDigdogger = GetComponent<EnemyAI_DigdoggerSmall>();
        if (smallDigdogger != null)
        {
            smallDigdogger.ParentDigdogger.SendMessage("OnBabyDied", smallDigdogger, SendMessageOptions.RequireReceiver);
        }

        Enemy e = GetComponent<Enemy>();
        DungeonRoom dr = e.DungeonRoomRef;
        EnemyItemDrop itemDrop = GetComponent<EnemyItemDrop>();

        Enemy.EnemiesKilled++;
        Enemy.EnemiesKilledWithoutTakingDamage++;

        if (itemDrop != null) { itemDrop.DropRandomItem(); }
        if (dr != null) { dr.OnRoomEnemyDied(e); }
        if (enemyAnim != null) { enemyAnim.PlayDeathAnimation(); }

        SendMessage("OnEnemyDeath", SendMessageOptions.DontRequireReceiver);
    }
    void OnMoldormDeath(EnemyAI_Moldorm moldorm)
    {
        Enemy e = GetComponent<Enemy>();
        DungeonRoom dr = e.DungeonRoomRef;


        if (dr != null) { dr.OnRoomEnemyDied(e); }
        if (enemyAnim != null) { enemyAnim.PlayDeathAnimation(); }

        if (moldorm.IsLastWormPiece)
        {
            Enemy.EnemiesKilled++;
            Enemy.EnemiesKilledWithoutTakingDamage++;

            EnemyItemDrop itemDrop = GetComponent<EnemyItemDrop>();
            if (itemDrop != null) { itemDrop.DropRandomItem(); }
        }

        moldorm.OnDeath();
        return;
    }


    // TODO: this is a hack.  the Weapon should store whether it does pushback
    bool DoesWeaponApplyPushbackForce(GameObject weapon)
    {
        Weapon_Melee_Boomerang b = weapon.GetComponent<Weapon_Melee_Boomerang>();
        if (b != null) { return false; }

        Projectile_Flame f = weapon.GetComponent<Projectile_Flame>();
        if (f != null) { return false; }

        return true;
    }


    void PlayShieldSound()
    {
        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot(sfx.shield);
    }
}