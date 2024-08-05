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
            Instantiate(ProjectileBird, new Vector3(fromPos.transform.position.x, fromPos.transform.position.y + 0.25f, fromPos.transform.position.z), ProjectileBird.transform.rotation, null)
                .GetComponent<ProjectileMover>();

        projectileMover.GetComponent<ProjectileColor>().SetColor(@class);
        projectileMover.MoveToTarget(targetPos, 0.56f);
        StartCoroutine(SpawnAttackCoroutine(targetPos, @class));

    }

    public IEnumerator SpawnAttackCoroutine(Transform targetPos, AxieClass @class)
    {
        yield return new WaitForSeconds(0.56f);

        MeleeAttack attack = Instantiate(MeleeAttack, new Vector3(targetPos.position.x, targetPos.position.y + 0.25f, targetPos.position.z), Quaternion.identity, null)
                .GetComponent<MeleeAttack>();

        attack.gameObject.GetComponent<ProjectileColor>().SetColor(@class);
    }


    public void SpawnAttack(Transform targetPos, AxieClass @class)
    {
        MeleeAttack attack = Instantiate(MeleeAttack, targetPos.transform.position, Quaternion.identity, null)
                .GetComponent<MeleeAttack>();

        attack.gameObject.GetComponent<ProjectileColor>().SetColor(@class);
    }
}