using UnityEngine;
using System.Collections;

namespace Steerings
{

	public class Steerings2 : MonoBehaviour
	{

		public SteeringType steeringType;

		public bool lookWhereYouGoInstantaneous = false;

		// basic parameters
		public float maxAcceleration = 2f;
		public float maxSpeed = 10f;
		public float maxAngularAcceleration = 45f;
		public float maxAngularSpeed = 45f; // max rotation

		// the target
		public GameObject target;

		// current kinematic state
		private KinematicState ownKS;

		// arrive parameters
		public float targetRadius = 5f;
		public float slowDownRadius = 20f;
		public float timeToDesiredSpeed = 0.1f;

		// align parameters
		public float targetAngularRadius = 2f;
		public float slowDownAngularRadius = 10f;
		public float timeToDesiredAngularSpeed = 0.1f;

		// pursue (intercept parameters)
		public float maxPredictionTime = 3f;

		// wander parameters
		public float wanderRate = 30f; // in DEGs
		public float wanderRadius = 5f;
		public float wanderDistance = 10f;

		// repulsion parameters
		public float repulsionThreshold = 20f;

		// cohesion parameters
		public float cohesionThreshold = 100f;

		// obstacle avoidance parameters
		public float avoidDistance = 10f;
		public float lookAheadLength = 10f;
		public float whisker_1_angle = 20;
		public float whisker_2_angle = 40;
		 

		// debug pursposes
		public float p_safetyDistance;

		private KinematicState wanderTarget; //surrogate (state of) target for wandering

		// Use this for initialization
		void Start ()
		{
			// initialize own kinematic state
			ownKS = new KinematicState ();
			ownKS.angularSpeed = 0;
			ownKS.linearVelocity = Vector3.zero;
			ownKS.position = transform.position;
			ownKS.orientation = transform.eulerAngles.z;

			//avoidDistance = determineRadius ();

			//ownKS.orientation = 180 * binomial (); 

			wanderTarget = new KinematicState ();
		}
			
	
		// Update is called once per frame
		void Update ()
		{
			float dt = Time.deltaTime;

			SteeringOutput steering = ComputeSteering ();
			if (steering == null) {
				// no steering, do nothing
				ownKS.linearVelocity = Vector3.zero; // stop!!!
				ownKS.angularSpeed = 0f; // stop!!!
			} else {
				// apply linear steering: change linear velocity and position 
				ownKS.linearVelocity = ownKS.linearVelocity + steering.linearAcceleration*dt; // v=v+a·t
				if (ownKS.linearVelocity.magnitude > maxSpeed)
					ownKS.linearVelocity = ownKS.linearVelocity.normalized * maxSpeed; // clipping of velocity
				ownKS.position = ownKS.position + ownKS.linearVelocity*dt + 0.5f*steering.linearAcceleration*dt*dt; // s = v·t + 1/2(a·t^2)


				// DEBUG

				if (float.IsNaN (ownKS.position.x))
					Debug.Log ("NaN detected");

				// -- DEBUG

				// apply angular steering: change angular velocity and orientation
				ownKS.angularSpeed = ownKS.angularSpeed + steering.angularAcceleration*dt;
				if (Mathf.Abs(ownKS.angularSpeed) > maxAngularSpeed)
					ownKS.angularSpeed = maxAngularSpeed * Mathf.Sign(ownKS.angularSpeed); // clip if necessary
				ownKS.orientation = ownKS.orientation + ownKS.angularSpeed*dt+0.5f*steering.angularAcceleration*dt*dt;

				// change position and orientation
				transform.position = ownKS.position;
				if (lookWhereYouGoInstantaneous) {
					if (ownKS.linearVelocity.magnitude > 0.001f) {
						//transform.rotation = Quaternion.Euler (90, VectorToOrientation (ownKS.linearVelocity), 0);
						transform.rotation = Quaternion.Euler (0, 0,  VectorToOrientation (ownKS.linearVelocity));
					}
				} else {
					//transform.rotation = Quaternion.Euler (90, ownKS.orientation, 0);
					transform.rotation = Quaternion.Euler(0, 0, ownKS.orientation);
				}
				//ownKS.orientation = transform.rotation.eulerAngles.y; // regrabbing
			}
		}
			

