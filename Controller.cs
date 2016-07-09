using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	public Transform[] controlPathLeft;
	public Transform[] controlPathCentre;
	public Transform[] controlPathRight;

	public Sun sun;

	private Transform[] playerControlPath;
	private uint currentPathIndex = 1;

	public Transform character;
	private enum PlayerMovement {Left,Right};

	public float rotationSpeed = 0.8f;
	public float jumpHeight = 0.1f;
	public float maximumForwardVelocity = 7.0f;

	private PlayerMovement movement;
	private float targetXPosition;
	private float xSpeed = 3.5f;
	private bool shouldMoveX = false;

	//for swiping
	private readonly Vector2 mXAxis = new Vector2(1, 0);
	private readonly Vector2 mYAxis = new Vector2(0, 1);
	private const float mAngleRange = 30;
	private const float mMinSwipeDist = 50.0f;
	private const float mMinVelocity  = 1000.0f;
	private Vector2 mStartPosition;
	private float mSwipeStartTime;

	void OnDrawGizmos(){
		iTween.DrawPath(controlPathLeft,Color.blue);
		iTween.DrawPath(controlPathCentre,Color.red);
		iTween.DrawPath(controlPathRight,Color.green);
	}	

	void Start()
	{
		//plop the character pieces in the "Ignore Raycast" layer so we don't have false raycast data:	
		foreach (Transform child in character) 
		{
			child.gameObject.layer=2;
		}

		playerControlPath = controlPathCentre;
		targetXPosition = 0;

		Rigidbody rb = character.GetComponent<Rigidbody>();
		rb.velocity = new Vector3 (0, 0, 10.0f);


	}

	void Update()
	{
		DeathDetection();
		DetectKeys();
		MoveCharacter();
		MoveCamera();
	}

	void DeathDetection()
	{
		if (character.position.y < -30) 
		{
			Application.LoadLevel (Application.loadedLevelName);
		}
	}

	void DetectKeys()
	{
		// Mouse button down, possible chance for a swipe
		if (Input.GetMouseButtonDown(0)) 
		{
			// Record start time and position
			mStartPosition = new Vector2(Input.mousePosition.x,
			                             Input.mousePosition.y);
			mSwipeStartTime = Time.time;
		}
		
		// Mouse button up, possible chance for a swipe
		if (Input.GetMouseButtonUp(0)) 
		{
			float deltaTime = Time.time - mSwipeStartTime;
			
			Vector2 endPosition  = new Vector2(Input.mousePosition.x,
			                                   Input.mousePosition.y);
			Vector2 swipeVector = endPosition - mStartPosition;
			
			float velocity = swipeVector.magnitude/deltaTime;
			
			if (velocity > mMinVelocity &&
			    swipeVector.magnitude > mMinSwipeDist) 
			{
				swipeVector.Normalize();
				
				float angleOfSwipe = Vector2.Dot(swipeVector, mXAxis);
				angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
				
				// Detect left and right swipe
				if (angleOfSwipe < mAngleRange) 
				{
					OnSwipeRight();
				} else if ((180.0f - angleOfSwipe) < mAngleRange) 
				{
					OnSwipeLeft();
				} else 
				{
					// Detect top and bottom swipe
					angleOfSwipe = Vector2.Dot(swipeVector, mYAxis);
					angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
					if (angleOfSwipe < mAngleRange) 
					{
						OnSwipeUp();
					} 
				}
			}
		}

		//keyboard controls! ///////////////
		if(Input.GetKeyDown("right"))
		{
			MovePlayerRight();
		}

		if(Input.GetKeyDown("left"))
		{
			MovePlayerLeft();
		}

		if (Input.GetKeyDown("space")) 
		{
			characterJump();
		}
	}

	private void OnSwipeLeft() 
	{
		MovePlayerLeft();
	}
	
	private void OnSwipeRight() 
	{
		MovePlayerRight();
	}

	private void OnSwipeUp()
	{
		characterJump ();
	}

	private void MovePlayerLeft()
	{
		if (!sun.isGrounded) 
		{
			return;
		}
		movement = PlayerMovement.Left;
		ChangePath();
	}

	private void MovePlayerRight()
	{
		if (!sun.isGrounded) 
		{
			return;
		}
		movement = PlayerMovement.Right;
		ChangePath();
	}

	void ChangePath()
	{
		if (movement == PlayerMovement.Left) 
		{
			if (currentPathIndex > 0)
			{
				shouldMoveX = true;
				targetXPosition -= 2;
				currentPathIndex--;
				characterJump();
			}
		}
		if (movement == PlayerMovement.Right) 
		{
			if (currentPathIndex < 2)
			{
				shouldMoveX = true;
				targetXPosition += 2;
				currentPathIndex++;
				characterJump();
			}
		}

		if (currentPathIndex == 0) 
		{
			playerControlPath = controlPathLeft;
		}
		else if (currentPathIndex == 1) 
		{
			playerControlPath = controlPathCentre;
		}
		else if (currentPathIndex == 2) 
		{
			playerControlPath = controlPathRight;
		}
	}

	void characterJump()
	{
		if (!sun.isGrounded) 
		{
			return;
		}


		Rigidbody rb = character.GetComponent<Rigidbody>();
		rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
	}

	void MoveCharacter()
	{
		var characterX = character.position.x;
		Rigidbody rb = character.GetComponent<Rigidbody>();

		if (movement == PlayerMovement.Left) 
		{
			shouldMoveX = characterX >= targetXPosition;
		} else 
		{
			shouldMoveX = characterX <= targetXPosition;
		}

		if (shouldMoveX) 
		{
			rb.constraints = RigidbodyConstraints.None;
			rb.constraints = RigidbodyConstraints.FreezeRotationZ;
			rb.constraints = RigidbodyConstraints.FreezeRotationY;

			if (movement == PlayerMovement.Right) {
				rb.velocity = new Vector3 (1.8f, rb.velocity.y, rb.velocity.z);
			}
			
			if (movement == PlayerMovement.Left) 
			{
				rb.velocity = new Vector3 (-1.8f, rb.velocity.y, rb.velocity.z);
			}
		} else 
		{
			rb.constraints = RigidbodyConstraints.FreezePositionX;
			rb.constraints = RigidbodyConstraints.FreezeRotationZ;
			rb.constraints = RigidbodyConstraints.FreezeRotationY;
		}

		var movementZ = 0.0f;
		if (rb.velocity.z < maximumForwardVelocity) {
			movementZ = 3.5f;
		} else {
			movementZ = -2.0f;
		}
		Vector3 zVector = new Vector3 (0, 0, movementZ);
		rb.AddForce (zVector * rotationSpeed);
	}

	void MoveCamera()
	{
		iTween.MoveUpdate(Camera.main.gameObject,new Vector3(character.position.x,2.7f,character.position.z-5f),.9f);	
	}
}