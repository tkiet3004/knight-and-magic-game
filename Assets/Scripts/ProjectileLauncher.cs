using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public Transform launchPoint;
    public GameObject projectilePrefab;

    public void FireProjectile()
    {
        GameObject projectileObj = Instantiate(projectilePrefab, launchPoint.position, projectilePrefab.transform.rotation);
        Vector3 origScale = projectileObj.transform.localScale;
        
        projectileObj.transform.localScale = new Vector3(
            origScale.x * transform.localScale.x > 0 ? 1 : -1,
            origScale.y,
            origScale.z
            );

        // Apply Damage Upgrade if Player
        if (GetComponentInParent<PlayerController>() != null)
        {
            Projectile p = projectileObj.GetComponent<Projectile>();
            if (p != null)
            {
                int damageLevel = PlayerPrefs.GetInt("DamageLevel", 0);
                if (damageLevel > 0)
                {
                    p.damage += damageLevel * 5;
                }
            }
        }
    }
}