		private SteeringOutput RoomWander (KinematicState own) {

			SteeringOutput obstacleAvoidance = ObstacleAvoidance2D (own);
			SteeringOutput wander = NaiveWander(own);


			if (obstacleAvoidance == null) {
				wander.linearAcceleration = wander.linearAcceleration * 0.8f;
				return wander;
			}
			else {
				return obstacleAvoidance;
			}
		}


		private SteeringOutput ConfinedFlocking (KinematicState own) {
			SteeringOutput obstacleAvoidance = ObstacleAvoidance2D (own);
			SteeringOutput flocking  = Flocking(own);


			if (obstacleAvoidance == null) {
				return flocking;
			}
			else {
				return obstacleAvoidance;
			}
		}


		private SteeringOutput ScaredFish (KinematicState own) {
			SteeringOutput obstacleAvoidance = ObstacleAvoidance2D (own);
			//SteeringOutput flocking  = Flocking(own);
			//SteeringOutput collision = CollisionAvoidance (own, "PREDATOR");

			if (obstacleAvoidance == null) {
				GameObject pred = GameObject.FindGameObjectWithTag ("PREDATOR");
				KinematicState targetKS = pred.GetComponent<Steerings2> ().ownKS;
				if ((own.position - targetKS.position).magnitude < 200) {
					return Flee (own, targetKS);
				} else {
					return Flocking (own);
				}
			}
			else {
				return obstacleAvoidance;
			}
		}


		private SteeringOutput ObstacleAvoidance2D (KinematicState own) {
			// avoids colliding against a static not necessarily spherical object

			Vector2 main_rayDirection;
			Vector2 rayDirection;


			// if we are not moving avoid obstacles just in front of us
			if (own.linearVelocity.magnitude < 0.0001f) {
				main_rayDirection = OrientationToVector(own.orientation);
			} else {
				main_rayDirection = own.linearVelocity.normalized;
				//rayDirection = OrientationToVector(own.orientation);
			}
				
			rayDirection = main_rayDirection;



			// cast the ray and see if it has collided against something
			RaycastHit2D hit;
			//Debug.DrawRay (own.position, rayDirection* lookAheadLength);
			hit = Physics2D.Raycast (own.position, rayDirection, lookAheadLength);
			if (hit.collider!=null) {
				// obstacle found
				
					Debug.Log("Main whisker avoidance");
					KinematicState surrogateTarget = new KinematicState();
					surrogateTarget.position = hit.point + hit.normal * avoidDistance;
					return Seek (own, surrogateTarget);
					
			}

			// when here, "main whisker" found nothing. Let's try with a secondary one...

			rayDirection = OrientationToVector (VectorToOrientation (main_rayDirection) + whisker_1_angle);
			//Debug.DrawRay (own.position, rayDirection* lookAheadLength*0.7f);
			hit = Physics2D.Raycast (own.position, rayDirection, lookAheadLength*0.7f);
			if (hit.collider!=null) {
				// obstacle found
					
					Debug.Log("Whisker 1 avoidance");
					KinematicState surrogateTarget = new KinematicState();
					surrogateTarget.position = hit.point + hit.normal * avoidDistance;
					return Seek (own, surrogateTarget);
				
			}

			// when here, first secondary whisker found nothing. Let's try another one...

			rayDirection = OrientationToVector (VectorToOrientation (main_rayDirection) - whisker_1_angle);
			//Debug.DrawRay (own.position, rayDirection* lookAheadLength*0.7f);
			hit = Physics2D.Raycast (own.position, rayDirection, lookAheadLength*0.7f);
			if (hit.collider!=null) {
				// obstacle found
					
					Debug.Log("Whisker 2 avoidance");
					KinematicState surrogateTarget = new KinematicState();
					surrogateTarget.position = hit.point + hit.normal * avoidDistance;
					return Seek (own, surrogateTarget);
					
			}

			// short whisker 1

			rayDirection = OrientationToVector (VectorToOrientation (main_rayDirection) + whisker_2_angle);
			//Debug.DrawRay (own.position, rayDirection* lookAheadLength*0.5f);
			hit = Physics2D.Raycast (own.position, rayDirection, lookAheadLength*0.5f);
			if (hit.collider!=null) {
				// obstacle found
					Debug.Log("Short Whisker 1 avoidance");
					KinematicState surrogateTarget = new KinematicState();
					surrogateTarget.position = hit.point + hit.normal * avoidDistance;
					return Seek (own, surrogateTarget);
			}

			// short whisker 2

			rayDirection = OrientationToVector (VectorToOrientation (main_rayDirection) - whisker_2_angle);
			//Debug.DrawRay (own.position, rayDirection* lookAheadLength*0.5f);
			hit = Physics2D.Raycast (own.position, rayDirection, lookAheadLength*0.5f);
			if (hit.collider!=null) {
				// obstacle found

					Debug.Log("Short Whisker 2 avoidance");
					KinematicState surrogateTarget = new KinematicState();
					surrogateTarget.position = hit.point + hit.normal * avoidDistance;
					return Seek (own, surrogateTarget);
			
			}
				
			Debug.Log ("no obstacle");
			return null;
		}
			


