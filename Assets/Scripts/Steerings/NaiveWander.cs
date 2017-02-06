using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class NaiveWander : SteeringBehaviour
	{

		public float wanderRate = 30f;
		public float targetAngularRadius = 2f;
		public float slowDownAngularRadius = 10f;
		public float timeToDesiredAngularSpeed = 0.1f;


		private static GameObject surrogateTarget = null;

		public override SteeringOutput GetSteering ()
		{ 
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return NaiveWander.GetSteering(this.ownKS, this.wanderRate, this.targetAngularRadius, this.slowDownAngularRadius, this.timeToDesiredAngularSpeed);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, float wanderRate=30f, float targetAngularRadius=2f,
			                                       float slowDownAngularRadius = 10f, float timeToDesiredAngularSpeed = 0.1f ) {
			// align with a surrogate target that has your new orientation and go there

			// slightly change the orientation
			float desiredOrientation = ownKS.orientation + wanderRate * Utils.binomial ();
			// give that orientation to the surrogate target

			if (NaiveWander.surrogateTarget == null) {
				NaiveWander.surrogateTarget = new GameObject ("Surrogate for Naive Wander");
				NaiveWander.surrogateTarget.SetActive (false);
			}
				

			NaiveWander.surrogateTarget.transform.rotation = Quaternion.Euler(0, 0, desiredOrientation);

			// align with the surrogate target
			SteeringOutput al = Align.GetSteering(ownKS, NaiveWander.surrogateTarget, targetAngularRadius, slowDownAngularRadius, timeToDesiredAngularSpeed);

			// go where you look (looked, actually)
			SteeringOutput gwyl = GoWhereYouLook.GetSteering(ownKS); // should never return null

			// combine, if possible
			if (al != null)
				gwyl.angularAcceleration = al.angularAcceleration;



			return gwyl;
		}
	
	}
}