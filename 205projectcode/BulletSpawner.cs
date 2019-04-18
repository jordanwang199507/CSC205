using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour {
	public GameObject pickupBullet;
	GameObject bulletPrefab;
	bool hasBullet;
	float respawnTime = 0;
	float countdownTimer;

	// Use this for initialization
	void Start () {
		countdownTimer = respawnTime;
		//bulletPrefab = Instantiate (pickupBullet, transform.position, transform.rotation);
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (!hasBullet) {
			respawnTime -= Time.deltaTime;
		}

		if (respawnTime <= 0) {
			bulletPrefab = Instantiate (pickupBullet, transform.position, transform.rotation);
			hasBullet = true;
			respawnTime = 10;
			print ("respawn");
		}
	}

	void OnTriggerEnter(Collider other) {
		switch (other.tag) {
		case "Player":
			print ("Player");
			if (hasBullet) {
				Destroy (bulletPrefab);
				print ("Destroyed");
				hasBullet = false;
			}
			break;
		}
	}
}
