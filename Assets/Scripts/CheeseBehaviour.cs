using UnityEngine;
using System.Collections;

public class CheeseBehaviour : MonoBehaviour {

	public int numberOfBites = 100;

	public void BeBitten () {
		numberOfBites--;
		if (numberOfBites <= 0)
			Destroy (gameObject);
	}
}
