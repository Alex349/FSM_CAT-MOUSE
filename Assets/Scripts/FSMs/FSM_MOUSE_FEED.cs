using UnityEngine;
using System.Collections;
using Steerings;

namespace FSM
{

	public class FSM_MOUSE_FEED: FiniteStateMachine
	{

		public enum State {INITIAL, WANDERING, REACHING_CHEESE, EATING};

		public State currentState = State.INITIAL; 

		private MOUSE_Blackboard blackboard;
		public MICE_GLOBAL_Blackboard globalBlackboard;  // BEWARE instantiator should provide a value for this attribute

		private WanderPlusAvoid wander;
		private ArrivePlusAvoid arrive;

		private GameObject cheese;

		private float timeSinceLastBite;

		void Start () {
			wander = GetComponent<WanderPlusAvoid>();
			if (wander == null)
			Debug.LogError (gameObject +" has no WanderPlusAvois attached in "+this);

			arrive = GetComponent<ArrivePlusAvoid>();
			if (arrive == null)
				Debug.LogError (gameObject +" has no ArrivePlusAvoid attached in "+this);

			blackboard = GetComponent<MOUSE_Blackboard>();
			if (blackboard == null) {
				blackboard = gameObject.AddComponent<MOUSE_Blackboard>();
			}

			// the global blackboard it's better created by an external agent
			// or (if has to be created here) must come from a class implementing the SINGLETON PATTERN

			wander.enabled = false;
			arrive.enabled = false;
		}


		public override void Exit () {
			wander.enabled = false;
			arrive.enabled = false;
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
			case State.WANDERING:
				if (blackboard.hunger >= blackboard.hungerTooHigh) {
					cheese = SensingUtils.FindInstanceWithinRadius (gameObject, "CHEESE", blackboard.cheeseDetectableRadius);
					if (cheese != null) {
						// announce the presence of cheese
						globalBlackboard.AnnounceCheese(cheese);
						ChangeState(State.REACHING_CHEESE);
						break;
					}
					// there's no cheese around but maybe someone has announced some
					if (globalBlackboard.announcedCheese!=null) {
						cheese = globalBlackboard.announcedCheese;
						ChangeState (State.REACHING_CHEESE);
						break;
					}
				}

				// no transition has fired. Stay in the state. Hunger increases
				blackboard.hunger += blackboard.normalHungerIncrement*Time.deltaTime;
				break;

			case State.REACHING_CHEESE:
				// cheese is "higly volatile". It may vanish.
				if (cheese == null || cheese.Equals (null)) {
					// if it has vanished just forget about it and wander again
					ChangeState (State.WANDERING);
					break;
				}
				if (SensingUtils.DistanceToTarget (gameObject, cheese) < blackboard.cheeseReachedRadius) {
					ChangeState (State.EATING);
					break;
				}
				// no transition has fired. Stay in the state. Hunger increases
				blackboard.hunger += blackboard.normalHungerIncrement*Time.deltaTime;
				break;

			case State.EATING:
				// remember, cheese is highly volatile. it may "disappear"
				if (cheese == null || cheese.Equals (null)) {
					// if it has vanished just forget about it wander and wander again
					ChangeState (State.WANDERING);
					break;
				}
				if (blackboard.hunger <= blackboard.hungerLowEnough) { // am I satiated?
					ChangeState (State.WANDERING);
					break;
				}
				// there's cheese and I'm not still satiated...
				// mouse bites cheese at  blackboard.bitesPerSecond rate
				if (timeSinceLastBite >= 1 / blackboard.bitesPerSecond) {
					Bite (cheese);
					blackboard.hunger -= blackboard.cheeseHungerDecrement;
					timeSinceLastBite = 0;
				} else {
					timeSinceLastBite += Time.deltaTime;
				}
				break;

			} // end of switch
		}



		private void ChangeState (State newState) {
			// EXIT STATE LOGIC. Depends on current state
			switch (currentState) {
			case State.WANDERING:
				wander.enabled = false;
				break;
			case State.REACHING_CHEESE:
				arrive.enabled = false;
				break;
			case State.EATING:
				// do nothing in particular when leaving this state
				break;

			} // end of switch

			// ENTER STATE LOGIC. Depends on newState
			switch (newState) {
			case State.WANDERING:
				wander.enabled = true;
				break;

			case State.REACHING_CHEESE:
				// When reaching cheese is entered start moving towards the position of it
				arrive.target = cheese;
				arrive.enabled = true;
				break;

			case State.EATING:
				timeSinceLastBite = 1 / blackboard.bitesPerSecond;
				break;

			} // end of switch

			currentState = newState;
		}

		private void Bite (GameObject cheese) {
			cheese.SendMessage ("BeBitten");
		}

	}
}