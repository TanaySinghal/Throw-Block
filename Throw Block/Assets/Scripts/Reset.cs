using UnityEngine;
using System.Collections;

public class Reset : MonoBehaviour {

	// GUI
	void OnGUI() {
		if (GUI.Button (new Rect (10, 10, 70, 30), "Reset")){
			// Reset to initial position

			GameObject[] shapes = GameObject.FindGameObjectsWithTag ("Object");
			foreach (GameObject shape in shapes) {
				shape.GetComponent<Move> ().ResetBox();
				//transform.position = initialPosition;
			}
		}
	}

}
