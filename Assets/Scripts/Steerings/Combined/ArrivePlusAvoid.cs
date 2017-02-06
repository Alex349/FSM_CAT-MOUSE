using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class ArrivePlusAvoid : SteeringBehaviour
	{

		// parameters required by Arrive
		public GameObject target;
		public float closeEnoughRadius = 5f;  // also know as targetRadius
		public float slowDownRadius = 20f;    // if inside this radius, slow down
		public float timeToDesiredSpeed = 0.1f; 


		// parameters required by obstacle avoidance...
		public bool showWhisker = true;
		public float lookAheadLength = 10f;
		public float avoidDistance = 10f;
		public float secondaryWhiskerAngle = 30f;
		public float secondaryWhiskerRatio = 0.7f;


		public override SteeringOutput GetSteering () {

			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return ArrivePlusAvoid.GetSteering (ownKS, target, closeEnoughRadius, slowDownRadius, timeToDesiredSpeed,
				showWhisker, lookAheadLength, avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS,
			GameObject target, float closeEnoughRadius = 5f, float slowDownRadius = 20f, float timeToDesiredSpeed = 0.1f, 
			bool showWhishker=true, float lookAheadLength=10f, float avoidDistance=10f, float secondaryWhiskerAngle=30f, float secondaryWhiskerRatio=0.7f) {

			// give priority to obstacle avoidance
			SteeringOutput so = ObstacleAvoidance.GetSteering(ownKS, showWhishker, lookAheadLength, 
				avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);

			if (so == null) {
				return Arrive.GetSteering (ownKS, target, closeEnoughRadius, slowDownRadius, timeToDesiredSpeed);
			}

			return so;

		}
	}
}