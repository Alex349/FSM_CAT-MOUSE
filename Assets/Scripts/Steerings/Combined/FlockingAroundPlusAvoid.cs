using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class FlockingAroundPlusAvoid : FlockingAround
	{

		// parameters required by obstacle avoidance...
		public bool showWhisker = true;
		public float lookAheadLength = 10f;
		public float avoidDistance = 10f;
		public float secondaryWhiskerAngle = 30f;
		public float secondaryWhiskerRatio = 0.7f;


		public override SteeringOutput GetSteering () {

			SteeringOutput avoidance = ObstacleAvoidance.GetSteering (ownKS, showWhisker, lookAheadLength, 
				avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);

			if (avoidance != null)
				return avoidance;

			return base.GetSteering ();
		}


	}
}
