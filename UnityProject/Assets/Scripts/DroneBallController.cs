using UnityEngine;
using System.Collections;

public class DroneBallController : MonoBehaviour {

	enum DroneBallState {
		Normal = 0,
		Spider = 1
	}

	public Camera camera;
	public float cameraFollowDist = 10.0f;
	public float movementForce = 2000.0f;
	public float maxSpeed = 5.0f;
	public float maxSpiderGrabDist = 1.0f;
	public float spiderStickForce = 2000.0f;
    public float maxSlope = 0.6f;

	DroneBallState state = DroneBallState.Normal;
	Rigidbody rigidbody;
	SpiderPanel[] spiderPanels; 

	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody>();
		spiderPanels = FindObjectsOfType<SpiderPanel>();
	}

	Vector3 ClampV3(Vector3 v, float min, float max)
	{
		return new Vector3(
			Mathf.Clamp(v.x, min, max),
			Mathf.Clamp(v.y, min, max),
			Mathf.Clamp(v.z, min, max)
		);
	}

	Vector3 ProjectToPlane(Vector3 pos, Vector3 planePos, Vector3 normal)
	{
		Vector3 planeToPos = pos - planePos;
		float distFromPlane = Vector3.Dot(planeToPos, normal);

		return pos - normal * distFromPlane;
	}

	float DistAlongAxis(Vector3 pos, Vector3 planePos, Vector3 axis)
	{
		Vector3 planeToPos = pos - planePos;
		return Vector3.Dot(axis, planeToPos);
	}

	Vector3 ClosestPointOnPlane(Vector3 pos, SpiderPanel plane)
	{
		Vector3 planePos = plane.transform.position;
		Vector3 bounds = plane.ScaledBounds;
		float zdist = Mathf.Clamp(DistAlongAxis(pos, planePos, plane.transform.forward), -bounds.z, bounds.z);
		float xdist = Mathf.Clamp(DistAlongAxis(pos, planePos, plane.transform.right), -bounds.x, bounds.x);

		return planePos + plane.transform.forward * zdist + plane.transform.right * xdist;
	}

	SpiderPanel FindClosestPanel(out Vector3 dirToPanel)
	{
		SpiderPanel closestPanel = null;
		float shortestDist = float.MaxValue;
		dirToPanel = Vector3.zero;

		foreach(SpiderPanel p in spiderPanels)
		{
			Vector3 pointOnP = ProjectToPlane(transform.position, p.transform.position, p.transform.up);
			Vector3 closestPointOnP = ClosestPointOnPlane(pointOnP, p);
			Vector3 toClosestPointOnP = closestPointOnP - transform.position;
			float dist = toClosestPointOnP.magnitude;

			if(dist < maxSpiderGrabDist && (closestPanel == null || dist < shortestDist))
			{
				closestPanel = p;
				shortestDist = dist;
				dirToPanel = toClosestPointOnP.normalized;
			}
		}

		return closestPanel;
	}

	// Update is called once per frame
	void FixedUpdate () {

		float hdir = Input.GetAxis("Horizontal");
		float vdir = Input.GetAxis("Vertical");
		Vector3 movementDir = Vector3.zero;

		Vector3 dirToPanel = Vector3.zero;
		SpiderPanel closest = null;
		if(Input.GetAxis("RightTrigger") > 0.0)
		{
			closest = FindClosestPanel(out dirToPanel);
		}

		state = closest != null ? DroneBallState.Spider : DroneBallState.Normal;
		rigidbody.useGravity = state == DroneBallState.Normal;
		rigidbody.drag = state == DroneBallState.Normal ? 0.1f : 2.0f;

        bool onground = true;
		if(state == DroneBallState.Normal)
		{
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.8f))
            {
                /*
                Vector3 right = Vector3.Cross(hit.normal, Vector3.forward).normalized;
                Vector3 forward = Vector3.Cross(right, hit.normal).normalized;
                movementDir = (right * Vector3.Dot(right, Vector3.right) * hdir + forward * Vector3.Dot(forward, Vector3.forward) * vdir).normalized;
                */
                movementDir = new Vector3(hdir, 0.0f, vdir);
            }
            else
            {
                onground = false;
            }
		}
		else if(state == DroneBallState.Spider)
		{
			float dot = Mathf.Abs(Vector3.Dot(closest.transform.up, dirToPanel));
			rigidbody.AddForce(dirToPanel * spiderStickForce * Time.fixedDeltaTime);
			if(dot > 0.8)
			{
				movementDir = (closest.transform.right * hdir + closest.transform.forward * vdir).normalized;
			}
		}

		rigidbody.AddForce(movementDir * movementForce * Time.fixedDeltaTime);

        if(onground)
        {
            rigidbody.velocity = ClampV3(rigidbody.velocity, -maxSpeed, maxSpeed);
        }

        camera.transform.position = transform.position + Vector3.back * cameraFollowDist;
		camera.transform.LookAt(transform.position);
	}
}
