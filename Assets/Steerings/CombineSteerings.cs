using UnityEngine;
using System.Collections;

namespace Steerings
{

	// take any two steerings and BLEND them using given weights (WEIGHTED BLENDING)

	public class CombineSteerings : SteeringBehaviour
	{
		public string firstBehaviour, secondBehaviour; // names of the behaviours
		public float w1 = 0.5f, w2=0.5f; // weights

		private SteeringBehaviour bh1, bh2;

		protected override void Start () {
			base.Start ();
			bh1 = GetComponent (firstBehaviour) as SteeringBehaviour;
			bh2 = GetComponent (secondBehaviour) as SteeringBehaviour;

			if (bh1 == null || bh2 == null) {
				// make things crash...
				Debug.LogError ("null (inexistent, non-attached) steering behaviour in Combine Steerings");
			}
		}

		public override SteeringOutput GetSteering ()
		{
			return CombineSteerings.GetSteering (bh1, bh2, w1, w2);
		}

		public static SteeringOutput GetSteering (SteeringBehaviour bh1, SteeringBehaviour bh2, float w1=0.5f, float w2=0.5f) {
			SteeringOutput result;
			SteeringOutput first;
			SteeringOutput second;


			first = bh1.GetSteering ();
			second = bh2.GetSteering ();

			// beware. Some of them can be null...

			if (first == null && second == null)
				return null;
			if (first == null)
				return second;
			if (second == null)
				return first;

			result = new SteeringOutput ();
			result.linearAcceleration = first.linearAcceleration * w1 + second.linearAcceleration * w2;
			result.angularAcceleration = first.angularAcceleration * w1 +second.angularAcceleration * w2;

			return result;
		}


			
	}
}