using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class MenuWindowScript : MonoBehaviour {

	public bool menuOpen = false;
	public bool invOpen = false;
	int w = 200;
	int h = 300;
	public Rect windowRect = new Rect();
	public Rect invRect = new Rect();
	public GameObject player;

	void Start() {
		
	}

	void Update() {
		if (Input.GetKeyUp(KeyCode.Escape)) {
			menuOpen = menuOpen ? false : true;
		}
		if (Input.GetKeyUp(KeyCode.I)) {
			invOpen = invOpen ? false : true;
		}
	}

	void OnGUI() {
		CheckMenuStatus();
		CheckInvStatus();
	}

	void CheckMenuStatus() {
		if (menuOpen) 
			windowRect = GUI.Window(0, new Rect(Screen.width/2 - w/2, Screen.height/2 - h/2, w, h), OpenMenuWindow, "Menu");
		if (!menuOpen) {
			//Time.timeScale = 1.0f;
			//s.mouseLook.SetCursorLock(true);
		}
	}

	void CheckInvStatus() {
		if (invOpen) 
			invRect = GUI.Window(0, new Rect(Screen.width/2 - w/2, Screen.height/2 - h/2, w, h), OpenInv, "Inventory");
		if (!invOpen) {
			Time.timeScale = 1.0f;
			//s.mouseLook.SetCursorLock(true);
		}
	}

	void OpenMenuWindow(int windowID) {
		//menuOpen = true;
		//Time.timeScale = 0.0f;
		//s.mouseLook.SetCursorLock(false);
		/*if (GUI.Button(new Rect(w/4, 20, w/2, 20), "Reset")) {
		}*/
		if (GUI.Button(new Rect(w/4, 50, w/2, 20), "Exit")) {
			Application.Quit();
		}
	}

	void OpenInv(int windowID) {
		//Time.timeScale = 0.0f;
		//s.mouseLook.SetCursorLock(false);

	}
}
