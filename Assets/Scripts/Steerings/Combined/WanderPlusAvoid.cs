using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class WanderPlusAvoid : SteeringBehaviour
	{

		// parameters required by Wander
		public float wanderRate = 30f;
		public float wanderRadius = 10f;
		public float wanderOffset = 20f;
		private float targetOrientation = 0f;

		// parameters required by obstacle avoidance...
		public bool showWhisker = true;
		public float lookAheadLength = 10f;
		public float avoidDistance = 10f;
		public float secondaryWhiskerAngle = 30f;
		public float secondaryWhiskerRatio = 0.7f;


		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return WanderPlusAvoid.GetSteering (this.ownKS, wanderRate, wanderRate, wanderOffset, ref targetOrientation, 
				showWhisker, lookAheadLength, avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS,
			float WanderRate, float wanderRadius, float wanderOffset, ref float targetOrientation,
			bool showWhishker, float lookAheadLength, float avoidDistance, float secondaryWhiskerAngle, float secondaryWhiskerRatio) {

			// give priority to obstacle avoidance
			SteeringOutput so = ObstacleAvoidance.GetSteering(ownKS, showWhishker, lookAheadLength, 
				                                               avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);

			if (so == null) {
				return Wander.GetSteering (ownKS, ref targetOrientation, WanderRate, wanderRadius, wanderOffset);
			}

			return so;

		}
	}
}
