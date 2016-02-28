using UnityEngine;
using System.Collections;

public class InstantiateObjectEffect : WeaponEffect {

    public GameObject objectToInstantiate;

    public override void TriggerEffect()
    {
        if (objectToInstantiate == null)
            return;

        // Spawn an object at the location of the object this effect is attached to.
        GameObject.Instantiate(objectToInstantiate, transform.position, Quaternion.identity);
    }
}
