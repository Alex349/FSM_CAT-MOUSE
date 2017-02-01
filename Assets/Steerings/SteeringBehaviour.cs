using UnityEngine;
using System.Collections;

namespace Steerings
{

	public class SteeringBehaviour : MonoBehaviour 
	{
		public bool lookWhereYouGoInstantaneous = false;
		protected KinematicState ownKS;

		// BEWARE: made vitual in order to allow redefinitions
		protected virtual void Start ()
		{
			// get a reference to the kinematic state and hold it
			ownKS = GetComponent<KinematicState>();
		}
	
		// Update is called once per frame
		void Update ()
		{
			SteeringOutput steering = GetSteering ();

			if (steering == null) {
				// no steering. Stop the object
				ownKS.linearVelocity = Vector3.zero; // stop!!!
				ownKS.angularSpeed = 0f; // stop!!!
			} else {
				// apply linear steering: change linear velocity and position 

				float dt = Time.deltaTime;

				// apply linear steering: change linear velocity and position 
				ownKS.linearVelocity = ownKS.linearVelocity + steering.linearAcceleration*dt; // v=v+a·t
				if (ownKS.linearVelocity.magnitude > ownKS.maxSpeed)
					ownKS.linearVelocity = ownKS.linearVelocity.normalized * ownKS.maxSpeed; // clipping of velocity
				ownKS.position = ownKS.position + ownKS.linearVelocity*dt + 0.5f*steering.linearAcceleration*dt*dt; // s = v·t + 1/2(a·t^2)

				// apply angular steering: change angular velocity and orientation
				ownKS.angularSpeed = ownKS.angularSpeed + steering.angularAcceleration*dt;
				if (Mathf.Abs(ownKS.angularSpeed) > ownKS.maxAngularSpeed)
					ownKS.angularSpeed = ownKS.maxAngularSpeed * Mathf.Sign(ownKS.angularSpeed); // clip if necessary
				ownKS.orientation = ownKS.orientation + ownKS.angularSpeed*dt+0.5f*steering.angularAcceleration*dt*dt;

				// change position and orientation
				transform.position = ownKS.position;
				if (lookWhereYouGoInstantaneous) {
					if (ownKS.linearVelocity.magnitude > 0.001f) {
						transform.rotation = Quaternion.Euler (0, 0,  Utils.VectorToOrientation (ownKS.linearVelocity));
						ownKS.orientation = transform.rotation.eulerAngles.z;
					}
				} else {
					transform.rotation = Quaternion.Euler(0, 0, ownKS.orientation);
				}
			}
		}

		public virtual SteeringOutput GetSteering () {
			Debug.Log ("Invoking a non-redefined virtual method");
			return null;
		} 
	}
}
