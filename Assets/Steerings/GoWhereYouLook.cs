using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class GoWhereYouLook : SteeringBehaviour
	{

		private static GameObject surrogateTarget = null;


		public override SteeringOutput GetSteering ()
		{
			return GoWhereYouLook.GetSteering (this.ownKS);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS) {
			// just "seek" your own direction

			if (GoWhereYouLook.surrogateTarget==null)
				GoWhereYouLook.surrogateTarget = new GameObject ("Surrogate target for go where you look");

			Vector3 myDirection = Utils.OrientationToVector (ownKS.orientation);
			GoWhereYouLook.surrogateTarget.transform.position = ownKS.position + myDirection;


			return Seek.GetSteering (ownKS, surrogateTarget);
		}
			
	}
}