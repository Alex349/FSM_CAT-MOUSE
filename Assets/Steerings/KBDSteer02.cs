using UnityEngine;
using System.Collections;

namespace Steerings
{

	public class KBDSteer02 : SteeringBehaviour
	{



		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return KBDSteer02.GetSteering (this.ownKS);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS) {

			SteeringOutput steering = new SteeringOutput ();

			Vector3 desiredDirection = Vector3.zero;

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				desiredDirection += Vector3.left;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				desiredDirection += Vector3.right;
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				desiredDirection += Vector3.up;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				desiredDirection += Vector3.down;
			}


			// Beware, this part of the code tampers with speed...
			if (desiredDirection.magnitude < 0.01f) {
				ownKS.linearVelocity = Vector3.zero;  // STOP
			}

			steering.linearAcceleration = desiredDirection * ownKS.maxAcceleration;
			return steering;

		}
	}

}