using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class KBDSteerPlusAvoid : SteeringBehaviour
	{

		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return KBDSteerPlusAvoid.GetSteering (this.ownKS);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS) {

			// give priority to obstacle avoidance
			SteeringOutput so = ObstacleAvoidance.GetSteering(ownKS, true, 30f, 30f);

			if (so == null) {
				return KBDSteer01.GetSteering (ownKS);
			}

			return so;

		}
	}
}
