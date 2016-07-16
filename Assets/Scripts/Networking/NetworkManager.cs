using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : Photon.MonoBehaviour {

	// TEMPORARY TESTING STUFF
	public string botResourceName;
	//public Waypoint botSpawnWaypoint;
	// END OF TESTING

	public Queue<GameObject> players = new Queue<GameObject>();


	public GameObject spawnpoint;
	public GameObject standbyCamera;
	public GameObject ui;
	public bool offlineMode = false;
	bool connecting = false;

	List<string> chatMessages;
	int maxChatMessages = 5;

	public float respawnTimer = 0;

	bool hasPickedTeam = false;
	int teamID=0;
	string playername = "";

	// Use this for initialization
	void Start () {
		PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "Awesome Dude");
		chatMessages = new List<string>();
	}

	void OnDestroy() {
		PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
	}

	public void AddChatMessage(string m) {
		GetComponent<PhotonView>().RPC ("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
	}

	[PunRPC]
	void AddChatMessage_RPC(string m) {
		while(chatMessages.Count >= maxChatMessages) {
			chatMessages.RemoveAt(0);
		}
		chatMessages.Add(m);
	}

	void Connect() {
		PhotonNetwork.ConnectUsingSettings( "MultiFPS v004" );
	}

	void OnGUI() {
		GUILayout.Label( PhotonNetwork.connectionStateDetailed.ToString() );

		if(PhotonNetwork.connected == false && connecting == false ) {


			// We have not yet connected, so ask the player for online vs offline mode.
			GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();

			GUILayout.Label("Username: ");

			playername = GUILayout.TextField(playername, 20, GUILayout.Width(150f), GUILayout.MinWidth(100f));
			PhotonNetwork.playerName = playername;

			GUILayout.Label("(max 20 characters)");

			GUILayout.EndHorizontal();

			if( GUILayout.Button("Single Player") ) {
				connecting = true;
				PhotonNetwork.offlineMode = true;
				OnJoinedLobby();
			}

			if( GUILayout.Button("Multi Player") ) {
				connecting = true;
				Connect ();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		if(PhotonNetwork.connected == true && connecting == false) {


			if(hasPickedTeam) {
				// We are fully connected, make sure to display the chat box.
				GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();

				foreach(string msg in chatMessages) {
					GUILayout.Label(msg);
				}

				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			else {
				// Player has not yet selected a team.
				//SpawnMyPlayer(0);
				GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				SpawnMyPlayer(0);

				/*if( GUILayout.Button("Red Team") ) {
					SpawnMyPlayer(1);
				}

				if( GUILayout.Button("Green Team") ) {
					SpawnMyPlayer(2);
				}

				if( GUILayout.Button("Random") ) {
					SpawnMyPlayer(Random.Range(1,3));	// 1 or 2
				}

				if( GUILayout.Button("Renegade!") ) {
					SpawnMyPlayer(0);
				}*/

				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.EndArea();


			}

		}

	}

	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom( null );
	}

	void OnJoinedRoom() {
		Debug.Log ("OnJoinedRoom");
		connecting = false;
	}

	void SpawnMyPlayer(int teamID) {

		this.teamID = teamID;
		hasPickedTeam = true;
		AddChatMessage("Spawning player: " + PhotonNetwork.player.name);

		GameObject myPlayerGO = (GameObject)PhotonNetwork.Instantiate("Player",spawnpoint.transform.position,Quaternion.identity,0);
		standbyCamera.gameObject.SetActive(false);
		myPlayerGO.GetComponent<ConstantForce>().enabled = true;
		myPlayerGO.GetComponent<PlayerController>().enabled = true;

		myPlayerGO.transform.FindChild("Camera").gameObject.SetActive(true);
		//myPlayerGO.GetComponent<NetworkCharacter>().skillPanel = ui.transform.Find("SkillPanel").gameObject;
		//myPlayerGO.GetComponent<NetworkCharacter>().skillPanel = (GameObject) GameObject.FindGameObjectWithTag("SkillPanel");
		ui.SetActive(true);
		myPlayerGO.GetComponent<NetworkCharacter>().ui = ui;
		// BOT TESTING
		//GameObject botGO = (GameObject)PhotonNetwork.Instantiate(botResourceName, botSpawnWaypoint.transform.position, botSpawnWaypoint.transform.rotation, 0);
		//((MonoBehaviour)botGO.GetComponent("BotController")).enabled = true;
		// END OF BOT TESTING
	}


	void Update() {
		if(respawnTimer > 0) {
			respawnTimer -= Time.deltaTime;

			if(respawnTimer <= 0) {
				players.Dequeue().GetComponent<PhotonView>().RPC("Respawn", PhotonTargets.All);
			}
		}
	}
}
