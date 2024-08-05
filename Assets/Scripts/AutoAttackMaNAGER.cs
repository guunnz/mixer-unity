using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttackMaNAGER : MonoBehaviour
{
    static public AutoAttackMaNAGER instance;

    public GameObject ProjectileBird;
    public GameObject MeleeAttack;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void SpawnProjectile(Transform fromPos, Transform targetPos, AxieClass @class)
    {
        ProjectileMover projectileMover =
            Instantiate(ProjectileBird, fromPos.transform.position, ProjectileBird.transform.rotation, null)
                .GetComponent<ProjectileMover>();

        projectileMover.GetComponent<ProjectileColor>().SetColor(@class);
        projectileMover.MoveToTarget(targetPos, 0.56f);
    }

    public void SpawnAttack(Transform targetPos, AxieClass @class)
    {
        MeleeAttack attack = Instantiate(MeleeAttack, targetPos.transform.position, Quaternion.identity, null)
                .GetComponent<MeleeAttack>();

        attack.gameObject.GetComponent<ProjectileColor>().SetColor(@class);
    }
}