using UnityEngine;
using System.Collections;
using Steerings;

namespace FSM
{

	public class FSM_MOUSE_FINAL: FiniteStateMachine
	{

		public enum State {INITIAL, NORMAL, TRAPPED};

		public State currentState = State.INITIAL; 
	
		private MOUSE_Blackboard blackboard;
		private FSM_MOUSE_AWARE fsmMouseAware;

		void Start () {
			
			blackboard = GetComponent<MOUSE_Blackboard>();
			if (blackboard == null) {
				blackboard = gameObject.AddComponent<MOUSE_Blackboard>();
			}

			fsmMouseAware = GetComponent<FSM_MOUSE_AWARE> ();
			if (fsmMouseAware == null) {
				fsmMouseAware = gameObject.AddComponent<FSM_MOUSE_AWARE>();
			}

			fsmMouseAware.enabled = false;
		}


		public override void Exit () {
			fsmMouseAware.enabled = false;
			base.Exit ();
		}

		public override void ReEnter() {
			base.ReEnter ();
		}

		void Update ()
		{
			switch (currentState) {
			case State.INITIAL:
				ChangeState (State.NORMAL);
				break;
			case State.NORMAL:
				if (tag == "TRAPPED_MOUSE") {
					ChangeState (State.TRAPPED);
					break;
				}
				// do nothing while in this state
				break;
			case State.TRAPPED:
				if (tag == "MOUSE") {
					ChangeState (State.NORMAL);
				}
				// do nothing while in this state
				break;
			} // end of switch
		}



		private void ChangeState (State newState) {
			// EXIT STATE LOGIC. Depends on current state
			switch (currentState) {
			case State.NORMAL:
				fsmMouseAware.Exit ();
				break;
			case State.TRAPPED:
				// do nothing when leaving this state
				break;
			} // end of EXIT SWITCH

			// ENTER STATE LOGIC. Depends on newState
			switch (newState) {
			case State.NORMAL:
				fsmMouseAware.ReEnter ();
				break;
			case State.TRAPPED:
				// do nothing when entering this state
				break;
			} // end of ENTER switch

			currentState = newState;
		}

	}
}