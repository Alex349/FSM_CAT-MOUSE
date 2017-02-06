using UnityEngine;
using System.Collections;

namespace Steerings
{

	public class Seek : SteeringBehaviour
	{

		public  GameObject target;
		public GameObject Target {
			get {
				return target;
			}
			set {
				target = value;
			}
		}

		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			if (this.target == null)
				Debug.Log ("Null target in Seek of "+this.gameObject );

			return Seek.GetSteering (this.ownKS, this.target);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, GameObject target) {
			SteeringOutput steering = new SteeringOutput ();
			Vector3 directionToTarget;

			// Compute direction to target
			directionToTarget = target.transform.position - ownKS.position;
			directionToTarget.Normalize ();

			// give maxAcceleration towards the target
			steering.linearAcceleration = directionToTarget * ownKS.maxAcceleration;



			return steering;
		}
	}
}
