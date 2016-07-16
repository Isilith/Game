using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public Camera camera;
	[SerializeField] public UnityStandardAssets.Characters.FirstPerson.MouseLook mouseLook;
	[SerializeField] private bool jump;
	[SerializeField] private bool jumping;
	[SerializeField] private bool isWalking;
	[SerializeField] private bool previouslyGrounded;
	[SerializeField] private Vector2 input;
	[SerializeField] private Vector3 moveDir = Vector3.zero;
	[SerializeField] private float walkSpeed = 5f;
	[SerializeField] private float runSpeed = 10f;
	[SerializeField] private float stickToGroundForce = 10f;
	[SerializeField] private float jumpSpeed = 10f;
	[SerializeField] private float gravityMultiplier = 2f;
	private Animator animator;
	private CharacterController characterController;

	// Use this for initialization
	void Start () {
		Camera[] cameras = Camera.allCameras;
		camera = Camera.main;
		for (int i=0; i<cameras.Length; i++) {
			if (cameras[i].transform.parent.gameObject == gameObject) {
				camera = cameras[i];
			}
		}
		jumping = false;
		jump = false;
		isWalking = false;
		mouseLook.Init(transform , camera.transform);
		characterController = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
		RotateView();

		//if jump is possible
		if (!jump && !jumping) {
			jump = Input.GetButtonDown("Jump");
		}

		//check if it is ok to jump again
		if (!previouslyGrounded && characterController.isGrounded) {
			jumping = false;
		}

		//save the last ground state
		previouslyGrounded = characterController.isGrounded;
	}

	void FixedUpdate() {

		//Get input and speed
		float speed;
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		isWalking = !Input.GetKey(KeyCode.LeftShift);

		speed = isWalking ? walkSpeed : runSpeed;

		input = new Vector2(horizontal, vertical);

		if (input.sqrMagnitude > 1)
			input.Normalize();

		// always move along the camera forward as it is the direction that it being aimed at
		Vector3 desiredMove = transform.forward * input.y + transform.right * input.x;
		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
			characterController.height/2f, ~0, QueryTriggerInteraction.Ignore);
		desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;	//haven't seen any differences with this on so far

		moveDir.x = desiredMove.x*speed;
		moveDir.z = desiredMove.z*speed;

		//Modify the y-value
		if (characterController.isGrounded) {
			moveDir.y = -stickToGroundForce;

			if (jump) {
				moveDir.y = jumpSpeed;
				jump = false;
				jumping = true;
			}
		} else {
			moveDir += Physics.gravity*gravityMultiplier*Time.fixedDeltaTime;
		}

		if (PlayerPrefs.GetInt("Player_Swimming") == 1) {
			float mult = -Mathf.Sin(camera.transform.eulerAngles.x*Mathf.Deg2Rad);
			GetComponent<ConstantForce>().relativeForce = new Vector3(0f, mult*5f, 0f);
			moveDir.y = GetComponent<ConstantForce>().relativeForce.y;
		}

		animator.SetFloat("Speed", moveDir.magnitude);


		//Move the character
		characterController.Move(moveDir*Time.fixedDeltaTime);


	}

	private void RotateView() {
		//Prevent camera rotation when menu is open
		if (mouseLook.lockCursor)
			mouseLook.LookRotation (transform, camera.transform);
	}

}
