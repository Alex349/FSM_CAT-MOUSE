using UnityEngine;
using System.Collections;
using FSM;

public class MiceSpawner : MonoBehaviour {

	private GameObject sample;
	private MICE_GLOBAL_Blackboard globalBlackboard;

	public int numInstances = 10;
	public float interval = 5f; // one ant every interval seconds

	private int generated = 0;
	private float elapsedTime = 0f; // time elapsed since last generation

	void Start () {
		sample =  Resources.Load<GameObject> ("MOUSE");
		if (sample == null)
			Debug.LogError ("No MOUSE prefab found as a resource");

		globalBlackboard = GetComponent<MICE_GLOBAL_Blackboard>();
		if (globalBlackboard == null)
			globalBlackboard = gameObject.AddComponent<MICE_GLOBAL_Blackboard> ();

	}

	// Update is called once per frame
	void Update () {
		if (generated == numInstances)
			return;

		GameObject clone;
		if (elapsedTime >= interval) {
			// spawn creating an instance...
			clone = Instantiate(sample);
			clone.transform.position = this.transform.position;

			// give the global blackboard to the clone
			FSM_MOUSE_FEED fsm = clone.GetComponent<FSM_MOUSE_FEED>();
			if (fsm != null) {
				fsm.globalBlackboard = globalBlackboard;
			} else {
				Debug.Log ("spawned mouse has no FSM_MOUSE_FEED");
			}

			generated++;
			elapsedTime = 0;
		} else {
			elapsedTime += Time.deltaTime;
		}

	}
}
