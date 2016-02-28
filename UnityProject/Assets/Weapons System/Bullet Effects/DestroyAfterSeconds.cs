using UnityEngine;
using System.Collections;

public class DestroyAfterSeconds : MonoBehaviour {

    public float timeToWait = 5.0f;

	void Start () {
        waitThenDie();
	}

    IEnumerator waitThenDie()
    {
        yield return new WaitForSeconds(timeToWait);
        Destroy(this.gameObject);
    }
}
