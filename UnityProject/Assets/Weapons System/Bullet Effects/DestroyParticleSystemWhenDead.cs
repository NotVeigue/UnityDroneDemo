using UnityEngine;
using System.Collections;

public class DestroyParticleSystemWhenDead : MonoBehaviour {

    private ParticleSystem particles;
	
	void Start () {
        particles = GetComponent<ParticleSystem>();
        if(particles == null)
        {
            Destroy(this.gameObject);
        }
	}
	
	void Update () {
        if (!particles.IsAlive())
        {
            Destroy(this.gameObject);
        }
	}
}
