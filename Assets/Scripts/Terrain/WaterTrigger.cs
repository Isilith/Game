using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WaterTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider col) {
		if (col.tag == "TopTrigger") {
			PlayerPrefs.SetInt(PhotonNetwork.player.name+"_Diving", 1);
			//ChangeAnimation();
		}
		if (col.tag == "BottomTrigger") {
			PlayerPrefs.SetInt(PhotonNetwork.player.name+"_Swimming", 1);
		}

	}

	void OnTriggerExit(Collider col) {
		if (col.tag == "TopTrigger") {
			PlayerPrefs.SetInt(PhotonNetwork.player.name+"_Diving", 0);
			//ChangeAnimation();
		}
		if (col.tag == "BottomTrigger") {
			PlayerPrefs.SetInt(PhotonNetwork.player.name+"_Swimming", 0);
		}
	}

	void ChangeAnimation() {
		RenderSettings.fog = RenderSettings.fog ? false : true;
	}
}
