using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class WeaponEffect : MonoBehaviour 
{
    abstract public void TriggerEffect();
}

public class Bullet : MonoBehaviour, IPoolable {

    private int _poolIndex;
    public int PoolIndex
    {
        get { return _poolIndex;  }
        set { _poolIndex = value; }
    }

    public Action<Bullet> OnDead;
    public Action<Bullet, Collision> OnHit;

    public float lifetime;
    private float elapsedTime;

    public float range;
    private float travelledDist;

    public List<WeaponEffect> onHitEffects;
    public List<WeaponEffect> onDeathEffects;

    void Start()
    {
        //onHitEffects = new List<IWeaponEffect>();
        //onDeathEffects = new List<IWeaponEffect>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
     
    void OnEnable()
    {
        elapsedTime = travelledDist = 0.0f;
    }

    void Update()
    {
        if (elapsedTime >= lifetime)
            return;

        elapsedTime += Time.deltaTime;
        if(elapsedTime >= lifetime && OnDead != null)
        {
            lifetimeOver();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Trigger any effects we have set to trigger on hit
        foreach(WeaponEffect effect in onHitEffects)
        {
            effect.TriggerEffect();
        }

        if (OnHit != null)
            OnHit(this, collision);

        // This should go away once I think of a better way to handle death on hit
        lifetimeOver();
    }

    // This function can be overridden to create more varied results if necessary
    protected void lifetimeOver()
    {
        // Trigger any effects we have set to trigger on death
        foreach (WeaponEffect effect in onDeathEffects)
        {
            effect.TriggerEffect();
        }

        if (OnDead != null)
            OnDead(this);
    }
}