		private SteeringOutput CollisionAvoidance (KinematicState own, string tag) {
			// avoids colliding against a moving spherical object

			float shortestTime = float.PositiveInfinity;
			KinematicState firstTarget = null;
			float firstMinSeparation = 0;
			float firstDistance=0;
			Vector3 firstRelativePosition = Vector3.zero, relativePosition;
			Vector3 firstRelativeVelocity = Vector3.zero, relativeVelocity;
			float distance, minSeparation=0f;
			float relativeSpeed;
			float timeToCollision;
			float radius, myRadius, firstRadius=0;
			KinematicState target;

			// get all the targets 
			// Steerings2 [] targets = GetComponents<Steerings2>();
			//Steerings2 [] targets = FindObjectsOfType(typeof (Steerings2)) as Steerings2[];
			GameObject [] tgts = GameObject.FindGameObjectsWithTag(tag);
			Steerings2 [] targets = new Steerings2[tgts.Length];
			for (int i = 0; i < tgts.Length; i++) {
				targets [i] = tgts [i].GetComponent<Steerings2> ();
			}
 
			myRadius = Mathf.Max (GetComponent<Renderer> ().bounds.size.x / 2f, 
				GetComponent<Renderer> ().bounds.size.y / 2f);

			// iterate thorough all the targets
			for (int i = 0; i < targets.Length; i++) {
				// do not consider yourself...
				if (targets [i].ownKS == own) 
					continue;

				target = targets [i].ownKS;
				relativePosition = target.position - own.position;
				relativeVelocity = target.linearVelocity - own.linearVelocity;
				relativeSpeed = relativeVelocity.magnitude;

				timeToCollision = -(DotProduct (relativePosition, relativeVelocity))/(relativeSpeed*relativeSpeed);

				if (float.IsNaN (timeToCollision)) {
					//Debug.Log ("No collisions");
					continue; 
				}
				if (timeToCollision < 0) {
					//Debug.Log ("No collisions. Object getting away");
					continue;
				}
				if (timeToCollision > 10) {
					//Debug.Log ("Possible collision too distant in time");
					continue; // don't mind distant collisions...
				} 

				// is there going to be a collision at all?
				distance = relativePosition.magnitude; // current distance
				minSeparation = distance - relativeSpeed * timeToCollision;
				radius = Mathf.Max (targets [i].GetComponent<Renderer>().bounds.size.x / 2,
					targets [i].GetComponent<Renderer>().bounds.size.y / 2);

				if (minSeparation > (myRadius+radius)) {
					Debug.Log ("No collisions. Object will not get too close");
					continue; // there'll be no collision at all
				}

				Debug.Log ("Collision Detected. Min separation is: "+minSeparation+" but required is: "+(myRadius+radius));
				// is this the first object we're are going to collide with?
				if (timeToCollision < shortestTime) {
					// save data...
					shortestTime = timeToCollision;
					firstTarget = target;
					firstMinSeparation = minSeparation;
					firstDistance = distance;
					firstRelativePosition = relativePosition;
					firstRelativeVelocity = relativeVelocity;
					firstRadius = radius;
				}
			} // end of iteration;

			if (firstTarget == null)
				return null;

			// if we're going to hit exactly or we're already colliding do the
			// steering based on the current position
			// else do the steering based on the future collision position
			if (firstMinSeparation <= 0 || firstDistance < (myRadius+firstRadius))
				relativePosition = firstRelativePosition;  // relative position is direction to target
			else
				relativePosition = firstRelativePosition + firstRelativeVelocity * shortestTime;

			Debug.DrawRay (transform.position, relativePosition, Color.red);

			SteeringOutput result = new SteeringOutput ();
			result.linearAcceleration = -relativePosition.normalized * maxAcceleration;

			return result;
		}
			
