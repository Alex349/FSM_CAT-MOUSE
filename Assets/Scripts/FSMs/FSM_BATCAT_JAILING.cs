﻿using UnityEngine;
using System.Collections;
using Steerings;

namespace FSM
{

    public class FSM_BATCAT_JAILING : FiniteStateMachine
    {

        public enum State {
            INITIAL,
            HIDING,
            // a mouse has been detected, the cat is pursuing it
            PURSUING,
            // the cat hunted a mouse, and is dragging it to the jail
            TRANSPORTING,
            // the cat dragged a mouse to its jail, and is now returning
            // to its hiding spot
            RETURNING,
            RESTING
        };

        public State currentState = State.INITIAL;

        private BATCAT_Blackboard blackboard;

        private GameObject mouse; // the mouse being pursued or transported
        private Arrive arrive; // steering
        private Pursue pursue; // steering
        private float pursuingTime; // time elapsed since pursuing behaviour started
        private float restingTime; // time elapsed since resting time started

        void Start()
        {
            // get the steerings that are going to be used by this FSM
            arrive = GetComponent<Arrive>();
            if (arrive == null)
                Debug.LogError(gameObject + " has no Arrive attached in " + this);

            pursue = GetComponent<Pursue>();
            if (pursue == null)
                Debug.LogError(gameObject + " has no Pursue attached in " + this);


            // get the internal state + world representation (the "BLACKBOARD")
            blackboard = GetComponent<BATCAT_Blackboard>();
            if (blackboard == null)
            {
                blackboard = gameObject.AddComponent<BATCAT_Blackboard>();
            }

            arrive.enabled = false;
            pursue.enabled = false;
        }


        public override void Exit()
        {
            // stop any steering that may be enabled
            arrive.enabled = false;
            pursue.enabled = false;
            base.Exit();
        }

        public override void ReEnter()
        {
            currentState = State.INITIAL;
            base.ReEnter();
        }

        void Update()
        {
            // do this no matter the state
            UpdateHunger();

            switch (currentState)
            {
                case State.INITIAL:

                    ChangeState(State.RETURNING);
                    break;

                case State.HIDING:

                    mouse = SensingUtils.FindInstanceWithinRadius(gameObject, "MOUSE", blackboard.mouseDetectableRadius);
                    if (mouse != null)
                    {
                        // mouse detected 
                        ChangeState(State.PURSUING);
                        break;
                    }
                    break;

                case State.PURSUING:

                    if (SensingUtils.DistanceToTarget(gameObject, mouse) <= blackboard.mouseReachedRadius)
                    {
                        // mouse reached
                        // stop the mouse: disable all its steering behaviours

                        // FIXME the teacher is an imbecile
                        
                        // SteeringBehaviour[] sts = mouse.GetComponents<SteeringBehaviour> ();
                        // foreach (SteeringBehaviour st in sts)
                        // st.enabled = false;
                        
                        // take the mouse and begin transportation
                        mouse.transform.parent = gameObject.transform;
                        mouse.tag = "TRAPPED_MOUSE";
                        // start transporting the mouse
                        ChangeState(State.TRANSPORTING);
                        break;
                    }

                    // no mouse reached

                    // FIXME
                    // does this method return the closest instance with the tag 'MOUSE'
                    // to the current game object or are we playing with fire here?

                    GameObject otherMouse = SensingUtils.FindInstanceWithinRadius(gameObject, "MOUSE", blackboard.mouseDetectableRadius);

                    if (otherMouse != null && otherMouse != mouse &&
                        SensingUtils.DistanceToTarget(gameObject, otherMouse) < SensingUtils.DistanceToTarget(gameObject, mouse))
                    {
                        // if there's another mouse that is closer to me than the one I'm pursuing, "retarget"
                        mouse = otherMouse;
                        break;
                    }

                    if (SensingUtils.DistanceToTarget(gameObject, mouse) >= blackboard.mouseHasVanishedRadius)
                    { 
                        // mouse vanished
                        ChangeState(State.RETURNING);
                        break;
                    }

                    if (pursuingTime >= blackboard.maxPursuingTime)
                    { 
                        // time limit for pursuing exceeded
                        ChangeState(State.RESTING);
                        break;
                    }

                    // increment current pursuing time
                    pursuingTime += Time.deltaTime;
                    break;

                case State.TRANSPORTING:

                    if (SensingUtils.DistanceToTarget(gameObject, blackboard.jail) < blackboard.placeReachedRadius)
                    {
                        // mice-jail reached
                        // drop the mouse
                        mouse.transform.parent = null;
                        ChangeState(State.RETURNING);
                        break;
                    }
                    break;

                case State.RETURNING:

                    mouse = SensingUtils.FindInstanceWithinRadius(gameObject, "MOUSE", blackboard.mouseDetectableRadius);
                    if (mouse != null)
                    {
                        // mouse detected 
                        ChangeState(State.PURSUING);
                        break;
                    }

                    if (SensingUtils.DistanceToTarget(gameObject, blackboard.hideout) < blackboard.placeReachedRadius)
                    { 
                        // hideout reached
                        ChangeState(State.HIDING);
                        break;
                    }
                    break;

                case State.RESTING:
                    if (restingTime >= blackboard.maxRestingTime)
                    {
                        // the cat is well rested
                        ChangeState(State.RETURNING);
                        break;
                    }

                    restingTime += Time.deltaTime;
                    break;
            // end of switch
            }
        // end of Update
        }



