using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class WanderAround : Wander
	{
	
		public float seekWeight = 0.2f; 
		public GameObject attractor;

		public override SteeringOutput GetSteering ()
		{
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return WanderAround.GetSteering (ownKS, attractor, seekWeight, ref targetOrientation, wanderRate, wanderRadius, wanderOffset);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, GameObject attractor, float seekWeight,  
			ref float targetOrientation, float wanderRate = 30f, 
			float wanderRadius = 10f, float wanderOffset = 20f) {

			SteeringOutput seekOutput = Seek.GetSteering (ownKS, attractor);
			SteeringOutput result = Wander.GetSteering (ownKS, ref targetOrientation, wanderRate, wanderRadius, wanderOffset);

			result.linearAcceleration = result.linearAcceleration * (1 - seekWeight) + seekOutput.linearAcceleration * seekWeight;
			result.angularAcceleration = result.angularAcceleration * (1 - seekWeight) + seekOutput.angularAcceleration * seekWeight;

			return result;
		}



		/*
		public override SteeringOutput GetSteering ()
		{
			SteeringOutput seekOutput = Seek.GetSteering (ownKS, attractor);
			SteeringOutput result = base.GetSteering ();

			result.linearAcceleration = result.linearAcceleration * (1 - seekWeight) + seekOutput.linearAcceleration * seekWeight;
			result.angularAcceleration = result.angularAcceleration * (1 - seekWeight) + seekOutput.angularAcceleration * seekWeight;

			return result;
		}*/

	}
}
