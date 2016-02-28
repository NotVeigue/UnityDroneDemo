using UnityEngine;
using System.Collections;

public class DroneCamera : MonoBehaviour {

	public DroneMovementController drone;
	public float followDist = 2.0f;
	public float verticalOffset = 1.0f;
	public float followSpeedCoefficient = 1.0f;

	// Use this for initialization
	void Start () {
		Vector3 newPos = drone.transform.position + drone.transform.forward * -followDist;
		newPos += drone.transform.up * verticalOffset;

		transform.position = newPos;
		transform.LookAt(drone.transform.position + drone.transform.up * verticalOffset);
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        //Vector3 focusPoint = drone.transform.position + drone.transform.up * verticalOffset ;
        //Vector3 target = focusPoint + drone.transform.forward * -followDist;
        Vector3 focusPoint = drone.transform.position + drone.transform.forward * 10.0f;
        Vector3 target = drone.transform.position + drone.transform.up * verticalOffset + drone.transform.forward * -followDist;
        Vector3 toTarget = target - transform.position;
		float speed = drone.maxSpeed * ((target - transform.position).magnitude/followDist) * followSpeedCoefficient;
		transform.position += toTarget.normalized * Mathf.Min(speed * Time.fixedDeltaTime, toTarget.magnitude);
		transform.LookAt(focusPoint);
        if(Vector3.Dot(transform.forward, Vector3.down) > 0.99f)
        {
            Debug.Log("This!!!!!");
            //transform.up = -transform.up;
            transform.Rotate(transform.transform.up, 180.0f);
        }

		//Debug.Log(rotAngle);
		/*
		Vector3 newPos = drone.transform.position + drone.transform.forward * -followDist;
		newPos += drone.transform.up * verticalOffset;

		transform.position = newPos;
		transform.LookAt(drone.transform.position + drone.transform.up * verticalOffset);
		*/
		//Vector3 perp = Vector3.Cross(transform.forward, Vector3.up).normalized;
		//transform.up = Vector3.Cross(perp, transform.forward);
	}
}
