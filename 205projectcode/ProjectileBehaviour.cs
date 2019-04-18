using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position += transform.forward * 50.0f * Time.deltaTime;
		/*Vector3 forwardDir = transform.TransformDirection (Vector3.forward);
		if (Physics.Raycast (transform.position,forwardDir, 1f)) {
			print ("hit");
			Destroy (gameObject, 1f);
		} else {
			
		}*/

	}

	void OnTriggerEnter(Collider other) {
		switch(other.tag) {
		case "Enemy":
			//print ("enemy hit");
			Destroy (gameObject);
			break;
		
		}
	}

}
