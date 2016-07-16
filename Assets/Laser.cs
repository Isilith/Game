using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {
	void OnTriggerEnter(Collider col) {
		print("Hit");
		GetComponentInParent<AutoDestroy>().Execute();
	}


}
