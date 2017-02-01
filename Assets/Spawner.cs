using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public GameObject sample;

	public int numInstances = 10;
	public float interval = 5f; // one ant every interval seconds

	private int generated = 0;
	private float elapsedTime = 0f; // time elapsed since last generation



	// Update is called once per frame
	void Update () {
		if (generated == numInstances)
			return;

		GameObject clone;
		if (elapsedTime >= interval) {
			// spawn creating an instance...
			clone = Instantiate(sample);
			clone.transform.position = this.transform.position;
			generated++;
			elapsedTime = 0;
		} else {
			elapsedTime += Time.deltaTime;
		}

	}
}
