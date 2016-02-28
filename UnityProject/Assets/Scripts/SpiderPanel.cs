using UnityEngine;
using System.Collections;

public class SpiderPanel : MonoBehaviour {

	Vector3 scaledBounds;

	public Vector3 ScaledBounds {
		get { return scaledBounds; }
	}

	// Use this for initialization
	void Start () {
		scaledBounds = Vector3.Scale(GetComponent<MeshFilter>().mesh.bounds.extents, transform.localScale);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
