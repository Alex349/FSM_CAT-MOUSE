using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class EvadePlusAvoid : SteeringBehaviour
	{

		// parameters required by Evade
		public float maxPredictionTime = 3f;
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

			return EvadePlusAvoid.GetSteering (ownKS, target, maxPredictionTime,
				showWhisker, lookAheadLength, avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS,
			GameObject target,  float maxPredictionTime = 3f, 
			bool showWhishker=true, float lookAheadLength=10f, float avoidDistance=10f, float secondaryWhiskerAngle=30f, float secondaryWhiskerRatio=0.7f) {

			// give priority to obstacle avoidance
			SteeringOutput so = ObstacleAvoidance.GetSteering(ownKS, showWhishker, lookAheadLength, 
				avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);

			if (so == null) {
				return Evade.GetSteering (ownKS, target, maxPredictionTime);
			}

			return so;

		}
	}
}