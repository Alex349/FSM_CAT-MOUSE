using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class FleePlusAvoid : SteeringBehaviour
	{

		// parameters required by Flee
		public GameObject target;



		// parameters required by obstacle avoidance...
		public bool showWhisker = true;
		public float lookAheadLength = 10f;
		public float avoidDistance = 10f;
		public float secondaryWhiskerAngle = 30f;
		public float secondaryWhiskerRatio = 0.7f;


		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return FleePlusAvoid.GetSteering (ownKS, target,
				showWhisker, lookAheadLength, avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS,
			GameObject target,  
			bool showWhishker=true, float lookAheadLength=10f, float avoidDistance=10f, float secondaryWhiskerAngle=30f, float secondaryWhiskerRatio=0.7f) {

			// give priority to obstacle avoidance
			SteeringOutput so = ObstacleAvoidance.GetSteering(ownKS, showWhishker, lookAheadLength, 
				avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);

			if (so == null) {
				return Flee.GetSteering (ownKS, target);
			}

			return so;

		}
	}
}