using UnityEngine;
using System.Collections;

public class DroneMovementController : MonoBehaviour {

	public float maxSpeed = 10.0f;
	public AnimationCurve speedCurve;
	//public float acceleration = 5.0f;
	public float drag = 0.9f;
	public float turnForce = 50.0f;

	float speed = 0.0f;
	float yrot = 0.0f;
	float xrot = 0.0f;

	public float Speed
	{
		get { return speed; }
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float reverse = Input.GetButton("LeftBumper") ? -1.0f : 1.0f;

		float thrust = Input.GetAxis("RightTrigger");
		float targetSpeed = speedCurve.Evaluate(thrust) * maxSpeed;
		speed = ((speed = Mathf.Max(targetSpeed, speed * drag)) < 0.01f ? 0.0f : speed) * reverse;

		float tfy = Input.GetAxis("Horizontal") * turnForce;
		float tfx = Input.GetAxis("Vertical") * turnForce;

		yrot += tfy * Time.fixedDeltaTime;
		xrot = Mathf.Clamp(xrot + tfx * Time.fixedDeltaTime, -89.0f, 89.0f);

		transform.rotation = Quaternion.AngleAxis(yrot, Vector3.up) * Quaternion.AngleAxis(xrot, Vector3.right);
		transform.position += transform.forward * speed * Time.fixedDeltaTime;
	}
}
