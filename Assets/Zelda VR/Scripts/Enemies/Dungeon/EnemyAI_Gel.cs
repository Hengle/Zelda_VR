﻿using System.Collections;
using UnityEngine;

public class EnemyAI_Gel : EnemyAI
{
    void Start()
    {
        StartCoroutine("WaitForSpawnToFinish");
    }

    IEnumerator WaitForSpawnToFinish()
    {
        while (_enemy.enemyAnim.IsSpawning)
        {
            yield return new WaitForSeconds(0.02f);
        }
        AssignSkinForDungeonNum(WorldInfo.Instance.DungeonNum);
    }

    void AssignSkinForDungeonNum(int num)
    {
        if (num < 1 || num > 9) { num = 1; }
        AnimatorInstance.SetInteger("DungeonNum", num);
    }
}