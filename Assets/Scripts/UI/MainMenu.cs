using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {



	void OnGUI() {
		if (GUI.Button(new Rect(Screen.width/2 - 75, Screen.height/3, 150, 50), "Start")) {
			SceneManager.LoadScene("Game1");
		}
		if (GUI.Button(new Rect(Screen.width/2 - 75, Screen.height/2, 150, 50), "Quit")) {
			Application.Quit();
		}
	}

}
