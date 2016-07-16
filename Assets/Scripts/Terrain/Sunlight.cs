using UnityEngine;
using System.Collections;

public class Sunlight : MonoBehaviour {

	public float speed = 5f;

	void FixedUpdate () {
		float rot = speed*Time.fixedDeltaTime;
		transform.Rotate(new Vector3(rot,0f,0f));
	}
}
