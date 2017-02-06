using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class FlockingAround : Flocking
	{

		public float seekWeight = 0.2f; 
		public GameObject attractor;

		public override SteeringOutput GetSteering ()
		{
			SteeringOutput seekOutput = Seek.GetSteering (ownKS, attractor);
			SteeringOutput result = base.GetSteering ();

			// beware, Flocking may return null. In that case, just apply seek

			if (result == null)
				return seekOutput;

			result.linearAcceleration = result.linearAcceleration * (1 - seekWeight) + seekOutput.linearAcceleration * seekWeight;
			result.angularAcceleration = result.angularAcceleration * (1 - seekWeight) + seekOutput.angularAcceleration * seekWeight;

			return result;
		}

	}
}
