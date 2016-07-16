using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour {

	public float lifetime = .15f;

	private LineRenderer lr;
	private float counter;
	private float distance;
	private Vector3 dir;
	public float speed = 10f;

	public void Init(Vector3 start, Vector3 end) {
		lr = GetComponent<LineRenderer>();
		distance = Vector3.Distance(start,end);
		dir = Vector3.Normalize(end - start);
		counter = 0f;
	}

	public void Execute() {
		PhotonView pv = GetComponent<PhotonView>();
		if (pv != null && pv.instantiationId != 0) {
			PhotonNetwork.Destroy(gameObject);
		} else {
			Destroy(gameObject);
		}
	}
	// Update is called once per frame
	void FixedUpdate () {
		lifetime -= Time.deltaTime;
		if (lifetime <= 0) {
			print("lifetime 0");
			Execute();
		}
		if (lr != null) {
			if (counter < distance) {
				counter += dir.magnitude*0.1f;
				transform.position = Vector3.Lerp(transform.position, transform.position+dir, speed);
				lr.SetPosition(0, transform.forward*0.5f+transform.position);
				lr.SetPosition(1, transform.position - transform.forward*0.5f);
			}
		}
	}
}
