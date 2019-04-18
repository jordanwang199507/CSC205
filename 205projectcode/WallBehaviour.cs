using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBehaviour : MonoBehaviour {
	public GameObject portal;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/*void OnTriggerEnter(Collider other) {
		switch(other.tag) {

		case "Projectile":
			Debug.Log ("Projectile");
			GameObject prefab = Instantiate (portal, transform.position, transform.rotation);
			break;
		} // switch

	} // OnTriggerEnter()*/
}
