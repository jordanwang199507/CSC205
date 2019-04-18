using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
	public float movementSpeed;
	public float sprintingSpeed;
	public float walkingSpeed;
	public bool sprinting = false;

	//public gameObject omni;

	private float health;
	private float stamina;
	public float healthRegen;
	public float staminaRegen;
	public bool tired = false;
    //public string[] keys =new string[4];

	public enum Direction {Forward, Back, Right, Left};
	public Direction facing = Direction.Forward;

	public enum Floor {TopFloor, BottomFloor};
	public Floor currentFloor = Floor.TopFloor;

	bool turning = false;
	float smooth = 1f;
	private Quaternion targetRotation;
	float raylength = 6;

	Animator animator;

	[SerializeField]
	Camera topDownCamera;
	[SerializeField]
	Camera firstPersonCamera;
	public enum CamState {Top, First};
	public CamState activeCam = CamState.Top;

	private bool onLadder = false;

	private bool colliding = false;

	public GameObject projectile;

	int ammo = 3;
	bool canPickUp = true;
	float pickupWait = 10;
	public AudioSource shotSound;

	public Canvas Winscreen;
	public Canvas Losescreen;

	float gracePeriodTime = 3;
	bool inGracePeriod = false;
	public AudioSource hurtSound;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		animator.SetBool ("Grounded_b", true);

		walkingSpeed = 15;
		sprintingSpeed = walkingSpeed * 2;
		movementSpeed = walkingSpeed;

		health = 5;
		stamina = 10;

		topDownCamera = GameObject.Find ("Main Camera Player1").GetComponent<Camera> ();
		targetRotation = transform.rotation;

		topDownCamera.GetComponent<Camera> ().enabled = true;
		firstPersonCamera.GetComponent<Camera> ().enabled = false;

		ammo = 10;
		Winscreen.gameObject.SetActive(false);
		Losescreen.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void FixedUpdate () {
		//print (colliding);
		//print (health);


		checkWin ();
		getKeyInput ();
		// Rotate towards facing direction
		transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, 10 * smooth * Time.deltaTime);

		checkCollision ();

		if (Input.GetKeyDown (KeyCode.F) && ammo > 0) {
			GameObject prefab = Instantiate (projectile, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), transform.rotation);
			Instantiate (shotSound);
			ammo -= 1;
			Destroy (prefab, 3);
		}

		if (!canPickUp) {
			pickupWait -= Time.deltaTime;
		}
		if (pickupWait <= 0) {
			canPickUp = true;
			pickupWait = 10;
		}

		if (inGracePeriod) {
			gracePeriodTime -= Time.deltaTime;
		}
		if (gracePeriodTime <= 0) {
			inGracePeriod = false;
			gracePeriodTime = 3;
		}

		if (Input.GetKey (KeyCode.LeftShift) && !tired) {
			sprinting = true;
			movementSpeed = sprintingSpeed;

			// if player is actually running, lower stamina
			if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.D)) {
				stamina -= 1 * Time.deltaTime;
			} // if

			// if stamina goes under 0, cap it
			if (stamina < 0) {
				stamina = 0;
			} // if

		} else if (tired) {
			sprinting = false;

			if (stamina > 3) {
				tired = false;
			} // if

			movementSpeed = walkingSpeed / 2;
			stamina += 0.2f * Time.deltaTime;

		} else {
			sprinting = false;
			movementSpeed = walkingSpeed;

			// stamina regen
			if (stamina < 10) {
				stamina += 0.2f * Time.deltaTime;
			} // if

			// if stamina goes over 10, cap it
			if (stamina > 10) {
				stamina = 10;
			} // if
		} // else

		if (stamina < 1) {
			tired = true;
			sprinting = false;
		}
			
		if (currentFloor.Equals (Floor.BottomFloor)) {
			topDownCamera.GetComponent<Camera> ().enabled = false;
			firstPersonCamera.GetComponent<Camera> ().enabled = true;
			activeCam = CamState.First;
		}

		if (Input.GetKeyDown (KeyCode.Space) && !currentFloor.Equals(Floor.BottomFloor)) {//Camera switching 
			switch (activeCam) {

			case CamState.Top:
				topDownCamera.GetComponent<Camera> ().enabled = false;
				firstPersonCamera.GetComponent<Camera> ().enabled = true;
				activeCam = CamState.First;
				//omni.GetComponent<SplitScreen>().ChangeCam1(firstPersonCamera);
				break;

			case CamState.First:
				topDownCamera.GetComponent<Camera> ().enabled = true;
				firstPersonCamera.GetComponent<Camera> ().enabled = false;
				activeCam = CamState.Top;
				break;

			} // switch
		} // if

		if (Input.GetKeyDown (KeyCode.E) && onLadder) {
			if (currentFloor.Equals (Floor.TopFloor)) {
				
				transform.Translate (0, -10, 0);
				currentFloor = Floor.BottomFloor;

			} else {
				
				transform.Translate (0, 10, 0);
				currentFloor = Floor.TopFloor;

			}
		}
	}

	void OnTriggerEnter(Collider other) {
		
		switch (other.tag) {
		case "Ladder":
			//Debug.Log ("Enter Ladder");
			onLadder = true;
			break;

		case "Bullet Spawner":
			if (canPickUp) {
				ammo++;
				health++;
				stamina = 5;
				tired = false;
				Debug.Log ("Health: " + health);
				Debug.Log ("Ammmo: " + ammo);
				canPickUp = false;
			}

			break;

		case "Enemy":
			if (!inGracePeriod) {
				inGracePeriod = true;
				decreaseHealth ();
				print (health);
				Instantiate (hurtSound);
			}
			break;
		} // switch


	} // OnTriggerEnter()

	void OnTriggerExit(Collider other) {
		
		switch (other.tag) {
		case "Ladder":
			//Debug.Log("Exit Ladder");
			onLadder = false;
			break;
		} // switch

	} // OnTriggerExit()

	void checkCollision() {
		Vector3 forwardDir = transform.TransformDirection (Vector3.forward);
		int bulletmask = ~((1 << 12) | (1 << 13)); //also includes ladders
		if (Physics.Raycast (transform.position,forwardDir, 4, bulletmask)) {
			colliding = true;
		} else {
			colliding = false;
		}
	}

	bool checkCollisionBackwards() {
		Vector3 backDir = -transform.TransformDirection (Vector3.forward);
		int bulletmask = ~((1 << 12) | (1 << 13));
		if (Physics.Raycast (transform.position,backDir, 4, bulletmask)) {
			return true;
		} else {
			return false;
		}
	}


	void getKeyInput() {

		// Movement when in Top-Down camera mode
		if (activeCam.Equals (CamState.Top)) {
			
			// Move Forward
			if (Input.GetKeyDown (KeyCode.W)) {

				switch (facing) {
				case Direction.Back:
					targetRotation *= Quaternion.AngleAxis (-180, Vector3.up);
					break;
				case Direction.Right:
					targetRotation *= Quaternion.AngleAxis (-90, Vector3.up);
					break;
				case Direction.Left:
					targetRotation *= Quaternion.AngleAxis (90, Vector3.up);
					break;
				} // switch

				facing = Direction.Forward;
			} // if

			// Move Backward
			 if (Input.GetKeyDown (KeyCode.S)) {

				switch (facing) {
				case Direction.Forward:
					targetRotation *= Quaternion.AngleAxis (-180, Vector3.up);
					break;
				case Direction.Right:
					targetRotation *= Quaternion.AngleAxis (90, Vector3.up);
					break;
				case Direction.Left:
					targetRotation *= Quaternion.AngleAxis (-90, Vector3.up);
					break;
				} // switch

				facing = Direction.Back;
			} // if

			// Move Right
			 if (Input.GetKeyDown (KeyCode.D)) {

				switch (facing) {
				case Direction.Forward:
					targetRotation *= Quaternion.AngleAxis (90, Vector3.up);
					break;
				case Direction.Back:
					targetRotation *= Quaternion.AngleAxis (-90, Vector3.up);
					break;
				case Direction.Left:
					targetRotation *= Quaternion.AngleAxis (-180, Vector3.up);
					break;
				} // switch

				facing = Direction.Right;
			} // if

			// Move Left
			 if (Input.GetKeyDown (KeyCode.A)) {

				switch (facing) {
				case Direction.Forward:
					targetRotation *= Quaternion.AngleAxis (-90, Vector3.up);
					break;
				case Direction.Back:
					targetRotation *= Quaternion.AngleAxis (90, Vector3.up);
					break;
				case Direction.Right:
					targetRotation *= Quaternion.AngleAxis (-180, Vector3.up);
					break;
				} // switch
				//Debug.Log("found A");
				//animator.SetFloat("Speed_f",1.0f);
				facing = Direction.Left;
			} // if
			//ANIMATION!!

			//if (Input.GetKeyDown (KeyCode.A)) {
				//animator.SetFloat("Speed_f", 0.0f);
			//}


			if ((Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.D)) && !colliding) {	
				if (sprinting) {
					animator.SetFloat ("Speed_f", 0.8f);
				} else if (tired) {
					animator.SetFloat ("Speed_f", 0.11f);	
				} else {
					animator.SetFloat ("Speed_f", 0.4f);
				}
				gameObject.transform.position += gameObject.transform.forward * movementSpeed * Time.deltaTime;
			} // else if
			else {
				animator.SetFloat("Speed_f", 0.0f);
			}

		} // if 

		// Movement when in First-Person camera mode
		else if (activeCam.Equals (CamState.First)) {

			// Move Right
			if (Input.GetKeyDown (KeyCode.D)) {
				//Debug.Log ("move");
				targetRotation *= Quaternion.AngleAxis (90, Vector3.up);

				switch (facing) {
				case Direction.Forward:
					facing = Direction.Right;
					break;
				case Direction.Right:
					facing = Direction.Back;
					break;
				case Direction.Back:
					facing = Direction.Left;
					break;
				case Direction.Left:
					facing = Direction.Forward;
					break;
				} // switch
					
			} // if

			// Move Left
			if (Input.GetKeyDown (KeyCode.A)) {
				targetRotation *= Quaternion.AngleAxis (-90, Vector3.up);

				switch (facing) {
				case Direction.Forward:
					facing = Direction.Left;
					break;
				case Direction.Left:
					facing = Direction.Back;
					break;
				case Direction.Back:
					facing = Direction.Right;
					break;
				case Direction.Right:
					facing = Direction.Forward;
					break;
				} // switch
			} // if

			if (Input.GetKey (KeyCode.W) && !colliding) {			
				gameObject.transform.position += gameObject.transform.forward * movementSpeed * Time.deltaTime;
			} // else if

			if (Input.GetKey (KeyCode.S) && !checkCollisionBackwards()) {			
				gameObject.transform.position += gameObject.transform.forward * -movementSpeed * Time.deltaTime;
			} // else if

		} // else if

	} // getKeyInput

	public void decreaseHealth() {
		health--;
		if (health == 0) {
			Losescreen.gameObject.SetActive (true);
			Time.timeScale = 0;
		}
	}

	void checkWin() {
		//treasure layer mask
		int treasuremask = 1 << 11;
		Vector3 forwardDir = transform.TransformDirection (Vector3.forward);
		if (Physics.Raycast (transform.position, forwardDir, raylength, treasuremask)) {
			//enter win condition, game over
			Winscreen.gameObject.SetActive(true);
			Time.timeScale = 0;
		}

	}
		
}