		private SteeringOutput Seek (KinematicState own, KinematicState target)
		{
			SteeringOutput steering = new SteeringOutput ();
			Vector3 directionToTarget;

			// Compute direction to target
			directionToTarget = target.position - own.position;
			directionToTarget.Normalize ();

			// give maxAcceleration towards the target
			steering.linearAcceleration = directionToTarget * maxAcceleration;

			return steering;
		}

		private SteeringOutput Flee (KinematicState own, KinematicState target)
		{
			SteeringOutput steering;

			steering = Seek (own, target);
			steering.linearAcceleration *= -1; // reverse sense of acceleration

			return steering;
		}

		private SteeringOutput Arrive (KinematicState own, KinematicState target)
		{
			SteeringOutput steering = new SteeringOutput ();
			Vector3 directionToTarget;
			float distanceToTarget;
			float desiredSpeed;
			Vector3 desiredVelocity;
			Vector3 requiredAcceleration;

			// compute direction and distance to target
			directionToTarget = target.position - own.position;
			distanceToTarget = directionToTarget.magnitude;

			// if we're already there, no steering required
			if (distanceToTarget < targetRadius)
				return null;

			// if we're are far away from target, let's go full speed
			// if we're getting closer speed has to be inversely proportional to distance
			if (distanceToTarget > slowDownRadius) {
				desiredSpeed = maxSpeed;
			} else {
				desiredSpeed = maxSpeed * (distanceToTarget / slowDownRadius);
			}

			// desired velocity is towards the target
			desiredVelocity = directionToTarget.normalized * desiredSpeed;

			// compute the acceleration required to get desiredVelocity in timeToDesiredSpeed
			// take into account that we already have a velocity
			requiredAcceleration = (desiredVelocity - own.linearVelocity) / timeToDesiredSpeed;

			// if required acceleration is too high, clip it
			if (requiredAcceleration.magnitude > maxAcceleration) {
				requiredAcceleration = requiredAcceleration.normalized * maxAcceleration;
			}

			steering.linearAcceleration = requiredAcceleration;

			return steering;
		}


		private SteeringOutput goWhereYouLook (KinematicState own) {
			// just "seek" your own orientation

			Vector3 myDirection = OrientationToVector (own.orientation);

			KinematicState surrogateTarget = new KinematicState ();
			surrogateTarget.position = own.position + myDirection;

			// delegate to seek
			return Seek(own, surrogateTarget);
		}

