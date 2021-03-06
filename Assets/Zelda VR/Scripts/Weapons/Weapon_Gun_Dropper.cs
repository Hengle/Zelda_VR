﻿using UnityEngine;

public class Weapon_Gun_Dropper : Weapon_Gun
{
    public float dropDistance = 1.0f;
    public bool modifyDropDistIfRaycastHit = true;


    override protected Projectile_Base FireProjectile(Vector3 dir)
    {
        Projectile_Base p = base.FireProjectile(dir);
        MoveProjectileToDropPosition(p);
        return p;
    }

    void MoveProjectileToDropPosition(Projectile_Base p)
    {
        float actualDropDistance = dropDistance;

        if (modifyDropDistIfRaycastHit)
        {
            float halfLength = p.Length * 0.5f;

            Ray ray = new Ray(p.transform.position, p.MoveDirection);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(ray, out hitInfo, dropDistance + halfLength);

            actualDropDistance = hit ? (hitInfo.distance - halfLength) : dropDistance;
        }

        p.transform.position += p.MoveDirection * actualDropDistance;
    }
}