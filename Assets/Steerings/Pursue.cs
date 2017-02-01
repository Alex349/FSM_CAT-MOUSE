using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class Pursue : SteeringBehaviour
	{
		// INTERCEPT

		public float maxPredictionTime = 3f;
		public GameObject target;

		private static GameObject surrogateTarget = null;

		public override SteeringOutput GetSteering ()
		{
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return Pursue.GetSteering (this.ownKS, this.target, this.maxPredictionTime);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, GameObject target, float maxPredictionTime=3f) {
			// we need to know the kinematic state of the target since we need to know its linear velocity

			// if target has no kinematic state "give up" and just seek
			KinematicState targetKS = target.GetComponent<KinematicState> ();
			if (targetKS == null) {
				Debug.Log("Pursue invoked with a target that has no kinematic state attached. Resorting to Seek");
				Debug.Log ("target is "+target);
				return Seek.GetSteering (ownKS, target);
			}

			Vector3 directionToTarget = targetKS.position - ownKS.position;
			float distanceToTarget = directionToTarget.magnitude;
			float currentSpeed = ownKS.linearVelocity.magnitude;

			// determine the time it will take to reach the target
			float predictedTimeToTarget = distanceToTarget / currentSpeed;
			if (predictedTimeToTarget > maxPredictionTime) {
				predictedTimeToTarget = maxPredictionTime;
			}

			// now determine future (at predicted time) location of target
			Vector3 futurePositionOfTarget = targetKS.position + targetKS.linearVelocity*predictedTimeToTarget;

			// create surrogate target and place it at future location
			if (Pursue.surrogateTarget == null)
				Pursue.surrogateTarget = new GameObject ("Dummy (Surrogate Target for Pursue)");
			Pursue.surrogateTarget.transform.position = futurePositionOfTarget;

			// delegate to seek
			return Seek.GetSteering(ownKS, surrogateTarget);
			// could also delegate to Arrive if overshooting is an issue...
		}

	}
}