		private SteeringOutput Pursue (KinematicState own, KinematicState target)
		// INTERCEPT
		{

			Vector3 directionToTarget = target.position - own.position;
			float distanceToTarget = directionToTarget.magnitude;
			float currentSpeed = own.linearVelocity.magnitude;

			// determine the time it will take to reach the target
			float predictedTimeToTarget = distanceToTarget / currentSpeed;
			if (predictedTimeToTarget > maxPredictionTime) {
				predictedTimeToTarget = maxPredictionTime;
			}

			// now determine future (at predicted time) location of target
			Vector3 futurePositionOfTarget = target.position + target.linearVelocity*predictedTimeToTarget;

			// create surrogate target and place it at future location
			KinematicState surrogateTarget = new KinematicState ();
			surrogateTarget.position = futurePositionOfTarget;

			// delegate to arrive
			return Arrive(own, surrogateTarget);
			// could also delegate to seek... but we risk overshooting...
			// return Seek(own, surrogateTarget);
		}
			

		private SteeringOutput NaiveWander (KinematicState own) {
			// align with a surrogate target that has your new orientation and go there

			SteeringOutput align, goWYL;
			KinematicState surrogateTarget = new KinematicState ();

			// "sligtly" change the orientation
			float desiredOrientation = own.orientation + wanderRate * binomial ();

			// give that orientation to the surrogate target
			surrogateTarget.orientation = desiredOrientation;
			// align
			align = Align (own, surrogateTarget);

			// go where you look (move "forward");
			goWYL = goWhereYouLook(own); //goWhereYouLook never returns null... 

			// combine and return

			if (align!=null && goWYL!=null) 
				goWYL.angularAcceleration = align.angularAcceleration;

			return goWYL;
		}

		private SteeringOutput Wander (KinematicState own, KinematicState wanderTarget) {

			// update orientation of wanderTarget
			wanderTarget.orientation = wanderTarget.orientation + wanderRate*binomial();


			// place the center of the wander circle
			wanderTarget.position = own.position + wanderDistance * OrientationToVector (own.orientation);
			// place the target somewhere in the wander circle
			wanderTarget.position = wanderTarget.position + wanderRadius*OrientationToVector(wanderTarget.orientation+own.orientation);
			/* why wanderTarget.orientation+own.orientation? 
			 * because wanderTarget.orintation is in local -own- system and must be in world system 
			 */


			// face the target
			SteeringOutput face = Face(own, wanderTarget);

			// go where you look (move "forward");
			SteeringOutput goWYL = goWhereYouLook(own); //goWhereYouLook never returns null... 

			// combine and return

			if (face!=null && goWYL!=null) 
				goWYL.angularAcceleration = face.angularAcceleration;

			return goWYL;
		}

		private SteeringOutput VelocityMatching (KinematicState own, KinematicState target) {

			SteeringOutput result = new SteeringOutput ();

			// compute required acceleration to have same velocity
			result.linearAcceleration = (target.linearVelocity - own.linearVelocity) / timeToDesiredSpeed;

			// clip if necessary
			if (result.linearAcceleration.magnitude > maxAcceleration)
				result.linearAcceleration = result.linearAcceleration.normalized * maxAcceleration;

			return result;
		}


		private SteeringOutput LinearRepulsion (KinematicState own, string tag = "REPULSIVE") {

			Vector3 directionToTarget;
			float distanceToTarget;
			float repulsionStrength = 0;
			int activeTargets = 0;

			SteeringOutput result = new SteeringOutput ();

			// get all potential "targets"
			GameObject [] targets = GameObject.FindGameObjectsWithTag(tag);


			// iterate over all repulsive targets
			for (int i = 0; i < targets.Length; i++) {
				// do not take yourself into account
				if (targets[i].GetComponent<Steerings2>()==this) continue;

				directionToTarget = targets [i].transform.position - own.position;
				distanceToTarget = directionToTarget.magnitude;
				if (distanceToTarget < repulsionThreshold) {
					// a repulsive object is too close. Do someting
					activeTargets++;
					repulsionStrength = maxAcceleration*(repulsionThreshold-distanceToTarget)/repulsionThreshold;
					result.linearAcceleration = result.linearAcceleration - directionToTarget.normalized * repulsionStrength;
				}
				// clip if necessary
				if (result.linearAcceleration.magnitude > maxAcceleration)
					result.linearAcceleration = result.linearAcceleration.normalized * maxAcceleration;
			}

			if (activeTargets > 0)
				return result;
			else
				return null;
		}

