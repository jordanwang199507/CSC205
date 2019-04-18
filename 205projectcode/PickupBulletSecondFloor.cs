using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBulletSecondFloor : MonoBehaviour {
	float speed = 5f;
	float height = 1f;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void FixedUpdate () {
		Vector3 position = transform.position;
		float newY = Mathf.Sin (Time.time * speed) - 5;
		transform.position = new Vector3 (position.x, newY, position.z) * height ;
	}
}
