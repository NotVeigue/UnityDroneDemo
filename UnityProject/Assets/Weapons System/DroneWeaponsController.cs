using UnityEngine;
using System.Collections;

public class DroneWeaponsController : MonoBehaviour {

    private Weapon weapon;

	// Use this for initialization
	void Start () {
        weapon = gameObject.GetComponent<Weapon>();
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetButton("Fire2"))
        {
            weapon.TryFire();
        }
	}
}
