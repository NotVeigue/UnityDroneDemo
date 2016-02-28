using UnityEngine;
using System.Collections;

public class BulletStraightMovement : MonoBehaviour {

    public Vector3 velocity;

	void FixedUpdate () {
        transform.position += velocity * Time.fixedDeltaTime;
        transform.forward = velocity.normalized;
	}
}
