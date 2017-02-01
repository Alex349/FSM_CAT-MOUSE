using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class FlockingPlusAvoid : SteeringBehaviour
	{

		// parameters required by flocking
		public string idTag = "BOID";
		public float cohesionThreshold = 40f;
		public float repulsionThreshold = 10f;
		public float wanderRate = 10f;

		// parameters required by obstacle avoidance...
		public bool showWhisker = true;
		public float lookAheadLength = 10f;
		public float avoidDistance = 10f;
		public float secondaryWhiskerAngle = 30f;
		public float secondaryWhiskerRatio = 0.7f;


		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return FlockingPlusAvoid.GetSteering (ownKS, idTag, cohesionThreshold, repulsionThreshold, wanderRate, 
				showWhisker, lookAheadLength, avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS,
			string idTag, float cohesionsThreshold, float repulsionThreshold, float wanderRate,
			bool showWhishker, float lookAheadLength, float avoidDistance, float secondaryWhiskerAngle, float secondaryWhiskerRatio) {

			// give priority to obstacle avoidance
			SteeringOutput so = ObstacleAvoidance.GetSteering(ownKS, showWhishker, lookAheadLength, 
				avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);

			if (so == null) {
				return Flocking.GetSteering (ownKS, idTag, cohesionsThreshold, repulsionThreshold, wanderRate);
			}

			return so;

		}
	}
}