		private SteeringOutput Cohesion (KinematicState own, string tag = "ATTRACTIVE") {

			Vector3 centerOfMass = Vector3.zero;
			int count = 0;
			float distanceToMate;

			// get all your mates (potential targets) 
			GameObject [] mates = GameObject.FindGameObjectsWithTag(tag);

			// iterate to compute center of mass
			for (int i = 0; i < mates.Length; i++) {
				// Only consider close mates. Disregard far ones
				distanceToMate = (mates[i].transform.position - own.position).magnitude;
				if (distanceToMate < cohesionThreshold) {
					centerOfMass = centerOfMass + mates [i].transform.position;
					count++;
				}
			}

			if (count == 0)
				return null;
			
			centerOfMass = centerOfMass / count;

			// generate a surrogate target and delegate to arrive 

			KinematicState surrogateTarget = new KinematicState ();
			surrogateTarget.position = centerOfMass;
			//return Arrive (own, surrogateTarget);
			return Seek (own, surrogateTarget);
		}

		private SteeringOutput Flocking (KinematicState own, string tag="BOID") {

			float distanceToMate;
			KinematicState mate;
			Vector3 averageVelocity = Vector3.zero;
			int count = 0;


			// get all your mates...
			GameObject [] mates = GameObject.FindGameObjectsWithTag(tag);
			// and iterate to find average velocity
			for (int i = 0; i < mates.Length; i++) {
				// skip yourself
				if (mates[i].GetComponent<Steerings2>()==this) continue;

				mate = mates [i].GetComponent<Steerings2> ().ownKS;
				if (mate == null) {
					// this should never happen
					Debug.LogError("Incompatible mate in flocking");
					continue;
				}
				// disregard distant mates. Consider cohesionThreshold
				distanceToMate = (mate.position - own.position).magnitude;
				if (distanceToMate > cohesionThreshold)
					continue;
				
				averageVelocity = averageVelocity + mate.linearVelocity;
				count++;
			} // end of iteration
			if (count>0)
				averageVelocity = averageVelocity / count;

			// velocity matching with a surrogateTarget
			KinematicState surrogateTarget = new KinematicState();
			surrogateTarget.linearVelocity = averageVelocity;

			SteeringOutput velMatching = VelocityMatching (own, surrogateTarget);
			SteeringOutput repulsion = LinearRepulsion (own, tag);
			SteeringOutput cohesion = Cohesion (own, tag);
			SteeringOutput wander = NaiveWander (own);
			//SteeringOutput wander = Wander (own, wanderTarget);

			//string status = "xxx ";

			// avoid nasty errors due to null objects
			if (velMatching == null) {
				velMatching = new SteeringOutput ();
				//status = status  + "velmatching is null";
			}
			if (repulsion == null) {
				repulsion = new SteeringOutput ();
				//status = status  + " repulsion is null";
			}
			if (cohesion == null) {
				cohesion = new SteeringOutput ();
				//status = status  + " cohesion is null";
			}
			if (wander == null) {
				wander = new SteeringOutput ();
				//status = status  + " wander is null";
			}

			//Debug.Log (status);

			SteeringOutput result = new SteeringOutput ();
			result.linearAcceleration = velMatching.linearAcceleration * 0.4f +
			repulsion.linearAcceleration * 2f +
			cohesion.linearAcceleration * 1f +
			wander.linearAcceleration * 0.5f;

			// clip if necessary
			if (result.linearAcceleration.magnitude > maxAcceleration)
				result.linearAcceleration = result.linearAcceleration.normalized * maxAcceleration;



			// add the angular acceleration provided by wander...
			result.angularAcceleration = wander.angularAcceleration;

			return result;

		}

	

