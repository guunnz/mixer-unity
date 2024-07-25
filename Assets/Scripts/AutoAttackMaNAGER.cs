using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttackMaNAGER : MonoBehaviour
{
    static public AutoAttackMaNAGER instance;

    public GameObject ProjectileBird;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void SpawnProjectileBird(Transform fromPos, Transform targetPos)
    {
        ProjectileMover projectileMover =
            Instantiate(ProjectileBird, fromPos.transform.position, ProjectileBird.transform.rotation, null)
                .GetComponent<ProjectileMover>();
        projectileMover.MoveToTarget(targetPos, 0.56f);
    }
}