        private void ChangeState(State newState)
        {
            // EXIT STATE LOGIC. Depends on current state
            switch (currentState)
            {
                case State.HIDING:
                    // do nothing when leaving resting
                    break;
                case State.RETURNING:
                    // when leaving RETURNIG, turn off the ARRIVE steering
                    arrive.enabled = false;
                    arrive.target = null;
                    break;
                case State.PURSUING:
                    // when leaving PURSUING turn off the PURSUE steering 
                    pursue.enabled = false;
                    pursue.target = null;
                    break;
                case State.TRANSPORTING:
                    // when leaving TRANSPORTING turn off the ARRIVE steering
                    arrive.enabled = false;
                    arrive.target = null;
                    break;
                case State.RESTING:
                    break;
            // end of EXIT STATE LOGIC
            }

            // ENTER STATE LOGIC. Depends on newState
            switch (newState)
            {
                case State.HIDING:
                    // do nothing when entering HIDING
                    break;
                case State.RETURNING:
                    // when entering RETURNING activate the ARRIVE steering in order to go to hideout
                    arrive.target = blackboard.hideout;
                    arrive.enabled = true;
                    break;
                case State.PURSUING:
                    // when entering PURSUING activate the PURSUE behaviour. Use mouse as the target
                    // also initialize pursuingTime
                    pursue.target = mouse;
                    pursue.enabled = true;
                    pursuingTime = 0f;
                    break;
                case State.TRANSPORTING:
                    // when entering TRANSPORTING activate the ARRIVE behaviour in order to go to the jail building
                    arrive.target = blackboard.jail;
                    arrive.enabled = true;
                    break;

                case State.RESTING:
                    // when entering RESTING initialize restingTime
                    restingTime = 0;
                    break;
            // end of ENTER STATE LOGIC
            }
            currentState = newState;
        }

        private void UpdateHunger()
        {
            switch (currentState)
            {
                case State.PURSUING:
                case State.TRANSPORTING:
                    blackboard.hunger += 2f * blackboard.normalHungerIncrement * Time.deltaTime;
                    break;
                case State.RESTING:
                    blackboard.hunger += 0.5f * blackboard.normalHungerIncrement * Time.deltaTime;
                    break;
                default:
                    blackboard.hunger += blackboard.normalHungerIncrement * Time.deltaTime;
                    break;
            }
        }

    }
}