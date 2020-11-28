using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BasicTower : MonoBehaviour
{

    private Enemy target = null;

    private int towerDamage = 1;
    //the two variables allowing for a shoot delay.
    private float currentTime = 0;
    protected float fireRate = 0.5f;
    private float maxRange = 5;
    

    List<Transform> allEnemies;

    protected Enemy TargetedEnemy
    {
        get
        {
            return target;
        }
    }

    protected virtual void Update()
    {
        TargetEnemy();
        ShootingDelay();
    }

    protected abstract void RenderAttackVisuals();

    private Enemy TargetEnemy()
    {
        allEnemies = EnemySpawner.instance.allEnemyPositions;
        //Loop through them to find the closest
        float closestDistance = float.MaxValue;
        Transform closest = null;
        foreach (Transform enemy in allEnemies)
        {
            float distanceToEnemy = Vector3.Distance(enemy.transform.position, transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closest = enemy;
            }
        }
        if (closest != null)
        {
            target = closest.GetComponent<Enemy>();
        }
        return target;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(50, 150, 200, 30), "AllEnemies.count " + allEnemies.Count);
    }

    private void ShootAtEnemy()
    {
        float distanceToEnemy = Vector3.Distance(target.transform.position, transform.position);
            if (distanceToEnemy < maxRange)
            {
                target.TakeDamage(towerDamage);
                RenderAttackVisuals();
            }        
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