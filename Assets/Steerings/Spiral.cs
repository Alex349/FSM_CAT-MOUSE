using UnityEngine;
using System.Collections;

namespace Steerings
{

	public class Spiral : SteeringBehaviour
	{

		private static GameObject surrogateTarget;

		public override SteeringOutput GetSteering () {
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();


			return Spiral.GetSteering (this.ownKS);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS) {

			SteeringOutput result = new SteeringOutput ();
			result.angularAcceleration = ownKS.maxAngularAcceleration;
			SteeringOutput r2 = GoWhereYouLook.GetSteering (ownKS);
			result.linearAcceleration = r2.linearAcceleration;

			return result;
		}
	}
}
