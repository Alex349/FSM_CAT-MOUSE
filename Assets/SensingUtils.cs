using UnityEngine;
using System.Collections;

public class SensingUtils
{

	public static GameObject FindInstanceWithinRadius (GameObject me, string tag, float radius) {

		GameObject [] targets = GameObject.FindGameObjectsWithTag(tag);
		if (targets.Length==0) return null;

		float dist = 0;
		GameObject closest = targets[0];
		float minDistance = (closest.transform.position - me.transform.position).magnitude;

		for (int i=1; i<targets.Length; i++) {
			dist = (targets[i].transform.position - me.transform.position).magnitude;
			if (dist<minDistance) {
				minDistance = dist;
				closest = targets[i];
			}
		}
		if (minDistance<radius) return closest;
		else return null;
	}

	public static float DistanceToTarget (GameObject me, GameObject target) {
		return (target.transform.position - me.transform.position).magnitude;
	}

}

