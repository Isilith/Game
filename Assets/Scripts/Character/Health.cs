using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Health : MonoBehaviour {

	[SerializeField] private float maxHealth;
	[SerializeField] private float currentHealth;
	[SerializeField] private float prevHealth;

	[SerializeField] private float maxEnergy;
	[SerializeField] private float currentEnergy;
	[SerializeField] private float prevEnergy;


	const string display1 = "{0} / {1}";
	private Text health;
	const string display2 = "{0} / {1}";
	private Text energy;

	private bool initialized = false;

	public void Init(float startHealth, float startEnergy) {
		maxHealth = startHealth;
		currentHealth = startHealth;
		maxEnergy = startEnergy;
		currentEnergy = startEnergy;
		health = GameObject.Find("Health").GetComponent<Text>();
		health.text = string.Format(display1, (int)currentHealth, (int)maxHealth);
		energy = GameObject.Find("Energy").GetComponent<Text>();
		energy.text = string.Format(display2, (int)currentHealth, (int)maxHealth);
		initialized = true;
	}


	private void Update()
	{
		
		if (initialized) {

			currentEnergy += Time.deltaTime*2f;
			currentEnergy = currentEnergy < maxEnergy ? currentEnergy : maxEnergy;
			if (GetComponent<PhotonView>() != null) {
				if (GetComponent<PhotonView>().isMine) {
					health.text = string.Format(display1, (int)currentHealth, (int)maxHealth);
					energy.text = string.Format(display2, (int)currentEnergy, (int)maxEnergy);
				}
			}
		}
	}

	public float GetEnergy() {
		return currentEnergy;
	}

	public void UseEnergy(float value) {
		currentEnergy -= value;
	}

	[PunRPC]
	public void ChangeHealth(float value) {
		//negative value when taking damage
		//positive value when being healed
		currentHealth += value;

		if (currentHealth > maxHealth)
			currentHealth = maxHealth;
		else if (currentHealth <= 0)
			Die();
		
		if (GetComponent<PhotonView>() != null) {
			if (GetComponent<PhotonView>().isMine) {
				health.text = string.Format(display1, (int)currentHealth, (int)maxHealth);
			}
		}
	}

	void OnGUI() {
		
		if( GUI.Button(new Rect (0, 100, 100, 40), "Suicide!") ) {
			if( GetComponent<PhotonView>().isMine && gameObject.tag == "Player" ) {
				Die ();
			}
		}
	}

	[PunRPC]
	void Respawn() {
		if (GetComponent<PhotonView>().isMine ) {
			if( gameObject.tag == "Player" ) {		// This is my actual PLAYER object, then initiate the respawn process
				NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();

				transform.position = nm.spawnpoint.transform.position;
				nm.standbyCamera.SetActive(false);
				gameObject.transform.FindChild("Camera").gameObject.SetActive(true);
				//nm.respawnTimer = 3f;
			}
		}
		//gameObject.SetActive(true);
	}
		

	void Die() {
		//If gameObject wasn't instantiated by the network
		if (GetComponent<PhotonView>().instantiationId == 0) {
			Destroy(gameObject);
		} else {
			if (GetComponent<PhotonView>().isMine ) {
				if( gameObject.tag == "Player" ) {		// This is my actual PLAYER object, then initiate the respawn process
					NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();

					nm.standbyCamera.SetActive(true);
					nm.respawnTimer = 3f;
					nm.players.Enqueue(gameObject);
					gameObject.transform.FindChild("Camera").gameObject.SetActive(false);
				}
			}
			transform.position = new Vector3(0,0,0);
			//gameObject.SetActive(false);
		}
	}

}
