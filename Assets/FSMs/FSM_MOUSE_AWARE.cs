using UnityEngine;
using System.Collections;
using Steerings;

namespace FSM
{

	public class FSM_MOUSE_AWARE : FiniteStateMachine
	{

		public enum State {INITIAL, NORMAL, SCARED};

		public State currentState = State.INITIAL; 

		private FleePlusAvoid flee;
		private MOUSE_Blackboard blackboard;
		private FSM_MOUSE_FEED fsmMouseFeed;
		private KinematicState KS;
	
		private float normalSpeed, normalAcc;

		private GameObject peril;

		void Start () {
			
			flee = GetComponent<FleePlusAvoid>();
			if (flee == null)
				Debug.LogError (gameObject +" has no FleePlusAvoid attached in "+this);

			KS = GetComponent<KinematicState>();
			if (KS== null)
				Debug.LogError (gameObject +" has no Kinematic state attached in "+this);
			normalSpeed = KS.maxSpeed;
			normalAcc = KS.maxAcceleration;


			fsmMouseFeed = GetComponent<FSM_MOUSE_FEED> ();
			if (fsmMouseFeed == null) {
				fsmMouseFeed = gameObject.AddComponent<FSM_MOUSE_FEED>();
			}

			blackboard = GetComponent<MOUSE_Blackboard>();
			if (blackboard == null) {
				blackboard = gameObject.AddComponent<MOUSE_Blackboard>();
			}

			flee.enabled = false;
			fsmMouseFeed.enabled = false;
		}

		public override void Exit () {
			fsmMouseFeed.enabled = false;
			flee.enabled = false;
			KS.maxSpeed = normalSpeed;
			KS.maxAcceleration = normalAcc;
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
				ChangeState (State.NORMAL);
				break;
			case State.NORMAL:
				peril = SensingUtils.FindInstanceWithinRadius (gameObject, "BATCAT", blackboard.perilDetectableRadius);
				if (peril != null) {
					ChangeState (State.SCARED);
					break;
				}
				// do nothing in particular while in this state
				break;
			case State.SCARED:
				if (SensingUtils.DistanceToTarget (gameObject, peril) >= blackboard.perilSafetyRadius) {
					ChangeState (State.NORMAL);
				}
				// do nothing in particular, while in this state
				break;
			}
		}

		private void ChangeState (State newState) {
			// EXIT STATE LOGIC. Depends on current state
			switch (currentState) {
			case State.NORMAL:
				fsmMouseFeed.Exit();
				break;
			case State.SCARED:
				// non-scared mice return to their normal speed and acc...
				KS.maxSpeed = normalSpeed;
				KS.maxAcceleration = normalAcc;
				flee.enabled = false;
				break;
			}

			// ENTER STATE LOGIC. Depends on newState
			switch (newState) {
			case State.NORMAL:
				fsmMouseFeed.ReEnter ();
				break;

			case State.SCARED:
				// scared mice may get an "extra boost" of speed. But not always...
				if (Random.value>0.6f) {
					float boostFactor = 1.3f + Random.value;
					KS.maxSpeed *= boostFactor;
					KS.maxAcceleration *= boostFactor;
				}
				flee.target = peril;
				flee.enabled = true;
				break;
			}

			currentState = newState;
		}

	}
}