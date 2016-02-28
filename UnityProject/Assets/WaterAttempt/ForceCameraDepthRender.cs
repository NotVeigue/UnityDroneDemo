using UnityEngine;
using System.Collections;

public class ForceCameraDepthRender : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
	}
}
