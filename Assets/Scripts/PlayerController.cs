using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	// This component is only enabled for "my player" (i.e. the character belonging to the local client machine).
	// This script is responsible for reading input commands from the player
	// and then passing that info to NetworkCharacter, which is responsible for
	// actually moving things.

	NetworkCharacter netChar;
	public bool swimming = false;
	public bool diving = false;
	[SerializeField] UnityStandardAssets.Characters.FirstPerson.MouseLook mouseLook;
	KeyCode[] skillKeys = new KeyCode[5];//1 ... 5 for skills


	// Use this for initialization
	void Start ()
	{
		netChar = GetComponent<NetworkCharacter> ();
		mouseLook.Init (transform, GetComponentInChildren<Camera> ().transform);
		PlayerPrefs.SetInt (PhotonNetwork.player.name + "_Swimming", 0);
		PlayerPrefs.SetInt (PhotonNetwork.player.name + "_Diving", 0);

		skillKeys [0] = KeyCode.Alpha1;
		skillKeys [1] = KeyCode.Alpha2;
		skillKeys [2] = KeyCode.Alpha3;
		skillKeys [3] = KeyCode.Alpha4;
		skillKeys [4] = KeyCode.Alpha5;
	}


	void Inputs() {
		if (Input.GetKey (KeyCode.LeftShift))
			netChar.isWalking = false;
		else
			netChar.isWalking = true;

		if (Input.GetButton ("Jump")) {
			netChar.isJumping = true;
		} else {
			netChar.isJumping = false;
		}


		for (int i = 0; i < skillKeys.Length; i++) {
			if (Input.GetKeyDown (skillKeys [i])) {
				netChar.UseSkill (i);
			}
		}
		if (Input.GetKeyDown (KeyCode.Mouse1)) {
			//Right mouse button for attacking
			netChar.FireWeapon (Camera.main.transform.position, Camera.main.transform.forward);
		}

	}

	
	// Update is called once per frame
	void Update ()
	{
		if (gameObject.transform.FindChild("Camera").gameObject.activeSelf) {
			CheckSwimState ();
			mouseLook.LookRotation (transform, GetComponentInChildren<Camera> ().transform);
			// WASD forward/back & left/right movement is stored in "direction"
			netChar.direction = transform.rotation * new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));

			// This ensures that we don't move faster going diagonally
			if (netChar.direction.magnitude > 1f) {
				netChar.direction = netChar.direction.normalized;
			}

			Inputs();

			AdjustAimAngle ();
		}
	}

	void AdjustAimAngle ()
	{
		Camera myCamera = this.GetComponentInChildren<Camera> ();

		if (myCamera == null) {
			Debug.LogError ("Why doesn't my character have a camera?  This is an FPS!");
			return;
		}

		float aimAngle = 0;

		if (myCamera.transform.rotation.eulerAngles.x <= 90f) {
			// We are looking DOWN
			aimAngle = -myCamera.transform.rotation.eulerAngles.x;
		} else {
			aimAngle = 360 - myCamera.transform.rotation.eulerAngles.x;
		}

		netChar.aimAngle = aimAngle;
	}

	void SetUnderWater ()
	{
		Physics.gravity = new Vector3 (0, 0f, 0);
	}

	void SetNormal ()
	{
		Physics.gravity = new Vector3 (0, -9.81f, 0);
	}



	void CheckSwimState ()
	{
		swimming = false;
		diving = false;

		if (PlayerPrefs.GetInt (PhotonNetwork.player.name + "_Diving") == 1) {
			diving = true;

		} else if (PlayerPrefs.GetInt (PhotonNetwork.player.name + "_Swimming") == 1) {
			swimming = true;

		}



		//if diving or swimming set gravity off
		//else set gravity on
		if (swimming || diving) {
			SetUnderWater ();
		} else {
			SetNormal ();
		}
	}

}
