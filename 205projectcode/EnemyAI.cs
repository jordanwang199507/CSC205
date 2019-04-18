using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

	public PlayerBehaviour player1;
	public PLayer2 player2;
	public GameObject step;

	public enum AIState {Roaming, Frozen}
	public AIState currState = AIState.Roaming;

	public float movementSpeed;
	public enum Direction {Forward, Back, Right, Left};
	public Direction facing = Direction.Forward;

	bool turning = false;
	float smooth = 1f;
	private Quaternion targetRotation;

	Vector3 fwd = new Vector3(0f, 0f, 1.0f);
	Vector3 bkwd = new Vector3(0f, 0f, -1.0f);
	Vector3 lft = new Vector3(-1.0f, 0f, 0f);
	Vector3 rt = new Vector3 (1.0f, 0f, 0f);

	float raylength = 6;

	int count;
	int stepcount;
	int s_counter;

    Animator animator;

	int health = 8;

	public ParticleSystem stunnedParticles;
	ParticleSystem prefab;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        animator.SetBool("Grounded_b", true);
        movementSpeed = 15;
		stepcount = 50;

		targetRotation = transform.rotation;

		transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, 10 * smooth * Time.deltaTime);

	}
		

	// Update is called once per frame
	void FixedUpdate () {
		if (health <= 0) {
			Destroy (gameObject);
			Destroy (prefab);
		}
		//get wallmask
		int wallmask = (1 << 10);

		int player1mask = (1 << 8);
		int player2mask = (1 << 9);


		// Rotate towards facing direction
		transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, 10 * smooth * Time.deltaTime);

		//get vector of facing direction, and what a left, right, or backwards turn would be
		Vector3 dir = new Vector3(0f, 0f, 0f);
		Vector3 l_turn = new Vector3(0f, 0f, 0f);
		Vector3 r_turn = new Vector3(0f, 0f, 0f);
		Vector3 turn_around = new Vector3(0f, 0f, 0f);
		switch (facing) {
		case Direction.Forward:
			dir = fwd;
			l_turn = lft;
			r_turn = rt;
			turn_around = bkwd;
			break;
		case Direction.Right:
			dir = rt;
			l_turn = fwd;
			r_turn = bkwd;
			turn_around = lft;
			break;
		case Direction.Back:
			dir = bkwd;
			l_turn = rt;
			r_turn = lft;
			turn_around = fwd;
			break;
		case Direction.Left:
			dir = lft;
			l_turn = bkwd;
			r_turn = fwd;
			turn_around = rt;
			break;
		}

		//for testing, will delete later
		Vector3 rayOut = transform.position;
		rayOut.y += 4.0f;
		Debug.DrawRay (rayOut, dir * raylength, Color.white);
		Debug.DrawRay (rayOut, l_turn * raylength, Color.white);
		Debug.DrawRay (rayOut, r_turn * raylength, Color.white);

		switch (currState) {

		case AIState.Roaming:
			//check if run into player1 or player2, freeze
			if (Physics.Raycast (transform.position, dir, 1, player1mask)) {
				currState = AIState.Frozen;
				//player1.decreaseHealth ();
				break;
			}
			if (Physics.Raycast (transform.position, dir, 1, player2mask)) {
				currState = AIState.Frozen;
				//player2.decreaseHealth ();
				break;
			}

			//for navigating maze
			if (Physics.Raycast (transform.position, dir, raylength, wallmask)) {
				changeDirections (dir, l_turn, r_turn, turn_around);
			}

			//check if the player's in sight
			if (findPlayer (dir, l_turn, r_turn)) {
				movementSpeed = 30;
				stepcount = 20;
			} else {
				movementSpeed = 15;
				stepcount = 40;
			}

			//play the footstep sound
			s_counter++;
			if (s_counter == stepcount) {
				footStep ();
				s_counter = 0;
			}

			gameObject.transform.position += gameObject.transform.forward * movementSpeed * Time.deltaTime;
            animator.SetFloat("Speed_f",0.1f);
            break;

		case AIState.Frozen:

			//keeping the bad guy frozen for a while
			//return to roaming if no longer frozen
			count++;
			if (count == 150) {
				count = 0;
				currState = AIState.Roaming;
			}

			break;

		}
	}

	void changeDirections(Vector3 dir, Vector3 l_turn, Vector3 r_turn, Vector3 turn_around) {
		//ran into a wall, pick randomly between left and right to test first
		int pick = Random.Range(0, 2);

		switch (pick) {
		case (0):
			//try right first

			if (Physics.Raycast (transform.position, r_turn, raylength) == false) {
				//should turn right
				turn_right ();
			} else if (Physics.Raycast (transform.position, l_turn, raylength) == false) {
				//should turn left
				turn_left ();
			} else {
				//have to turn around
				turn_back();
			}

			break;
		case (1):
			//try left first

			if (Physics.Raycast (transform.position, l_turn, raylength) == false) {
				//should turn left
				turn_left ();
			} else if (Physics.Raycast (transform.position, r_turn, raylength) == false) {
				//should turn right
				turn_right ();
			} else {
				//have to turn around
				turn_back ();
			}
			break;

		}
		
	}

	//checks if the player is to the left, front or right. If found, turns towards them 
	// returns true 
	bool findPlayer(Vector3 dir, Vector3 l_turn, Vector3 r_turn){
		//bit shift the index of the player1 or 2 layer (8,9) to get a bit mask
		int playermask = ((1 << 8) | (1 << 9));

		//bit shift the index of the wall layer (10) to get a bit mask
		int wallmask = 1 << 10;

		//cast the ray from the center(ish) of the body rather than the floor
		Vector3 rayOut = transform.position;
		rayOut.y += 4.0f;

		RaycastHit playerHit;
		RaycastHit wallHit;

		//check if player to the left (and there's no wall in the way)
		if (Physics.Raycast (rayOut, l_turn, out playerHit, Mathf.Infinity, playermask) && Physics.Raycast (rayOut, l_turn, out wallHit, Mathf.Infinity, wallmask)) {
			if (playerHit.distance < wallHit.distance) {
				turn_left ();
				return true;
			}
		}
		//check if player in front (and there's no wall in the way)
		if (Physics.Raycast (rayOut, dir, out playerHit, Mathf.Infinity, playermask) && Physics.Raycast (rayOut, dir, out wallHit, Mathf.Infinity, wallmask)) {
			if (playerHit.distance < wallHit.distance) {
				return true;
			}
		}
		//check if player to the right (and there's no wall in the way)
		if (Physics.Raycast (rayOut, r_turn, out playerHit, Mathf.Infinity, playermask) && Physics.Raycast (rayOut, r_turn, out wallHit, Mathf.Infinity, wallmask)) {
			if (playerHit.distance < wallHit.distance) {
				turn_right ();
				return true;
			}
		}
		return false;
	}

	void turn_right() {
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
		}

	}

	void turn_left() {
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
		}
	}

	void turn_back() {
		targetRotation *= Quaternion.AngleAxis (-180, Vector3.up);

		switch (facing) {
		case Direction.Forward:
			facing = Direction.Back;
			break;
		case Direction.Left:
			facing = Direction.Right;
			break;
		case Direction.Back:
			facing = Direction.Forward;
			break;
		case Direction.Right:
			facing = Direction.Left;
			break;
		}
	}

	void footStep() {
		if (currState == AIState.Roaming) {
			GameObject sound = Instantiate (step, transform.position, transform.rotation);
			Destroy (sound, 1.0f);
		}
	}

	/*void onTriggerEnter(Collider other) {
		switch (other.tag) {
		case "Projectile":
			health--;
			print (health);
			break;
		}
	}*/

	void OnTriggerEnter(Collider other) {
		switch(other.tag) {

		case "Projectile":
			health--;
			Debug.Log (health);
			Destroy (prefab);
			prefab = Instantiate (stunnedParticles, new Vector3 (transform.position.x, transform.position.y + 3, transform.position.z), transform.rotation);
			count = 0;
			currState = AIState.Frozen;
			Destroy (prefab, 3f);

			//GameObject prefab = Instantiate (portal, transform.position, transform.rotation);
			break;
		} // switch

	} // OnTriggerEnter()

}
