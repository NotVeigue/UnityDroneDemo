using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{

    public Bullet ammoPrefab;
    public List<Transform> shotPositions;
    public float firingRate = 1.0f;
    public float bulletSpeed = 5.0f;
    public float range = 10.0f;
    public float maxBulletLife = 5.0f;

    private int bulletsPerShot = 2;
    private ObjectPool<Bullet> bulletPool;
    private float lastShotTime;

	// Use this for initialization
	void Start ()
    {
        Init();   
    }

    public void Init()
    {
        lastShotTime = -firingRate;

        float estimatedMax = (maxBulletLife / firingRate) * bulletsPerShot;
        estimatedMax += estimatedMax * 0.2f; // Add an extra 10% just to be safe
        bulletPool = new ObjectPool<Bullet>((int)estimatedMax, ammoPrefab);
    }

    public bool TryFire()
    {
        if (Time.realtimeSinceStartup - lastShotTime < firingRate || bulletPool.ObjectsAvailable < shotPositions.Count)
        {
            return false;
        }

        lastShotTime = Time.realtimeSinceStartup;
        Fire();

        return true;
    }

    // Default fire method. Can be overridden.
    protected void Fire()
    {
        for (int i = 0; i < shotPositions.Count; ++i)
        {
            Bullet bullet = bulletPool.Allocate();
            if (bullet == null)
            {
                Debug.Log("Null Bullet");
                return;
            }

            BulletStraightMovement movement = bullet.gameObject.GetComponent<BulletStraightMovement>();
            if(movement == null)
            {
                movement = bullet.gameObject.AddComponent<BulletStraightMovement>();
            }

            movement.velocity = transform.forward * bulletSpeed;
            bullet.lifetime = maxBulletLife;
            bullet.transform.position = shotPositions[i].position;
            bullet.OnDead += DeallocateBullet;
            bullet.gameObject.SetActive(true);
        }
    }

    protected void DeallocateBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        bullet.OnDead -= DeallocateBullet; 
        bulletPool.Deallocate(bullet);
    }

    void OnDrawGizmos()
    {
        foreach(Transform t in shotPositions)
        {
            Gizmos.DrawWireSphere(t.transform.position, 0.2f);
        }
    }


}