		private SteeringOutput KeyboardMove () {
			// no kinematic state required

			float x = Input.GetAxis ("Horizontal");
			float z = Input.GetAxis ("Vertical");

			Vector3 direction = new Vector3 (x, 0, z);

			if (direction.magnitude < 0.05)
				return null;

			// accelerate along direction
			SteeringOutput result = new SteeringOutput ();
			result.linearAcceleration = maxAcceleration * direction.normalized;

			return result;
		}


		//------
		//--- ANGULAR STEERINGS ----
		//----


		private SteeringOutput Align (KinematicState own, KinematicState target) {
			// align is the angular version of arrive

			float requiredAngularSpeed;


			float requiredRotation = target.orientation - own.orientation; // how many degs do we have to rotate?
			if (requiredRotation < 0)
				requiredRotation = 360 + requiredRotation; // map to positive angles

			if (requiredRotation > 180)
				requiredRotation = -(360 - requiredRotation); // don't rotate more than 180 degs. just reverse rotation sense

			// when here, angular direction is in [-180, 180]

			float rotationSize = Mathf.Abs (requiredRotation); 

			if (rotationSize <= targetAngularRadius) // if we're "there", no steering needed.
				return null;

			if (rotationSize > slowDownAngularRadius)
				requiredAngularSpeed = maxAngularSpeed;
			else
				requiredAngularSpeed = maxAngularSpeed * (rotationSize/slowDownAngularRadius);

			// restablish sign
			requiredAngularSpeed = requiredAngularSpeed * Mathf.Sign (requiredRotation);

			// compute acceleration
			SteeringOutput result = new SteeringOutput();
			result.angularAcceleration = (requiredAngularSpeed - own.angularSpeed)/timeToDesiredAngularSpeed;
			// clip if necessary
			if (Mathf.Abs (result.angularAcceleration) > maxAngularAcceleration)
				result.angularAcceleration = maxAngularAcceleration * Mathf.Sign (result.angularAcceleration);

			return result;
		}

		private SteeringOutput Face (KinematicState own, KinematicState target) {
			// face = look at your target

			Vector3 directionToTarget = target.position - own.position;

			// if were're very close, do nothing...
			if (directionToTarget.magnitude < targetRadius) return null;

			// we want an orientation that makes us look at the same direction where the target is
			float desiredOrientation = VectorToOrientation (directionToTarget);

			KinematicState surrogateTarget = new KinematicState ();
			surrogateTarget.orientation = desiredOrientation;

			return Align (own, surrogateTarget);
		}


