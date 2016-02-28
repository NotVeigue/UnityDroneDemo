using UnityEngine;
using System.Collections;

public class SkyController : MonoBehaviour {

    // PUBLIC

    public Light dayLight;
    public AnimationCurve dayColorCurve;
    public AnimationCurve dayIntensityCurve;
    public Color midDayColor;
    public Color sunsetColor;

    public Light nightLight;
    public AnimationCurve nightColorCurve;
    public AnimationCurve nightIntensityCurve;
    public Color midnightColor;
    public Color duskColor; 

    [Range(0, 1)]
    public float time = 0;

    // PRIVATE
    private float sunOffset  = 275.0f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        
        float currentAngle = time * 360.0f;
        dayLight.transform.rotation = Quaternion.AngleAxis(sunOffset + currentAngle, Vector3.right);
        nightLight.transform.rotation = Quaternion.Inverse(dayLight.transform.rotation);

        dayLight.intensity = dayIntensityCurve.Evaluate(time);
        nightLight.intensity = nightIntensityCurve.Evaluate(time); 
    }
}
