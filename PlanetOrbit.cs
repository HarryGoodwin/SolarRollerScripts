using UnityEngine;
using System.Collections;

public class PlanetOrbit : MonoBehaviour {

	public GameObject target;
	public float orbitDegreesPerSec = 180.0f;
	public Vector3 relativeDistance = Vector3.zero;

	private bool shouldOrbit = false;
	public float orbitDistance = 1.0f;
	
	public GameObject lightSource;
	
	void Start () 
	{
	}
	
	void Orbit()
	{
		if(shouldOrbit && target != null)
		{
			if (lightSource) Destroy(lightSource);
			transform.position = target.transform.position + relativeDistance;
			transform.RotateAround(target.transform.position, Vector3.down, orbitDegreesPerSec * Time.deltaTime);
			relativeDistance = transform.position - target.transform.position;
		}
	}

	void OnTriggerEnter(Collider other) 
	{
		if (other.gameObject.tag == "Sun") 
		{
			orbitDistance = 1.0f * 0.5f; // should use an array of other planets to calc distance
			relativeDistance = new Vector3(orbitDistance, target.transform.position.y, orbitDistance);
			transform.position = new Vector3(target.transform.position.x + orbitDistance, target.transform.position.y, target.transform.position.z + 3);
			shouldOrbit = true;
		}
	}
	
	void LateUpdate () 
	{
		Orbit();
	}
}
