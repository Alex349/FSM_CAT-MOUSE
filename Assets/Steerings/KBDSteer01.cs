using UnityEngine;
using System.Collections;

namespace Steerings
{

	public class KBDSteer01 : SteeringBehaviour
	{



		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return KBDSteer01.GetSteering (this.ownKS);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS) {

			SteeringOutput result = new SteeringOutput();
			Vector3 desiredDirection = Vector3.zero;
			float desiredAngularDirection = 0;

			// UP is MOVE FORWARD.
			// DOWN is MOVE BACKWARDS.
			if (Input.GetKey(KeyCode.UpArrow))
			{
				desiredDirection += Utils.OrientationToVector (ownKS.orientation);
			}
			if  (Input.GetKey(KeyCode.DownArrow))
			{
				desiredDirection += -Utils.OrientationToVector (ownKS.orientation);
			}




			if (Input.GetKey(KeyCode.LeftArrow))
			{
				desiredAngularDirection += 1f;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				desiredAngularDirection += -1f;
			}


			// Beware: this part of the code tampers with the speed...
			if (desiredAngularDirection == 0) {
				ownKS.angularSpeed = 0;  // stop!!!
				if (desiredDirection.magnitude < 0.01) 
					ownKS.linearVelocity = Vector3.zero; // stop!!
			}

			result.linearAcceleration = desiredDirection * ownKS.maxAcceleration;
			result.angularAcceleration = desiredAngularDirection * ownKS.maxAngularAcceleration;

			return result;

		}
	}

}


/*
			
			*/