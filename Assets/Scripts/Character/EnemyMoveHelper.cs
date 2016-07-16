using UnityEngine;
using System.Collections;


/*
 * NavMeshAgent has a lot of useful stuff to try
 * Or make your own obstacle avoidance system
 */

public class EnemyMoveHelper : MonoBehaviour {

	public GameObject target;
	public float distanceModifier = 5f;
	NavMeshAgent agent;
	public float maxSpeed = 3.5f; //seems to be default
	//public AudioClip spawnClip;
	//public AudioClip attackClip;

	//AudioSource spawnSource;
	//AudioSource attackSource;

	void Awake() {
		//spawnSource = AddAudio(attackClip, false, true, 0.4f);
		//attackSource = AddAudio(attackClip, false, false, 0.4f);


	}

	AudioSource AddAudio(AudioClip clip, bool loop, bool playOnAwake, float vol) {
		AudioSource ret = gameObject.AddComponent<AudioSource>();
		ret.clip = clip;
		ret.loop = loop;
		ret.playOnAwake = playOnAwake;
		ret.volume = vol;
		return ret;
	}

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		agent.stoppingDistance = 5f;
		//spawnSource.Play();

	}

	void FixedUpdate () {
		agent.destination = target.transform.position;
	}

}
