using UnityEngine;
using System;

namespace Steerings
{
	public class Flocking : SteeringBehaviour
	{
		public string idTag = "BOID";
		public float cohesionThreshold = 40f;
		public float repulsionThreshold = 10f;
		public float wanderRate = 10f;

		private static GameObject surrogateTarget;
		private static KinematicState surrogateKS;  // kinematic state for surrogate target

		public override SteeringOutput GetSteering ()
		{
			// no KS? get it
			if (this.ownKS==null) this.ownKS = GetComponent<KinematicState>();

			return Flocking.GetSteering (this.ownKS, idTag, cohesionThreshold, repulsionThreshold, wanderRate);
		}


		public static SteeringOutput GetSteering (KinematicState ownKS, string idTag="BOID", 
			                                      float cohesionThreshold = 40f, float repulsionThreshold = 10f,
												  float wanderRate = 10f) {
		
			float distanceToBoid;
			KinematicState boid;
			Vector3 averageVelocity = Vector3.zero;
			int count = 0;

			// get all the other boids
			GameObject [] boids = GameObject.FindGameObjectsWithTag(idTag);


			// ... and iterate to find average velocity
			for (int i = 0; i < boids.Length; i++) {
				// skip yourself
				if (boids[i]==ownKS.gameObject) continue;

				boid = boids [i].GetComponent<KinematicState> ();
				if (boid == null) {
					// this should never happen but you never know
					Debug.Log("Incompatible mate in flocking. Flocking mates must have a kinematic state attached: "+boids[i]);
					continue;
				}

				// disregard distant boids
				distanceToBoid = (boid.position - ownKS.position).magnitude;
				if (distanceToBoid > Math.Max(cohesionThreshold, repulsionThreshold))
					continue;

				averageVelocity = averageVelocity + boid.linearVelocity;
				count++;

			} // end of iteration to find average velocity

			if (count > 0)
				averageVelocity = averageVelocity / count;
			else
				return null;
			// if no boid is close enough (count==0) there's no flocking to be performed so return null
			// could also apply some wandering 


			if (surrogateTarget == null) {
				surrogateTarget = new GameObject ("surrogate target for Flocking");
				surrogateKS = surrogateTarget.AddComponent<KinematicState> ();
			}

			surrogateKS.linearVelocity = averageVelocity;
			SteeringOutput vm = VelocityMatching.GetSteering (ownKS, surrogateTarget); // (in normal conditions) this does NOT return null  
			SteeringOutput rp = LinearRepulsion.GetSteering(ownKS, idTag, repulsionThreshold); // this MAY return null
			SteeringOutput co = Cohesion.GetSteering(ownKS, idTag, cohesionThreshold); // this MAY return null

			// avoid nasty problems due to null references
			if (rp == null) {
				rp = new SteeringOutput ();
			}
			if (co == null) {
				co = new SteeringOutput ();
			}

			SteeringOutput result = new SteeringOutput ();
			result.linearAcceleration = vm.linearAcceleration * 0.4f +
				rp.linearAcceleration * 2f +
				co.linearAcceleration * 1f;

			// and now let's add some wandering to make things less predictable
			SteeringOutput wd = VeryNaiveWander.GetSteering(ownKS, wanderRate);
			result.linearAcceleration += wd.linearAcceleration * 1f;


			// clip if necessary
			if (result.linearAcceleration.magnitude > ownKS.maxAcceleration)
				result.linearAcceleration = result.linearAcceleration.normalized * ownKS.maxAcceleration;

			return result;

		}


	}
}

