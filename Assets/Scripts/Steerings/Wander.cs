using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class Wander : SteeringBehaviour
	{

		public float wanderRate = 30f;
		public float wanderRadius = 10f;
		public float wanderOffset = 20f;

		protected float targetOrientation = 0f;

		private static GameObject surrogateTarget = null;

		public override SteeringOutput GetSteering ()
		{ 
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return Wander.GetSteering (ownKS, ref targetOrientation, wanderRate, wanderRadius, wanderOffset);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, 
			ref float targetOrientation, float wanderRate = 30f, 
			float wanderRadius = 10f, float wanderOffset = 20f) {

			if (surrogateTarget == null) {
				surrogateTarget = new GameObject ("Surrogate target for Wander");
			}

			// change target orientation
			targetOrientation += wanderRate * Utils.binomial ();

			// place surrogate target on circle of wanderRadius
			surrogateTarget.transform.position = Utils.OrientationToVector(targetOrientation)*wanderRadius;

			// place "in front"
			surrogateTarget.transform.position += ownKS.position + Utils.OrientationToVector(ownKS.orientation)*wanderOffset;

			// Seek the surrogate target
			return Seek.GetSteering(ownKS, surrogateTarget);
		}
	}
}