		private SteeringOutput KeyboardRotate () {
			// no kinematic state required

			float sense = Input.GetAxis ("Horizontal");

			if (Mathf.Abs (sense) < 0.05)
				return null;
			
			SteeringOutput result = new SteeringOutput ();
			result.angularAcceleration = -sense * maxAngularAcceleration;

			return result;
		}

	
		// -- Dispatcher 
		private SteeringOutput ComputeSteering () {
			KinematicState targetKS;
			Steerings2 steerings;

			// simple switch. Pure dispatcher
			switch (steeringType) {
			case SteeringType.ARRIVE: 
				if (target == null)
					return null;
				targetKS = new KinematicState ();
				targetKS.position = target.transform.position;
				return Arrive (ownKS, targetKS); 

			case SteeringType.ALIGN: 
				if (target == null)
					return null;
				targetKS = new KinematicState ();
				targetKS.orientation = target.transform.eulerAngles.z;
				return Align (ownKS, targetKS); 

			case SteeringType.FACE: 
				if (target == null)
					return null;
				targetKS = new KinematicState ();
				targetKS.position = target.transform.position;
				return Face (ownKS, targetKS); 

			case SteeringType.SEEK: 
				if (target == null)
					return null;
				targetKS = new KinematicState ();
				targetKS.position = target.transform.position;
				return Seek (ownKS, targetKS); 

			case SteeringType.FLEE: 
				if (target == null)
					return null;
				targetKS = new KinematicState ();
				targetKS.position = target.transform.position;
				return Flee (ownKS, targetKS); 

			case SteeringType.LINEAR_REPULSION:
				return LinearRepulsion (ownKS);

			case SteeringType.COLLISION_AVOIDANCE:
				return CollisionAvoidance (ownKS, "REPULSIVE");

			case SteeringType.OBSTACLE_AVOIDANCE:
				return ObstacleAvoidance2D (ownKS);

			case SteeringType.COHESION:
				return Cohesion (ownKS);

			case SteeringType.FLOCKING:
				return Flocking (ownKS);

			case SteeringType.CONFINED_FLOCKING:
				return ConfinedFlocking (ownKS);

			case SteeringType.SCARED_FISH:
				return ScaredFish (ownKS);

			case SteeringType.GO_WHERE_YOU_LOOK:
				return goWhereYouLook (ownKS); 
			
			case SteeringType.NAIVE_WANDER:
				return NaiveWander (ownKS); 

			case SteeringType.WANDER:
				return Wander (ownKS, wanderTarget); 

			case SteeringType.ROOM_WANDER:
				return RoomWander (ownKS);

			case SteeringType.KEYBOARD_ROTATE:
				return KeyboardRotate (); 

			case SteeringType.KEYBOARD_MOVE:
				return KeyboardMove(); 

			case SteeringType.PURSUE_INTERCEPT: 
				if (target == null)
					return null;
				// Beware: pursue needs to know the kinematic state of the target
				// (since it neeeds to know its linear velocity).
				steerings = target.GetComponent<Steerings2> ();
				if (steerings == null)
					return null;
				targetKS = steerings.ownKS;
				return Pursue (ownKS, targetKS); 

			case SteeringType.VELOCITY_MATCHING: 
				if (target == null)
					return null;
				// Beware: velocity needs to know the kinematic state of the target
				// (since it neeeds to know its linear velocity).
				steerings = target.GetComponent<Steerings2> ();
				if (steerings == null)
					return null;
				targetKS = steerings.ownKS;
				return VelocityMatching (ownKS, targetKS); 

			case SteeringType.NONE:
			default:
				return null;
			}
		}




		// -----------------------------
		// some utility methods
		// -----------------------------

		private Vector3 OrientationToVector (float alpha) {
			// alpha is an angle in degrees (anticlockwise) 
			 
			//alpha = 360 - alpha; // do this in clockwise systems

			// convert to radians
			alpha = alpha * Mathf.Deg2Rad;

			float cos = Mathf.Cos (alpha);
			float sin = Mathf.Sin (alpha);

			return new Vector3 (cos, sin, 0);
		}


		private float VectorToOrientation (Vector3 vector) {

			Vector3 direction = vector.normalized;

			float sin = direction.y;
			float cos = direction.x;

			float tan = sin / cos;

			float orientation = Mathf.Atan (tan)*Mathf.Rad2Deg;

			// remeber atan returns in the interval [-pi/2, pi/2] [-90, 90]
			// cosine determines region 

			if (cos < 0)
				orientation = orientation + 180;

			// orientation = 360 - orientation; // do this in clockwise systems

			return orientation;
		}


		private float binomial () {
			return Random.value - Random.value;
		}

		private float DotProduct (Vector3 a, Vector3 b) {
			return a.x * b.x + a.z * b.z;
		}

		private float determineRadius ()  {

			// returns non-constant value

			Renderer renderer  = GetComponent<Renderer> ();
			if (renderer != null) {
				return Mathf.Max (renderer.bounds.size.x, renderer.bounds.size.y);
			} else
				return 10;
			
		}
		
	}



	public enum SteeringType {
		NONE, SEEK, FLEE, ARRIVE, PURSUE_INTERCEPT, GO_WHERE_YOU_LOOK, NAIVE_WANDER, WANDER,
		ALIGN, FACE, VELOCITY_MATCHING, COLLISION_AVOIDANCE, OBSTACLE_AVOIDANCE, 
		LINEAR_REPULSION, COHESION, FLOCKING, ROOM_WANDER, CONFINED_FLOCKING,
		KEYBOARD_ROTATE, KEYBOARD_MOVE, SCARED_FISH
	};






}
 // close namespace