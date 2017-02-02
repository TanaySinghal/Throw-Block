using UnityEngine;
using System.Collections;

public class Reset : MonoBehaviour {

	bool muted = false;
	string muteString = "Mute";
	// GUI
	void OnGUI() {
		if (GUI.Button (new Rect (10, 10, 70, 30), "Reset")){
			// Reset to initial position

			GameObject[] shapes = GameObject.FindGameObjectsWithTag ("Object");
			foreach (GameObject shape in shapes) {
				shape.GetComponent<Move> ().ResetBox();
			}
		}

		if (GUI.Button (new Rect (10, 50, 70, 30), muteString)){
			if (!muted) {
				AudioListener.volume = 0f;
				muteString = "Unmute";
				muted = true;
			} else {
				AudioListener.volume = 1f;
				muteString = "Mute";
				muted = false;
			}
		}
	}

}
