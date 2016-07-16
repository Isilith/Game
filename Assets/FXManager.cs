using UnityEngine;
using System.Collections;

public class FXManager : MonoBehaviour {

	public GameObject bulletPrefab;

	[PunRPC]
	public void BulletFX(Vector3 start, Vector3 end) {
		GameObject bullet = (GameObject) Instantiate(bulletPrefab, start, Quaternion.LookRotation(start - end));
		bullet.GetComponent<AutoDestroy>().Init(start, end);
	}


	void FixedUpdate() {
		
	}
}
