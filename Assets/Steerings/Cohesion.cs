using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class Cohesion : SteeringBehaviour
	{
		public string idTag = "ATTRACTIVE";
		public float cohesionThreshold = 20f;

		private static GameObject surrogateTarget;

		public override SteeringOutput GetSteering ()
		{
			if (this.ownKS == null) this.ownKS = GetComponent<KinematicState>();

			return Cohesion.GetSteering (this.ownKS, this.idTag, this.cohesionThreshold);
		}


		public static SteeringOutput GetSteering (KinematicState ownKS, string tag, float cohesionThreshold=20f) {
			Vector3 centerOfMass = Vector3.zero;
			int count = 0;
			float distanceToMate;

			// get all your mates (potential targets) 
			GameObject [] mates = GameObject.FindGameObjectsWithTag(tag);

			// iterate to compute center of mass
			for (int i = 0; i < mates.Length; i++) {
				// skip yourself...
				if (mates [i] == ownKS.gameObject)
					continue;

				// Only consider close mates. Disregard far ones
				distanceToMate = (mates[i].transform.position - ownKS.position).magnitude;
				if (distanceToMate < cohesionThreshold) {
					centerOfMass = centerOfMass + mates [i].transform.position;
					count++;
				}
			}
				

			if (count == 0)
				return null;

			centerOfMass = centerOfMass / count;

			// generate a surrogate target and delegate to seek or arrive...
			if (surrogateTarget == null) {
				surrogateTarget = new GameObject ("Surrogate target for cohesion");
			}
			surrogateTarget.transform.position = centerOfMass;

			return Seek.GetSteering (ownKS, surrogateTarget);
		}

	}
}