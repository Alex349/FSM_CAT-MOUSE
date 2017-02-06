
using UnityEngine;

namespace Steerings
{
	public class Flee : SteeringBehaviour
	{
		public GameObject target;
		public GameObject Target {
			get {
				return target;
			}
			set {
				target = value;
			}
		}

		public override SteeringOutput GetSteering ()
		{
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return Flee.GetSteering (this.ownKS, this.target);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, GameObject target) {
			SteeringOutput result = Seek.GetSteering (ownKS, target);
			result.linearAcceleration = -result.linearAcceleration;
			return result;
		}
	}
}

