using UnityEngine;
using System.Collections;

namespace Steerings
{
	public class ObstacleAvoidance : SteeringBehaviour
	{

		public bool showWhisker = true;
		public float lookAheadLength = 10f;
		public float avoidDistance = 10f;
		public float secondaryWhiskerAngle = 30f;
		public float secondaryWhiskerRatio = 0.7f;

		private static GameObject surrogateTarget;


		public override SteeringOutput GetSteering ()
		{
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return ObstacleAvoidance.GetSteering (this.ownKS, showWhisker, lookAheadLength, 
				avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, bool showWhisker = true, 
			                                      float lookAheadLength = 10f, float avoidDistance = 10f,
												  float secondaryWhiskerAngle = 30f,
												  float secondaryWhiskerRatio = 0.7f) {

			// avoids colliding against a static not necessarily spherical object

			Vector2 mainDirection;
			Vector2 whisker1Direction, whisker2Direction, whisker3Direction;
			RaycastHit2D hit;

			if (surrogateTarget == null) {
				surrogateTarget = new GameObject ("Surrogate target for Obstacle avoidance");
			}
				
			// if we are not moving avoid obstacles just in front of us
			if (ownKS.linearVelocity.magnitude < 0.0001f) {
				mainDirection = Utils.OrientationToVector(ownKS.orientation);
			} else {
				mainDirection = ownKS.linearVelocity.normalized;
				//rayDirection = OrientationToVector(own.orientation);
			}

			// disable own collider if any
			Collider2D collider = ownKS.gameObject.GetComponent<Collider2D>();
			bool  before = false;
			if (collider != null) {
				before = collider.enabled;
				collider.enabled = false;
			}

			whisker1Direction = mainDirection;
			whisker2Direction = Utils.OrientationToVector (Utils.VectorToOrientation (mainDirection) + secondaryWhiskerAngle);
			whisker3Direction = Utils.OrientationToVector (Utils.VectorToOrientation (mainDirection) - secondaryWhiskerAngle);


			if (showWhisker) {
				Debug.DrawRay (ownKS.position, whisker1Direction*lookAheadLength);
				Debug.DrawRay (ownKS.position, whisker2Direction*lookAheadLength*secondaryWhiskerRatio);
				Debug.DrawRay (ownKS.position, whisker3Direction*lookAheadLength*secondaryWhiskerRatio);
			}

			// cast the ray and see if it has collided against something
			hit = Physics2D.Raycast (ownKS.position, whisker1Direction, lookAheadLength);
			if (hit.collider!=null) {
				// obstacle found
				surrogateTarget.transform.position = hit.point + hit.normal * avoidDistance;

				if (collider != null) {
					collider.enabled = before;
				}

				if (showWhisker) {
					Debug.DrawRay (ownKS.position, whisker1Direction * lookAheadLength, Color.red);
				}

				return Seek.GetSteering(ownKS, surrogateTarget);
			}

			// when here, "main whisker" found nothing. Let's try with a secondary one...


			hit = Physics2D.Raycast (ownKS.position, whisker2Direction, lookAheadLength*secondaryWhiskerRatio);
			if (hit.collider!=null) {
				// obstacle found

				surrogateTarget.transform.position = hit.point + hit.normal * avoidDistance;

				if (collider != null) {
					collider.enabled = before;
				}

				if (showWhisker) {
					Debug.DrawRay (ownKS.position, whisker2Direction * lookAheadLength*secondaryWhiskerRatio, Color.red);
				}

				return Seek.GetSteering (ownKS, surrogateTarget);
			}

			// when here, first secondary whisker found nothing. Let's try the other one


			hit = Physics2D.Raycast (ownKS.position, whisker3Direction, lookAheadLength*secondaryWhiskerRatio);
			if (hit.collider!=null) {
				// obstacle found

				surrogateTarget.transform.position = hit.point + hit.normal * avoidDistance;

				if (collider != null) {
					collider.enabled = before;
				}

				if (showWhisker) {
					Debug.DrawRay (ownKS.position, whisker3Direction * lookAheadLength*secondaryWhiskerRatio, Color.red);
				}

				return Seek.GetSteering (ownKS, surrogateTarget);
			}

			// when here, no whisker collided. No obstacle detected

			if (collider != null) {
				collider.enabled = before;
			}

			return null;

		}

	}
}



/* here a not very successful invention 

Vector2 main_rayDirection;
Vector2 rayDirection1, rayDirection2, rayDirection3;
RaycastHit2D hit1, hit2, hit3;
int hitCount = 0;



// disable own collider if any
Collider collider = ownKS.gameObject.GetComponent<Collider>();
bool  before = false;
if (collider != null) {
	before = collider.enabled;
	collider.enabled = false;
}

// if we are not moving avoid obstacles just in front of us
if (ownKS.linearVelocity.magnitude < 0.0001f) {
	main_rayDirection = Utils.OrientationToVector(ownKS.orientation);
} else {
	main_rayDirection = ownKS.linearVelocity.normalized;
	//rayDirection = OrientationToVector(own.orientation);
}

rayDirection1 = main_rayDirection;
hit1 = Physics2D.Raycast (ownKS.position, rayDirection1, lookAheadLength);
if (showWhisker) {
	Debug.DrawRay (ownKS.position, rayDirection1 * lookAheadLength);
}

rayDirection2 = Utils.OrientationToVector (Utils.VectorToOrientation (main_rayDirection) + secondaryWhiskerAngle);
hit2 = Physics2D.Raycast (ownKS.position, rayDirection2, lookAheadLength*secondaryWhiskerRatio);
if (showWhisker) {
	Debug.DrawRay (ownKS.position, rayDirection2 * lookAheadLength*secondaryWhiskerRatio);
}

rayDirection3 = Utils.OrientationToVector (Utils.VectorToOrientation (main_rayDirection) - secondaryWhiskerAngle);
hit3 = Physics2D.Raycast (ownKS.position, rayDirection3, lookAheadLength*secondaryWhiskerRatio);
if (showWhisker) {
	Debug.DrawRay (ownKS.position, rayDirection3 * lookAheadLength*secondaryWhiskerRatio);
}

// enable own collider if necessary
if (collider != null) {
	collider.enabled = before;
}

// process the hits...

// no hit, just return null
if (hit1.collider==null && hit2.collider==null && hit3.collider ==null) return null;

// if here at least one hit

// create surrogate target, if needed...
if (surrogateTarget == null) {
	surrogateTarget = new GameObject ("Surrogate target for Obstacle avoidance");
}

if (hit1.collider != null) {
	surrogateTarget.transform.position = hit1.point + hit1.normal * avoidDistance;
	if (showWhisker) {
		Debug.DrawRay (ownKS.position, rayDirection1 * lookAheadLength, Color.red);
	}
	hitCount++;
}
if (hit2.collider != null) {
	surrogateTarget.transform.position = hit2.point + hit2.normal * avoidDistance;
	if (showWhisker) {
		Debug.DrawRay (ownKS.position, rayDirection2 * lookAheadLength*secondaryWhiskerRatio, Color.red);
	}
	hitCount++;
}
if (hit3.collider != null) {
	Debug.DrawRay (ownKS.position, rayDirection3 * lookAheadLength*secondaryWhiskerRatio, Color.red);
	hitCount++;
}

Debug.Log ("hitCount: " + hitCount);

surrogateTarget.transform.position /= hitCount;

return Seek.GetSteering (ownKS, surrogateTarget);


*/