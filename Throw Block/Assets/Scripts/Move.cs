using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Move : MonoBehaviour {

	// Queue holding a recent history of velocities
	// With a queue, we can add velocities to it while removing velocities at the tail of the queue
	// When calculating the weighted velocity, we add small weights to the back of the queue,
	//   and increasingly larger weights to the front of the queue
	Queue<Vector3> recentVelocities = new Queue<Vector3>();

	// Maximum amount of history stored. (If you change, update equation in calculateWeightedLaunchVelocity)
	const int velocityHistoryCount = 10;

	// Remember first position, so we can reset to it
	[HideInInspector]
	public Vector3 initialPosition;

	// Tracking last position so we can calculate velocity
	Vector3 lastPosition = Vector3.zero;

	// Triggers to start and stop
	float throwTrigger = 2f;
	float stopTrigger = 0.02f;

	// 0 means no movement, 1 means no friction - allow edit in inspector
	public float friction = 0.96f;

	// Some variables relating to launching
	bool launching = false;
	Vector3 launchVelocity = Vector3.zero;

	// Keeping track of input and manipulating it
	bool mousePressed = false;

	// Levitation - allow change on an object to object basis
	public float defaultYPos = 1f;
	public float hoverYPos = 3f;
	float yPos;

	void Awake () {
		// We do not want rotation - I used rigid body just to include collisions between objects
		this.GetComponent<Rigidbody> ().freezeRotation = true;

		// Set initial position
		initialPosition = transform.position;

		// Set last position
		lastPosition = transform.position;

		// Set initial height to default height
		yPos = defaultYPos;
	}

	void Update () {
		
		// Find difference in positions to get velocity
		Vector3 velocity = (changeYinVector(transform.position, 0) - changeYinVector(lastPosition, 0)) * Time.deltaTime * 60f;
		// Speed is the magnitude of the velocity
		float speed = velocity.magnitude;

		// Set last position to the position we have now for use in the next iteration of Update() to calculate velocity
		lastPosition = transform.position;

		// If we are not launching,
		if (!launching) {
			// Add new velocity to queue
			recentVelocities.Enqueue(velocity);

			// Remove excess velocities from queue
			int count = recentVelocities.Count;
			if (count > velocityHistoryCount) {
				recentVelocities.Dequeue();
			}
				
			// Move block using mouse - but only in directions parallel to ground
			if (mousePressed) {
				Plane plane = new Plane (Vector3.up, new Vector3 (0, 1, 0));
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

				float distance;
				if (plane.Raycast (ray, out distance)) {
					transform.position = ray.GetPoint (distance);
				}
			}

			// If our speed is faster than the throw trigger, 
			if (speed > throwTrigger) {
				// Launch the object
				launching = true;

				// Stop taking mouse input
				UnpressMouse();

				// Set launch velocity to a weighted average of the past velocities
				launchVelocity = calculateWeightedLaunchVelocity ();
			}
		} 
		// If we are launching,
		else {
			// Move object using launch velocity
			transform.Translate(launchVelocity * Time.deltaTime * 60f);
			// Reduce launch velocity using friction
			launchVelocity = launchVelocity * friction;

			// If our speed is slower than the stop trigger,
			if (launchVelocity.magnitude < stopTrigger || velocity.magnitude < stopTrigger) {
				// Stop launching
				StopLaunching();
			}
		}
			
		// Hover based on mouse input. Lerp makes the object look like it rises.
		if (mousePressed) {
			yPos = Mathf.Lerp (yPos, hoverYPos, 0.2f);
		} else {
			yPos = Mathf.Lerp (yPos, defaultYPos, 0.075f);
		}
		transform.position = changeYinVector (transform.position, yPos);

	}

	void LateUpdate() {
		// Ensure it is within boundary
		withinBoundary ();
	}

	public void ResetBox() {
		StopLaunching ();
		transform.position = initialPosition;
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

		// Calculated specifically for velocityHistoryCount of 10
		// Must satisfy the equation: 20 = 2*initialWeight + 9*weightIncrement
		float initialWeight = 1f; // 0 to 100
		float weightIncrement = 2f; // 0 to 10

		float weight = initialWeight;

		Vector3 aggregateVelocity = Vector3.zero;

		while (recentVelocities.Count > 0) {
			Vector3 oldestVelocity = recentVelocities.Dequeue();
			aggregateVelocity = oldestVelocity * weight / 100f;
			weight = weight + weightIncrement;
		}

		//Debug.Log ("Launch velocity: " + aggregateVelocity);
		return aggregateVelocity;
	}


	void StopLaunching() {
		launching = false;
		// Reset launch vector
		launchVelocity = Vector3.zero;
		// Reset queue
		recentVelocities.Clear();
	}

	// Called when mouse is pressed down
	void PressMouse() {
		mousePressed = true;
		StopLaunching ();
	}

	// Called when mouse is unpressed or when the object is thrown
	void UnpressMouse() { 
		mousePressed = false;
	}

	// Boundaries
	float xMax = 15;
	float zMax = 15;

	// Clamps vector within boundary
	void withinBoundary() {
		
		Vector3 newLocation = transform.position;

		float width = transform.lossyScale.x;
		float depth = transform.lossyScale.z;

		newLocation.x = Mathf.Clamp (transform.position.x, -xMax + width/2, xMax - width/2);
		newLocation.z = Mathf.Clamp (transform.position.z, -zMax + depth/2, zMax - depth/2);

		transform.position = newLocation;
	}

	// Receive inputs from in built methods
	void OnMouseDown() {
		PressMouse ();
	}

	void OnMouseUp() {
		UnpressMouse ();
	}

}
