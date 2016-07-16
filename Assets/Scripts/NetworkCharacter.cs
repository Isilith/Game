using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class NetworkCharacter : Photon.MonoBehaviour {

	// This script is responsible for actually moving a character.
	// For local character, we read things like "direction" and "isJumping"
	// and then affect the character controller.
	// For remote characters, we skip that and simply update the raw transform
	// position based on info we received over the network.


	// NOTE! Only our local character will effectively use this.
	// Remove character will just give us absolute positions.
	public float runSpeed = 10f;		// The speed at which I run
	public float walkSpeed = 5f;
	public float jumpSpeed = 10f;	// How much power we put into our jump. Change this to jump higher.
	public float stickToGroundForce = 10f;

	Skills skills = new Skills();

	// Bookeeping variables
	[System.NonSerialized]
	public Vector3 direction = Vector3.zero;	// forward/back & left/right
	[System.NonSerialized]
	public bool isJumping = false;
	[System.NonSerialized]
	public float aimAngle = 0;
	[System.NonSerialized]
	public bool isWalking = false;

	float   verticalVelocity = 0;		// up/down

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;
	float realAimAngle = 0;

	Animator anim;

	bool gotFirstUpdate = false;

	CharacterController cc;

	float cooldown = 0;

	private TerrainData terrainData;
	private Texture2D tex;
	private Texture2D tex2;

	Rect minimapRect;
	Rect minimapPlayer;
	float mapWidth;
	float mapHeight;
	float minimapWidth = 200f;

	public FXManager fxManager;
	public GameObject skillPanel;
	public GameObject ui;


	void Start() {
		CacheComponents();
		PlayerPrefs.SetInt(PhotonNetwork.player.name+"_Swimming", 0);
		PlayerPrefs.SetInt(PhotonNetwork.player.name+"_Diving", 0);

		terrainData = GameObject.Find("Terrain").GetComponent<Terrain>().terrainData;
		fxManager = GameObject.FindObjectOfType<FXManager>();

		mapWidth = terrainData.heightmapWidth;
		mapHeight = terrainData.heightmapHeight;



		minimapRect = new Rect(Screen.width-minimapWidth, 0f, minimapWidth, minimapWidth);//Screen.width-minimapWidth
		minimapPlayer = new Rect(minimapRect.x, minimapRect.y, 8f, 12f);
		Minimap();
		skills.Add("Blink", 5f, 10f);
		skills.Add("Heal", 5f, 25f);
		GetComponent<Health>().Init(100f, 100f);
	}

	public void UseSkill(int i) {
		//skills.Use(this, i);
	}
		
	void OnGUI() {
		if (tex != null) {
			if (photonView.isMine) {
				GUI.DrawTexture(minimapRect, tex);

				float w = transform.position.x / mapWidth * minimapWidth + minimapRect.x;
				float h = (mapHeight - transform.position.z) / mapHeight * minimapWidth;

				minimapPlayer.x = w;
				minimapPlayer.y = h;

				Matrix4x4 svMat = GUI.matrix; // save the current GUI matrix
				// find the pivot and rotation angle:
				Vector2 pivot = minimapPlayer.center; // get the arrow center point
				float ang = transform.eulerAngles.y; // get the character angle
				// set GUI matrix to rotate ang degrees around the pivot:
				GUIUtility.RotateAroundPivot(ang, pivot);
				GUI.DrawTexture(minimapPlayer, tex2); // draw the arrow
				GUI.matrix = svMat; // restore the matrix to not affect other GUI items
			}
		}
	}

	void Minimap() {
		tex = new Texture2D(terrainData.alphamapWidth,terrainData.alphamapHeight);
		tex2 = Resources.Load("arrow") as Texture2D;
		Color[] colors = new Color[tex.width*tex.height];
		float[,,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
		int ind = 0;
		Color[] values = new Color[3];
		values[0] = new Color(70f/255f,180f/255f,70f/255f);
		values[1] = new Color(0.5f,0.5f,0.5f);
		values[2] = new Color(105f/255f,75f/255f,255f/255f);
		for (int i=0; i<tex.height-1; i++) {
			for (int j=0; j<tex.width; j++) {
				float height = terrainData.GetHeight(i,j)/terrainData.size.y;
				for (int c=0; c<3; c++) {
					float a = alphas[i,j,c];
					colors[ind] += a * values[c] * height;
					colors[ind].a = 1.0f;
				}
				ind++;
			}
		}
		tex2.Apply();
		tex.SetPixels(colors);
		tex.Apply();
	}

	/*[PunRPC]
	void Respawn() {
		transform.position = GameObject.FindObjectOfType<NetworkManager>().spawnpoint.transform.position;
		gameObject.SetActive(true);
	}*/

	void CacheComponents() {
		anim = GetComponent<Animator>();
		cc = GetComponent<CharacterController>();
		//skillPanel = (GameObject) GameObject.FindGameObjectWithTag("SkillPanel");

	}

	void Update() {
		cooldown -= Time.deltaTime;
		/*skills.UpdateSkillCooldowns(Time.deltaTime);
		for (int i=0; i<skills.GetSkillCount(); i++) {
			double cd = System.Math.Round(skills.GetSkill(i).GetCooldown(), 1);
			string t = cd > 0 ? ""+cd : "";
			skillPanel.transform.Find("Skill"+(i+1)).Find("Cooldown").GetComponent<Text>().text = t;
		}*/
	}
	
	// FixedUpdate is called once per physics loop
	// Do all MOVEMENT and other physics stuff here.
	void FixedUpdate () {
		if( photonView.isMine ) {
			// Do nothing -- the character motor/input/etc... is moving us

			DoLocalMovement();
		}
		else {
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
			anim.SetFloat("AimAngle", Mathf.Lerp(anim.GetFloat("AimAngle"), realAimAngle, 0.1f ) );
		}
	}

	void DoLocalMovement () {
		float speed = isWalking ? walkSpeed : runSpeed;
		// "direction" is the desired movement direction, based on our player's input
		Vector3 dist = direction * speed * Time.deltaTime;

		anim.SetFloat("Speed", dist.magnitude);

		if(isJumping) {
			isJumping = false;
			if(cc.isGrounded) {
				verticalVelocity = jumpSpeed;

			}
		}

		if(cc.isGrounded && verticalVelocity < 0) {
			
			// Ensure that we aren't playing the jumping animation
			anim.SetBool("Jumping", false);
			
			// Set our vertical velocity to *almost* zero. This ensures that:
			//   a) We don't start falling at warp speed if we fall off a cliff (by being close to zero)
			//   b) cc.isGrounded returns true every frame (by still being slightly negative, as opposed to zero)
			verticalVelocity = -stickToGroundForce;
		}
		else {
			// We are either not grounded, or we have a positive verticalVelocity (i.e. we ARE starting a jump)
			
			// To make sure we don't go into the jump animation while walking down a slope, make sure that
			// verticalVelocity is above some arbitrary threshold before triggering the animation.
			// 75% of "jumpSpeed" seems like a good safe number, but could be a standalone public variable too.
			//
			// Another option would be to do a raycast down and start the jump/fall animation whenever we were
			// more than ___ distance above the ground.

			/*if(Mathf.Abs(verticalVelocity) > jumpSpeed*0.5f) {
				anim.SetBool("Jumping", true);
			}*/
			
			// Apply gravity.
			verticalVelocity += Physics.gravity.y * 2f * Time.deltaTime;
		}

		//print(anim.GetBool("Jumping"));

		// Add our verticalVelocity to our actual movement for this frame
		dist.y = verticalVelocity * Time.deltaTime;

		if (PlayerPrefs.GetInt(PhotonNetwork.player.name+"_Swimming") == 1) {
			float mult = -Mathf.Sin(transform.GetComponentInChildren<Camera>().transform.eulerAngles.x*Mathf.Deg2Rad);
			Vector3 force = new Vector3(0f, mult*10f, 0f);
			if (PlayerPrefs.GetInt(PhotonNetwork.player.name+"_Diving") == 0) {
				force.y = Mathf.Min(force.y, 0);
			}
			GetComponent<ConstantForce>().relativeForce = force;
			dist.y = GetComponent<ConstantForce>().relativeForce.y * Time.fixedDeltaTime;
		}

		cc.Move( dist );
	}
		
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		CacheComponents();

		if(stream.isWriting) {
			// This is OUR player. We need to send our actual position to the network.

			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.GetFloat("Speed"));
			stream.SendNext(anim.GetBool("Jumping"));
			stream.SendNext(anim.GetFloat("AimAngle"));
		}
		else {
			// This is someone else's player. We need to receive their position (as of a few
			// millisecond ago, and update our version of that player.

			// Right now, "realPosition" holds the other person's position at the LAST frame.
			// Instead of simply updating "realPosition" and continuing to lerp,
			// we MAY want to set our transform.position to immediately to this old "realPosition"
			// and then update realPosition
			realPosition = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
			anim.SetFloat("Speed", (float)stream.ReceiveNext());
			anim.SetBool("Jumping", (bool)stream.ReceiveNext());
			realAimAngle = (float)stream.ReceiveNext();

			if(gotFirstUpdate == false) {
				transform.position = realPosition;
				transform.rotation = realRotation;
				anim.SetFloat("AimAngle", realAimAngle );
				gotFirstUpdate = true;
			}

		}
	}


	public void FireWeapon(Vector3 orig, Vector3 dir) {
		if(cooldown > 0) {
			return;
		}

		//Debug.Log ("Firing our gun!");

		Ray ray = new Ray(orig, dir);
		Transform hitTransform;
		Vector3   hitPoint;

		hitTransform = FindClosestHitObject(ray, out hitPoint);

		if(hitTransform != null) {
			Debug.Log ("We hit: " + hitTransform.name);

			// We could do a special effect at the hit location
			// DoRicochetEffectAt( hitPoint );
			if (hitTransform.tag == "Player") {
				Health h = hitTransform.GetComponent<Health>();

				while(h == null && hitTransform.parent) {
					hitTransform = hitTransform.parent;
					h = hitTransform.GetComponent<Health>();
				}
				//Now h contains the Health component of our target

				h.GetComponent<PhotonView>().RPC ("ChangeHealth", PhotonTargets.AllBuffered, -50f);
			} else {
				//Debug.Log("Learn to aim noob! You hit a fucking " + hitTransform.name);
			}
			DoGunFX(hitPoint);
		}

		cooldown = .25f;
	}


	void DoGunFX(Vector3 hit) {
		Transform cam = transform.Find("Camera");
		Vector3 hand = cam.forward*0.5f - cam.up*0.3f;


		Vector3 start = cam.position + hand;

		fxManager.GetComponent<PhotonView>().RPC("BulletFX", PhotonTargets.All, start, hit);
	}


	public Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint) {

		RaycastHit[] hits = Physics.RaycastAll(ray);

		Transform closestHit = null;
		float distance = 0;
		hitPoint = Vector3.zero;
		foreach(RaycastHit hit in hits) {
			
			if (hit.distance > 200f)
				continue;
			if(hit.transform != this.transform && ( closestHit==null || hit.distance < distance ) ) {
				// We have hit something that is:
				// a) not us
				// b) the first thing we hit (that is not us)
				// c) or, if not b, is at least closer than the previous closest thing

				closestHit = hit.transform;
				distance = hit.distance;
				hitPoint = hit.point;
			}

		}
		// closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

		return closestHit;

	}


}
