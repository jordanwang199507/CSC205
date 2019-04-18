using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitScreen : MonoBehaviour {
	public Camera cam1;
	public Camera cam2;
	public Camera cam1Top;
	public Camera cam2Top;
	//public bool twoplayer; // this should start as false but is true for testing purposes
	public bool horizontal = false;
	//public GameObject Player2;
	// Update is called once per frame
	void Start() {
		Debug.Log ("Number players: " + PlayerPrefs.GetInt ("Players"));
		if(PlayerPrefs.GetInt("Players") == 2) { 
			ChangeSplitScreen ();
			//Player2.SetActive (true);
		}
	}

	public void ChangeSplitScreen(){
		horizontal = !horizontal;
			
//		if (horizontal) {
//			cam1.rect = new Rect (0, 0, 1, 0.5f);
//			cam2.rect = new Rect (0, 0.5f, 1, 0.5f);
//		} else {
		cam1.rect = new Rect (0, 0, 0.5f, 1);
		cam2.rect = new Rect (0.5f, 0, 0.5f, 1);
		cam1Top.rect = new Rect (0, 0, 0.5f, 1);
		cam2Top.rect = new Rect (0.5f, 0, 0.5f, 1);
		//}
	}
	public void ChangeCam1(Camera cam){
		cam1 = cam;
		ChangeSplitScreen ();
	}
	public void ChangeCam2(Camera cam){
		cam2 = cam;
		ChangeSplitScreen ();
	}
}
