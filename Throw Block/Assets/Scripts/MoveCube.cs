using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MoveCube : MonoBehaviour {

	// Use this for initialization

	//List<Vector3> recentVelocities = new List<Vector3>();
	Queue<Vector3> recentVelocities = new Queue<Vector3>();
	Vector3 lastPosition = Vector3.zero;
	float throwTrigger = 2f;
	float stopTrigger = 0.02f;
	float friction = 0.96f; // 0 means no movement, 1 means no friction
	bool launching = false;
	Vector3 launchVelocity = Vector3.zero;
	bool mousePressed = false;

	float yPos = 1f;

	// If you change, update equation in calculateWeightedLaunchVelocity
	const int velocityHistoryCount = 10;

	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector3 velocity = (changeYinVector(transform.position, 0) - changeYinVector(lastPosition, 0)) * Time.deltaTime * 60f;
		float speed = velocity.magnitude;

		//Debug.Log(Time.deltaTime);

		// Set last position
		lastPosition = transform.position;


		if (!launching) {

			// Add new velocity
			//recentVelocities.Add (velocity);
			//recentVelocities.Add(velocity;
			recentVelocities.Enqueue(velocity);

			// Remove excess velocities
			int count = recentVelocities.Count;
			if (count > velocityHistoryCount) {
				//recentVelocities.RemoveAt (count - 1);
				recentVelocities.Dequeue();
			}
				
			// Move block using mouse
			if (mousePressed) {
				Plane plane = new Plane (Vector3.up, new Vector3 (0, 1, 0));
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

				float distance;
				if (plane.Raycast (ray, out distance)) {
					transform.position = ray.GetPoint (distance);

				}
			}

			// If trigger launch
			if (speed > throwTrigger) {
				// Then go.. but also stop taking mouse input
				Debug.Log("Trigger launched: " + speed);
				launching = true;
				// Stop taking mouse input
				UnpressMouse();
				launchVelocity = velocity;
				//printRecentVelocities ();
				//launchVelocity = calculateLaunchVelocity ();
				launchVelocity = calculateWeightedLaunchVelocity ();
			}
		} 
		// If we are launching
		else {
			transform.Translate(launchVelocity * Time.deltaTime * 60f);

			//Debug.Log ("Launching: " + speed);
			launchVelocity = launchVelocity * friction;


			// If done launching
			if (launchVelocity.magnitude < stopTrigger || velocity.magnitude < stopTrigger) {
				// Stop launching
				Debug.Log("STOPPING: " + speed);
				launching = false;
				launchVelocity = Vector3.zero;
				// Empty list
				recentVelocities.Clear();
			}
		}
			
		// Ensure it is within boundary
		withinBoundary ();

		// Hover
		if (mousePressed) {
			//yPos = 3f;
			yPos = Mathf.Lerp (yPos, 3f, 0.1f);
		} else {
			//yPos = 1f;
			yPos = Mathf.Lerp (yPos, 1f, 0.075f);
		}
		transform.position = changeYinVector (transform.position, yPos);

	}

	Vector3 changeYinVector(Vector3 vector, float yValue) {
		return new Vector3 (vector.x, yValue, vector.z);
	}

	void printRecentVelocities() {
		foreach (Vector3 recentVelocity in recentVelocities) {
			Debug.Log (recentVelocity);
		}
	}
		
	// Calculates weighted launch velocity
	Vector3 calculateWeightedLaunchVelocity() {

		// Must satisfy the equation: 20 = 2*initialWeight + 9*weightIncrement
		float initialWeight = 1f; // 0 to 100
		float weightIncrement = 2f; // 0 to 10

		float weight = initialWeight;

		Vector3 aggregateVelocity = Vector3.zero;

		while (recentVelocities.Count > 0) {
			Vector3 oldestVelocity = recentVelocities.Dequeue();
			//oldestVelocity = new Vector3(oldestVelocity.x, 3, oldestVelocity.z);
			aggregateVelocity = oldestVelocity * weight / 100f;
			weight = weight + weightIncrement;
		}

		Debug.Log ("Launch velocity: " + aggregateVelocity);
		return aggregateVelocity;
	}

	void OnMouseDown() {
		PressMouse ();
	}

	void OnMouseUp() {
		UnpressMouse ();
	}

	void PressMouse() {
		mousePressed = true;
		// Levitate
		//transform.position = new Vector3(transform.position.x, 3, transform.position.z);
	}

	void UnpressMouse() { 
		mousePressed = false;
		// Unlevitate
		//transform.position = new Vector3(transform.position.x, 1, transform.position.z);
	}

	// Boundaries
	float xMax = 15;
	float zMax = 15;

	// Clamps vector within boundary
	void withinBoundary() {
		
		Vector3 newLocation = transform.position;

		if (transform.position.x + transform.lossyScale.x > xMax) {
			newLocation.x = xMax - transform.lossyScale.x;
		}

		if (transform.position.x - transform.lossyScale.x < -xMax) {
			newLocation.x = -xMax + transform.lossyScale.x;
		}

		if (transform.position.z + transform.lossyScale.x > zMax) {
			newLocation.z = zMax - transform.lossyScale.z;
		}

		if (transform.position.z - transform.lossyScale.x < -zMax) {
			newLocation.z = -zMax + transform.lossyScale.y;
		}

		transform.position = newLocation;
	}
}
