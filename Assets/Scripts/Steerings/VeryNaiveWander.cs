using UnityEngine;
using System.Collections;


namespace Steerings
{
	public class VeryNaiveWander : SteeringBehaviour
	{

		public float wanderRate=10f;

		public override SteeringOutput GetSteering ()
		{
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return VeryNaiveWander.GetSteering (this.ownKS, this.wanderRate);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, float wanderRate = 10f) {

			// "slightly" change your own orientation
			ownKS.orientation += Utils.binomial()*wanderRate;

			// and go where you look 
			return GoWhereYouLook.GetSteering (ownKS);
		}

	}
}
