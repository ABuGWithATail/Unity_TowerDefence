using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicTower : MonoBehaviour
{

    public Enemy target = null;

    private int towerDamage = 1;
    //the two variables allowing for a shoot delay.
    private float currentTime = 0;
    protected float fireRate = 0.5f;
    private float maxRange = 5;

    protected Enemy TargetedEnemy
    {
        get
        {
            return target;
        }
    }

    private void Update()
    {
        TargetEnemy();
    }

    private void TargetEnemy()
    {
        List<Transform> allEnemies = EnemySpawner.instance.allEnemyPositions;
        //Loop through them to find the closest
        float closestDistance = float.MaxValue;
        Transform closest = null;
        foreach (Transform enemy in allEnemies)
        {
            float distanceToEnemy = Vector3.Distance(enemy.transform.position, transform.position);
            if(distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closest = enemy;
            }
        }
       
        target = closest.GetComponent<Enemy>();
        Debug.LogError("TARGET SELECTED");
        if (target != null)
        {
            ShootingDelay();
        }
        if(target == null)
        {
            return;
        }
    }

    private void ShootAtEnemy()
    {
        target.TakeDamage(towerDamage);
        //make the attack visuals happen.
    }

    private void ShootingDelay()
    {
        if (target != null)
        {
            if (currentTime < fireRate)
            {
                currentTime += Time.deltaTime;
            }
            else
            {
                currentTime = 0;
                ShootAtEnemy();
            }
        }
    }
}