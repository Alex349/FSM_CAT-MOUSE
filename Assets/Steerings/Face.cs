using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class Face : SteeringBehaviour
	{

		public float closeEnoughAngle = 2f;
		public float slowDownAngle = 10f;
		public float timeToDesiredAngularSpeed = 0.1f;
		public GameObject target;

		private static GameObject surrogateTarget = null;

		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return Face.GetSteering (this.ownKS, this.target, this.closeEnoughAngle, this.slowDownAngle, this.timeToDesiredAngularSpeed);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, GameObject target,
			float targetAngularRadius,
			float slowDownAngularRadius,
			float timeToDesiredAngularSpeed) {

			Vector3 directionToTarget = target.transform.position - ownKS.position;

			if (surrogateTarget == null) {
				surrogateTarget = new GameObject ("Surrogate Target for Face");
			}
				
			surrogateTarget.transform.rotation = Quaternion.Euler (0, 0, Utils.VectorToOrientation(directionToTarget));

			// Align with surrogate target
			return Align.GetSteering (ownKS, surrogateTarget, targetAngularRadius, slowDownAngularRadius, timeToDesiredAngularSpeed);
		}
			

	}
}