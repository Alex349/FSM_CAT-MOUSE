using UnityEngine;
using System.Collections;

namespace Steerings
{
	// SEEK-based KEEP DISTANCE

	public class KeepDistance : SteeringBehaviour
	{

		private static GameObject surrogateTarget = null;

		public float requiredDistance = 40;

		public GameObject target;
		public GameObject Target {
			get {
				return target;
			}
			set {
				target = value;
			}
		}

		public override  SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return KeepDistance.GetSteering (this.ownKS, this.target, this.requiredDistance);
		} 

		public static SteeringOutput GetSteering (KinematicState ownKS, GameObject target, float requiredDistance = 40) {

			Vector3 directionToTarget;
			Vector3 desiredPosition;

			directionToTarget = (target.transform.position - ownKS.position).normalized;
			desiredPosition = target.transform.position - directionToTarget * requiredDistance;  
		
			/*
			 desiredPosition is a point at requiredDistance units of distance from the target, on the line from me to target
			 */
			// now desiredPosition is going to be the surrogate target for Seek

			if (KeepDistance.surrogateTarget == null) {
				surrogateTarget = new GameObject ("Surrogate for Keep distance");
				surrogateTarget.SetActive (false);
			}
			KeepDistance.surrogateTarget.transform.position = desiredPosition;

			return Seek.GetSteering (ownKS, KeepDistance.surrogateTarget);
		}


	}
}