using UnityEngine;
using System.Collections;
using Steerings;

namespace FSM
{

	public class FSM_BATCAT_FEED: FiniteStateMachine
	{

		public enum State {INITIAL, WANDERING, REACHING_CAN, RUMMAGING, REACHING_HIDEOUT, EATING};

		public State currentState = State.INITIAL; 

		private BATCAT_Blackboard blackboard;

		private GameObject trashCan; // the trash can being approached or rummaged
		private GameObject sardine; // the sardine being transported or eaten
		private GameObject fishbone; // the fishbone that is thrown 
		private Arrive arrive; // steering
		private Wander wander; // steering
		private float elapsedTime; // time elapsed in EATING or RUMMAGING states

		void Start () {

			// get the steerings
			arrive = GetComponent<Arrive>();
			if (arrive == null)
				Debug.LogError (gameObject +" has no Arrive attached in "+this);

			wander = GetComponent<Wander>();
			if (wander == null)
				Debug.LogError (gameObject +" has no Wander attached in "+this);

			// get the blackboard
			blackboard = GetComponent<BATCAT_Blackboard>();
			if (blackboard == null) {
				blackboard = gameObject.AddComponent<BATCAT_Blackboard>();
			}
		}


		public override void Exit () {
			arrive.enabled = false;
			wander.enabled = false;
			base.Exit ();
		}

		public override void ReEnter() {
			currentState = State.INITIAL;
			base.ReEnter ();
		}

		void Update ()
		{
			switch (currentState) {
			case State.INITIAL:
				ChangeState (State.WANDERING);
				break;

			case State.EATING:
				if (elapsedTime >= blackboard.eatingTime) { // time to finish eating?
					ChangeState (State.WANDERING);
					break;
				}
				elapsedTime += Time.deltaTime;
				break;

			case State.REACHING_CAN:
				if (SensingUtils.DistanceToTarget (gameObject, trashCan) < blackboard.placeReachedRadius) { // trashcan reached?
					ChangeState(State.RUMMAGING);
					break;
				}
				// do nothing while in this state
				break;

			case State.REACHING_HIDEOUT:
				if (SensingUtils.DistanceToTarget (gameObject, blackboard.hideout) < blackboard.placeReachedRadius) { //hideout reached?
					ChangeState (State.EATING);
					break;
				}
				// do nothing while in this state
				break;

			case State.RUMMAGING:
				if (elapsedTime >= blackboard.rummageTime) {// food found? 
					ChangeState (State.REACHING_HIDEOUT);
					break;
				}
				elapsedTime += Time.deltaTime;
				break;

			case State.WANDERING:
				trashCan = SensingUtils.FindInstanceWithinRadius (gameObject, "TRASH_CAN", blackboard.trashcanDetectableRadius);
				if (trashCan != null) { // trash can detected? 
					ChangeState(State.REACHING_CAN);
					break;
				}
				// do nothing while in this state.
				break;

			} // end of switch
		}



		private void ChangeState (State newState) {
			
			// EXIT STATE LOGIC. Depends on current state
			switch (currentState) {

			case State.EATING:
				// after eating, hunger decreases
				blackboard.hunger -= blackboard.sardineHungerDecrement;
				// Destroy the sardine
				Destroy (sardine);
				// create the fishbone
				fishbone = Instantiate (blackboard.fishbonePrefab);
				fishbone.transform.position = gameObject.transform.position;
				fishbone.transform.rotation = Quaternion.Euler(0,0,180*Utils.binomial());
				break;

			case State.REACHING_CAN:
				arrive.enabled = false;
				break;

			case State.REACHING_HIDEOUT:
				arrive.enabled = false;
				break;

			case State.RUMMAGING:
				// when exiting rummaging create a sardine and "hold" it
				sardine = Instantiate (blackboard.sardinePrefab);
				sardine.transform.parent = gameObject.transform;
				sardine.transform.position = gameObject.transform.position;
				sardine.transform.localRotation = Quaternion.Euler(0,0,gameObject.transform.rotation.z+90);
				break;

			case State.WANDERING:
				wander.enabled = false;
				break;

			} // end exit switch

			// ENTER STATE LOGIC. Depends on newState
			switch (newState) {

			case State.EATING:
				elapsedTime = 0;
				break;

			case State.REACHING_CAN:
				arrive.target = trashCan;
				arrive.enabled = true;
				break;

			case State.REACHING_HIDEOUT:
				arrive.target = blackboard.hideout;
				arrive.enabled = true;
				break;

			case State.RUMMAGING:
				elapsedTime = 0;
				break;

			case State.WANDERING:
				wander.enabled = true;
				break;

			} // end of enter switch
			currentState = newState;

		} // end of method ChangeState

	}
}