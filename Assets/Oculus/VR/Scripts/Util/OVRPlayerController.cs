/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus Utilities SDK License Version 1.31 (the "License"); you may not use
the Utilities SDK except in compliance with the License, which is provided at the time of installation
or download, or which otherwise accompanies this software in either electronic or hard copy form.
You may obtain a copy of the License at https://developer.oculus.com/licenses/utilities-1.31

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Normal.Realtime;


/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController : MonoBehaviour
{
	/// <summary>
	/// The rate acceleration during movement.
	/// </summary>
	public float Acceleration = 0.1f;

	/// <summary>
	/// The rate of damping on movement.
	/// </summary>
	public float Damping = 0.3f;

	/// <summary>
	/// The rate of additional damping when moving sideways or backwards.
	/// </summary>
	public float BackAndSideDampen = 0.5f;

	/// <summary>
	/// The force applied to the character when jumping.
	/// </summary>
	public float JumpForce = 0.3f;

	/// <summary>
	/// The rate of rotation when using a gamepad.
	/// </summary>
	public float RotationAmount = 1.5f;

	/// <summary>
	/// The rate of rotation when using the keyboard.
	/// </summary>
	public float RotationRatchet = 45.0f;

	/// <summary>
	/// The player will rotate in fixed steps if Snap Rotation is enabled.
	/// </summary>
	[Tooltip("The player will rotate in fixed steps if Snap Rotation is enabled.")]
	public bool SnapRotation = true;

	/// <summary>
	/// How many fixed speeds to use with linear movement? 0=linear control
	/// </summary>
	[Tooltip("How many fixed speeds to use with linear movement? 0=linear control")]
	public int FixedSpeedSteps;

	/// <summary>
	/// If true, reset the initial yaw of the player controller when the Hmd pose is recentered.
	/// </summary>
	public bool HmdResetsY = true;

	/// <summary>
	/// If true, tracking data from a child OVRCameraRig will update the direction of movement.
	/// </summary>
	public bool HmdRotatesY = true;

	/// <summary>
	/// Modifies the strength of gravity.
	/// </summary>
	public float GravityModifier = 0.379f;

	/// <summary>
	/// If true, each OVRPlayerController will use the player's physical height.
	/// </summary>
	public bool useProfileData = true;

	/// <summary>
	/// The CameraHeight is the actual height of the HMD and can be used to adjust the height of the character controller, which will affect the
	/// ability of the character to move into areas with a low ceiling.
	/// </summary>
	[NonSerialized]
	public float CameraHeight;

	/// <summary>
	/// This event is raised after the character controller is moved. This is used by the OVRAvatarLocomotion script to keep the avatar transform synchronized
	/// with the OVRPlayerController.
	/// </summary>
	public event Action<Transform> TransformUpdated;

	/// <summary>
	/// This bool is set to true whenever the player controller has been teleported. It is reset after every frame. Some systems, such as
	/// CharacterCameraConstraint, test this boolean in order to disable logic that moves the character controller immediately
	/// following the teleport.
	/// </summary>
	[NonSerialized] // This doesn't need to be visible in the inspector.
	public bool Teleported;

	/// <summary>
	/// This event is raised immediately after the camera transform has been updated, but before movement is updated.
	/// </summary>
	public event Action CameraUpdated;

	/// <summary>
	/// This event is raised right before the character controller is actually moved in order to provide other systems the opportunity to
	/// move the character controller in response to things other than user input, such as movement of the HMD. See CharacterCameraConstraint.cs
	/// for an example of this.
	/// </summary>
	public event Action PreCharacterMove;

	/// <summary>
	/// When true, user input will be applied to linear movement. Set this to false whenever the player controller needs to ignore input for
	/// linear movement.
	/// </summary>
	public bool EnableLinearMovement = true;

	/// <summary>
	/// When true, user input will be applied to rotation. Set this to false whenever the player controller needs to ignore input for rotation.
	/// </summary>
	public bool EnableRotation = true;

	/// <summary>
	/// Rotation defaults to secondary thumbstick. You can allow either here. Note that this won't behave well if EnableLinearMovement is true.
	/// </summary>
	public bool RotationEitherThumbstick = false;

	public CharacterController Controller = null;
	protected OVRCameraRig CameraRig = null;

	private float MoveScale = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;
	private OVRPose? InitialPose;
	public float InitialYRotation { get; private set; }
	private float MoveScaleMultiplier = 1.0f;
	private float RotationScaleMultiplier = 1.0f;
	private bool SkipMouseRotation = true; // It is rare to want to use mouse movement in VR, so ignore the mouse by default.
	private bool HaltUpdateMovement = false;
	private bool prevHatLeft = false;
	private bool prevHatRight = false;
	private float SimulationRate = 60f;
	private float buttonRotation = 0f;
	private bool ReadyToSnapTurn; // Set to true when a snap turn has occurred, code requires one frame of centered thumbstick to enable another snap turn.
	private bool playerControllerEnabled = false;

	Realtime realtime;

	//public enum EHandSide { LEFT, RIGHT }


	RealtimeView headRealtimeView;
	public RealtimeView HeadRealtimeView { set => headRealtimeView = value; get => headRealtimeView; }

	//Respawn
	Vector3 respawnPoint = Vector3.zero;

	public Vector3 RespawnPoint { set => respawnPoint = value; get => respawnPoint; }

	//Grabbing & Hanging

	[SerializeField]
	GameObject TrackingSpaceAnchor;

	public enum EOmniDeviceStartup
    {
		None,
		OnlyLeft,
		OnlyRight,
		Both
    }

	[SerializeField]
	EOmniDeviceStartup omniDeviceStartup;

	public EOmniDeviceStartup OmniDeviceStartupState { get => omniDeviceStartup; }

	Transform grabHandleTransform;
	Transform grabHandleChild;

	//----Vignette (Note: Still experimenting with vignette, cleaning up after final iteration)
	Vector3 vignetteOpen = new Vector3(0.2440064f, 0.3996776f, 0.3169284f);

	Vector3 vignetteClosed = new Vector3(0.1253421f, 0.2053079f, 0.1628009f);

	Vector3 vignette2Open = new Vector3(0.19f, 0.19f, 0.19f);
	Vector3 vignette2Closed = new Vector3(0.1023599f, 0.1023599f, 0.1023599f);

	string vignetteOpacityPropertyName = "Vector1_A9520D05";
	int vignetteGraphOpacityID;

	//Walking shake
	Vector3 vignetteUpperLocalPos = new Vector3(0.0f, -0.009f, 0.204f);
	Vector3 vignetteLowerLocalPos = new Vector3(0.0f, -0.02f, 0.204f);

	Material vignetteMaterial;

	[SerializeField]
	GameObject vignette;

	[SerializeField]
	AnimationCurve vignetteCurve;

	float vignetteIncrement = 0;

	float airTime = 0;

	//-------------

	//External pair of tracked hands that gives us delta movement in world space for climbing mechanic.
	GameObject externalAvatarBase;
	Transform externalLeftHand;
	Transform externalRightHand;
	Vector3 externalLeftHandPositionOnGrab;
	Vector3 externalRightHandPositionOnGrab;

	Vector3 PlayerControllerOffsetToHandle;

	bool grabbingClimbHandle = false;
	bool grabbingZipLine = false;
	bool grabbingAnything = false;

	public bool GrabbingAnything { get => grabbingAnything; }


	//ZipLine

	Vector3 handPositionOnZipLineGrab;

	Transform externalHandForZipLine;
	Vector3 externalHandForZipLinePosOnGrab;

	//The transform of the start point structure for the zipline we grabbed
	Transform grabbedZipLineStartPoint;
	Vector3 grabbedZipLineStartPointPosOnGrab;

	Vector3 playerControllerOffsettToHandOnGrab;

	float zipStartToHandDistanceOnGrab;

	Vector3 handOffsettToMiddleOfBeamAtGrabPoint;

	Vector3 ZipStartForwardOnGrab;

	//Trying to support moving the start-piece and have player transport normally on beam
	Vector3 projectionOfVectorBetweenOldAndNewStartPositionsOnNewDirection;

	OVRInput.Axis2D zipLineControllerThumbStick;

	float zipSpeedModifier;




	float zipLineSpeed = 3.0f;

	float distanceTravelledOnZipLine = 0;

	bool grabbing_LeftHand = false;
	bool grabbing_RightHand = false;

	//---- Controller Swing-movement

	bool swingMoving = false;

	float timeSinceLastSwing = 0.2f;
	float maxTimeForSwingMovement = 0.6f;

	Vector3 swingWalkVector;
	float swingWalkDefaultSpeed = 8;

	int numberOfSwings = 0;

	//----

	event Action OnUpdate_Fixed;
	event Action OnUpdate_PerFrame;

	public event Action OnHazardEncountered;
	public void RespondToEncounteredHazard()
    {
		OnHazardEncountered?.Invoke();
    }

	public void SetExternalHands(bool leftHand, Transform transform, GameObject baseGameObject)
    {
		if (leftHand) externalLeftHand = transform;
		else externalRightHand = transform;

		externalAvatarBase = baseGameObject;
    }

	public void EnableController()
    {
		Controller.enabled = true;
    }

	float handleYRotationOnGrab;
	Quaternion playerControllerRotationOnGrab;

	Vector3 handleOffsettToGrabPointOnGrab;

	float handleOffsettDivider = 5;

	public void SetGrabbingClimbHandle(bool grabbing, int hand, Vector3 handPosition, Transform handleTransform = null)
    {
		if (grabbing && handleTransform)
		{
			grabHandleTransform = handleTransform;
			grabHandleChild = handleTransform.GetChild(0);
			grabHandleChild.position = handPosition;
			handleOffsettToGrabPointOnGrab = (handPosition - handleTransform.position);

			PlayerControllerOffsetToHandle = transform.position - grabHandleTransform.position;
		}

		if (hand == 0)
		{
			grabbing_LeftHand = (grabbing) ? true : false;

			if (grabbing_LeftHand) grabbing_RightHand = false;

			externalLeftHandPositionOnGrab = externalLeftHand.position;
		}

		else if (hand == 1)
		{
			grabbing_RightHand = (grabbing) ? true : false;

			if (grabbing_RightHand) grabbing_LeftHand = false;

			externalRightHandPositionOnGrab = externalRightHand.position;

		}

		if (!grabbing_LeftHand && !grabbing_RightHand)
		{
			Controller.enabled = true;
			HmdRotatesY = true;
			grabbingClimbHandle = grabbingAnything = false;
		}
		else
		{
			if (handleTransform) handleYRotationOnGrab = handleTransform.rotation.eulerAngles.y;

			playerControllerRotationOnGrab = transform.rotation;

			Controller.enabled = false;
			HmdRotatesY = false;

			FallSpeed = 0.0f;
			grabbingClimbHandle = grabbingAnything = true;
		}
    }


	public void SetGrabbingZipLine(bool grabbing, Transform hand = null, int handSide = -1, Transform zipLineStart = null, float zipLineSpeed = 0.0f)
    {
		if (grabbing == true)
		{
			playerControllerOffsettToHandOnGrab = transform.position - hand.position;

			ZipStartForwardOnGrab = zipLineStart.forward;

			zipStartToHandDistanceOnGrab = (hand.transform.position - zipLineStart.position).magnitude;

			handOffsettToMiddleOfBeamAtGrabPoint = hand.position - (zipLineStart.position + (zipLineStart.forward * zipStartToHandDistanceOnGrab));

			grabbedZipLineStartPoint = zipLineStart;
			grabbedZipLineStartPointPosOnGrab = zipLineStart.position;

			externalHandForZipLine = (handSide == 0) ? externalLeftHand : externalRightHand;
			externalHandForZipLinePosOnGrab = externalHandForZipLine.position;

			zipLineControllerThumbStick = (handSide == 0) ? OVRInput.Axis2D.PrimaryThumbstick : OVRInput.Axis2D.SecondaryThumbstick;

			Controller.enabled = false;
			HmdRotatesY = false;

			this.zipLineSpeed = zipLineSpeed;

			FallSpeed = 0.0f;
			

			grabbingZipLine = grabbingAnything = true;
			
		}
		else
		{
			Controller.enabled = true;
			HmdRotatesY = true;

			grabbingZipLine = grabbingAnything = false;
			distanceTravelledOnZipLine = 0;
		}
	}

	public void ResetToRespawnPoint()
    {
		Controller.enabled = false;
		transform.position = respawnPoint;
		Controller.enabled = true;
	}
	

	void Start()
	{
		// Add eye-depth as a camera offset from the player controller
		var p = CameraRig.transform.localPosition;
		p.z = OVRManager.profile.eyeDepth;
		CameraRig.transform.localPosition = p;

		

		//Default respawn point for testing, before entering any respawn point triggers
		respawnPoint = transform.position;

		vignetteMaterial = vignette.GetComponent<MeshRenderer>().material;

		vignetteGraphOpacityID = vignetteMaterial.shader.FindPropertyIndex(vignetteOpacityPropertyName);

		if (!SceneManager.GetActiveScene().name.Contains("MainMenu"))
        {
			realtime = GameObject.Find("Realtime").GetComponent<Realtime>();

			OnUpdate_Fixed += GameplayFunctionsFixedUpdate;
			OnUpdate_PerFrame += GameplayFunctionsUpdate;


			if (PlayerPrefs.GetInt("blinders") == 1)
			{
				OnUpdate_Fixed += OperateVignette;
			}
			else vignette.transform.gameObject.SetActive(false);

			EnableLinearMovement = false;
		}

		OVRManager.display.RecenterPose();

	}

	public void EnableMovement()
    {
		EnableLinearMovement = true;
    }

	void Awake()
	{
		Controller = gameObject.GetComponent<CharacterController>();

		if (Controller == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

		// We use OVRCameraRig to set rotations to cameras,
		// and to be influenced by rotation
		OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();

		if (CameraRigs.Length == 0)
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		else if (CameraRigs.Length > 1)
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		else
			CameraRig = CameraRigs[0];

		InitialYRotation = transform.rotation.eulerAngles.y;
	}

	void OnEnable()
	{
	}

	void OnDisable()
	{
		if (playerControllerEnabled)
		{
			OVRManager.display.RecenteredPose -= ResetOrientation;

			if (CameraRig != null)
			{
				CameraRig.UpdatedAnchors -= UpdateTransform;
			}
			playerControllerEnabled = false;
		}
	}

	void Update()
	{
		if (!playerControllerEnabled)
		{
			if (OVRManager.OVRManagerinitialized)
			{
				OVRManager.display.RecenteredPose += ResetOrientation;

				if (CameraRig != null)
				{
					CameraRig.UpdatedAnchors += UpdateTransform;
				}
				playerControllerEnabled = true;
			}
			else
				return;
		}
		//Use keys to ratchet rotation
		if (Input.GetKeyDown(KeyCode.Q))
			buttonRotation -= RotationRatchet;

		if (Input.GetKeyDown(KeyCode.E))
			buttonRotation += RotationRatchet;

		//Our gameplay functions
		OnUpdate_PerFrame?.Invoke();
	}

	private void FixedUpdate()
    {
		OnUpdate_Fixed?.Invoke();
	}

	bool leftUpRightDownSwung = false;
	bool rightUpLeftDownSwung = false;

	bool isSwingJumping = false;

	public bool IsSwingJumping { get => isSwingJumping; }

	//It's a long function with different stuff, but since we're doing this every fixed update, 
	//there could be a small performance gain having them gathered rather than calling a bunch of different functions 
	void GameplayFunctionsFixedUpdate()
    {
		//Falldeath
		if (transform.position.y < -150)
		{
			transform.position = respawnPoint;
		}


		if (Controller.isGrounded && isSwingJumping) isSwingJumping = false;

		
		//Swing-jumping
		if ((!grabbingAnything && EnableLinearMovement && !isSwingJumping && OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).y > 1.5 &&
			OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).y > 1.5))
        {
			Debug.Log("OVRPC: Calling from swing jump");

			if (!isSwingJumping)
            {
				isSwingJumping = true;
				Jump();
			}			
		}

		//Swing-moving
		if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).y > 0.6 &&
			OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).y < -0.6 && !grabbingAnything)
		{
			swingMoving = true;
			timeSinceLastSwing = 0;
			/* If accidental swing combos gets annoying, this is a system for requiring two swings to start walking
			if (!leftUpRightDownSwung)
            {
				leftUpRightDownSwung = true;
				rightUpLeftDownSwung = false;
				numberOfSwings++;
			}
			*/
		}

		else if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).y < -0.6 &&
			OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).y > 0.6 && !grabbingAnything)
		{
			 swingMoving = true;
			 timeSinceLastSwing = 0;
			 
			 /*
			 if (!rightUpLeftDownSwung)
             {
			 	rightUpLeftDownSwung = true;
			 	leftUpRightDownSwung = false;
			 	numberOfSwings++;
			 }
			 */
		}

		if (swingMoving)
		{

			if (timeSinceLastSwing > maxTimeForSwingMovement)
			{
				swingMoving = false;
				numberOfSwings = 0;
			}

			else timeSinceLastSwing += Time.fixedDeltaTime;

			if (EnableLinearMovement /*&& numberOfSwings > 1*/)
            {
				swingWalkVector = (transform.forward * Time.fixedDeltaTime * (maxTimeForSwingMovement - timeSinceLastSwing) * swingWalkDefaultSpeed *
					(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y + 1));
				Controller.Move(swingWalkVector);
			}
		}
	}

	void OperateVignette()
	{
		//----Vignette

		//Move
		if (Controller.isGrounded)
		{
			if (grabbingAnything ||
				(Mathf.Abs(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y) > 0.3 || Mathf.Abs(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x) > 0.3))
			{
				if ((EnableLinearMovement || grabbingAnything) && vignetteIncrement < 1) vignetteIncrement += 0.025f;
			}

			else if (!grabbingZipLine && vignetteIncrement > 0)
			{
				vignetteIncrement -= 0.025f;
			}

			//Debug.Log("OVRPlayerController::FixedUpdate: vignetteOpacityPropertyIndex = " + vignetteGraphOpacityID);
			//Debug.Log("PropertyID gotten from Find function = " + vignetteMaterial.shader.FindPropertyIndex(vignetteOpacityPropertyName));
			//NOTE: SetFloat is not working with property ID gotten from Shader. Sending in property name until further developments
			vignetteMaterial.SetFloat(vignetteOpacityPropertyName, vignetteCurve.Evaluate(vignetteIncrement));

			if (!grabbingAnything) 
					vignette.transform.localPosition = 
					Vector3.Lerp(vignetteLowerLocalPos, vignetteUpperLocalPos, Mathf.Abs((Mathf.Sin(Time.fixedTime * 5))) * vignetteIncrement);
		}

		
		//Jump / Fall
		else if (!Controller.isGrounded)
		{
			//Jumping / Regular fall
			if ((EnableLinearMovement) && !swingMoving && vignetteIncrement < 1)
			{
				vignetteIncrement += 0.025f;
			}

				vignetteMaterial.SetFloat(vignetteOpacityPropertyName, vignetteCurve.Evaluate(vignetteIncrement));
		}

		//if (grabbingZipLine && vignetteIncrement < 1) vignetteIncrement += 0.025f;
		//vignetteMaterial.SetFloat(vignetteOpacityPropertyName, vignetteCurve.Evaluate(vignetteIncrement));
	}

	void GameplayFunctionsUpdate()
    {
		if (externalAvatarBase) externalAvatarBase.transform.rotation = TrackingSpaceAnchor.transform.rotation;

		if (OVRInput.GetDown(OVRInput.Button.One) && 
			OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) < 0.1f && 
			!GrabbingAnything && EnableLinearMovement) 
			Jump();


		//Handle grabbing and climbing
		if (grabbingClimbHandle)
		{
			Vector3 externalHandWorldPositionDelta = Vector3.zero;

			if (grabbing_LeftHand)
			{
				externalHandWorldPositionDelta = externalLeftHandPositionOnGrab - externalLeftHand.position;

			}

			else if (grabbing_RightHand)
			{
				externalHandWorldPositionDelta = externalRightHandPositionOnGrab - externalRightHand.position;
			}

			Vector3 handleOffsettToGrabPoint = (grabHandleChild.transform.position - grabHandleTransform.position);

			transform.position = (grabHandleTransform.position + PlayerControllerOffsetToHandle +
								 externalHandWorldPositionDelta); // + handleOffsettToGrabPointOnGrab + (handleOffsettToGrabPoint));

			//transform.rotation = playerControllerRotationOnGrab * Quaternion.Euler(0, grabHandleTransform.rotation.eulerAngles.y - handleYRotationOnGrab, 0);
		}
		//Handle ZipLine transportation
		if (grabbingZipLine)
		{
			zipSpeedModifier = (OVRInput.Get(zipLineControllerThumbStick).y + 1);

			distanceTravelledOnZipLine += (zipLineSpeed * Time.deltaTime * zipSpeedModifier);



			transform.position =
				grabbedZipLineStartPoint.position +
				(grabbedZipLineStartPoint.forward * zipStartToHandDistanceOnGrab) +
				playerControllerOffsettToHandOnGrab - (externalHandForZipLine.position - externalHandForZipLinePosOnGrab) +
				handOffsettToMiddleOfBeamAtGrabPoint +
				(grabbedZipLineStartPoint.forward * distanceTravelledOnZipLine ) +
				(grabbedZipLineStartPoint.forward * Time.deltaTime * zipSpeedModifier); //-
				//projectionOfVectorBetweenOldAndNewStartPositionsOnNewDirection;
		}
	}

    

	protected virtual void UpdateController()
	{
		if (Controller.enabled == false) return;

		if (useProfileData)
		{
			if (InitialPose == null)
			{
				// Save the initial pose so it can be recovered if useProfileData
				// is turned off later.
				InitialPose = new OVRPose()
				{
					position = CameraRig.transform.localPosition,
					orientation = CameraRig.transform.localRotation
				};
			}

			var p = CameraRig.transform.localPosition;
			if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
			{
				p.y = OVRManager.profile.eyeHeight - (0.5f * Controller.height) + Controller.center.y;
			}
			else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
			{
				p.y = -(0.5f * Controller.height) + Controller.center.y;
			}
			CameraRig.transform.localPosition = p;
		}
		else if (InitialPose != null)
		{
			// Return to the initial pose if useProfileData was turned off at runtime
			CameraRig.transform.localPosition = InitialPose.Value.position;
			CameraRig.transform.localRotation = InitialPose.Value.orientation;
			InitialPose = null;
		}

		CameraHeight = CameraRig.centerEyeAnchor.localPosition.y;

		if (CameraUpdated != null)
		{
			CameraUpdated();
		}

		UpdateMovement();

		Vector3 moveDirection = Vector3.zero;

		float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;

		moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

		// Gravity
		if (Controller.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);

		moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;


		if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
		{
			// Offset correction for uneven ground
			float bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
			moveDirection -= bumpUpOffset * Vector3.up;
		}

		if (PreCharacterMove != null)
		{
			PreCharacterMove();
			Teleported = false;
		}

		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

		// Move contoller
		Controller.Move(moveDirection);
		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
	}





	public virtual void UpdateMovement()
	{
		if (timeSinceLastSwing < 0.2 || HaltUpdateMovement)
			return;

		if (EnableLinearMovement)
		{
			bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
			bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
			bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
			bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

			bool dpad_move = false;

			if (OVRInput.Get(OVRInput.Button.DpadUp))
			{
				moveForward = true;
				dpad_move = true;

			}

			if (OVRInput.Get(OVRInput.Button.DpadDown))
			{
				moveBack = true;
				dpad_move = true;
			}

			MoveScale = 1.0f;

			if ((moveForward && moveLeft) || (moveForward && moveRight) ||
				(moveBack && moveLeft) || (moveBack && moveRight))
				MoveScale = 0.70710678f;

			// No positional movement if we are in the air
			//if (!Controller.isGrounded)
				//MoveScale = 0.0f;

			MoveScale *= SimulationRate * Time.deltaTime;

			// Compute this for key movement
			float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

			// Run!
			if (dpad_move || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				moveInfluence *= 2.0f;

			Quaternion ort = transform.rotation;
			Vector3 ortEuler = ort.eulerAngles;
			ortEuler.z = ortEuler.x = 0f;
			ort = Quaternion.Euler(ortEuler);

			if (moveForward)
				MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
			if (moveBack)
				MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
			if (moveLeft)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
			if (moveRight)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);



			moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

#if !UNITY_ANDROID // LeftTrigger not avail on Android game pad
			moveInfluence *= 1.0f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
#endif

			Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

			// If speed quantization is enabled, adjust the input to the number of fixed speed steps.
			if (FixedSpeedSteps > 0)
			{
				primaryAxis.y = Mathf.Round(primaryAxis.y * FixedSpeedSteps) / FixedSpeedSteps;
				primaryAxis.x = Mathf.Round(primaryAxis.x * FixedSpeedSteps) / FixedSpeedSteps;
			}

			if (primaryAxis.y > 0.0f)
				MoveThrottle += ort * (primaryAxis.y * transform.lossyScale.z * moveInfluence * Vector3.forward);

			if (primaryAxis.y < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.y) * transform.lossyScale.z * moveInfluence *
									   BackAndSideDampen * Vector3.back);

			if (primaryAxis.x < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.x) * transform.lossyScale.x * moveInfluence *
									   BackAndSideDampen * Vector3.left);

			if (primaryAxis.x > 0.0f)
				MoveThrottle += ort * (primaryAxis.x * transform.lossyScale.x * moveInfluence * BackAndSideDampen *
									   Vector3.right);
		}

		if (EnableRotation)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			float rotateInfluence = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;

			bool curHatLeft = OVRInput.Get(OVRInput.Button.PrimaryShoulder);

			if (curHatLeft && !prevHatLeft)
				euler.y -= RotationRatchet;

			prevHatLeft = curHatLeft;

			bool curHatRight = OVRInput.Get(OVRInput.Button.SecondaryShoulder);

			if (curHatRight && !prevHatRight)
				euler.y += RotationRatchet;

			prevHatRight = curHatRight;

			euler.y += buttonRotation;
			buttonRotation = 0f;


#if !UNITY_ANDROID || UNITY_EDITOR
			if (!SkipMouseRotation)
				euler.y += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
#endif

			if (SnapRotation)
			{
				if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) ||
					(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft)))
				{
					if (ReadyToSnapTurn)
					{
						euler.y -= RotationRatchet;
						ReadyToSnapTurn = false;
					}
				}
				else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) ||
					(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight)))
				{
					if (ReadyToSnapTurn)
					{
						euler.y += RotationRatchet;
						ReadyToSnapTurn = false;
					}
				}
				else
				{
					ReadyToSnapTurn = true;
				}
			}
			else
			{
				Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				if (RotationEitherThumbstick)
				{
					Vector2 altSecondaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
					if (secondaryAxis.sqrMagnitude < altSecondaryAxis.sqrMagnitude)
					{
						secondaryAxis = altSecondaryAxis;
					}
				}
				euler.y += secondaryAxis.x * rotateInfluence;
			}

			transform.rotation = Quaternion.Euler(euler);
		}
	}


	/// <summary>
	/// Invoked by OVRCameraRig's UpdatedAnchors callback. Allows the Hmd rotation to update the facing direction of the player.
	/// </summary>
	public void UpdateTransform(OVRCameraRig rig)
	{
		Transform root = CameraRig.trackingSpace;
		Transform centerEye = CameraRig.centerEyeAnchor;

		if (HmdRotatesY && !Teleported)
		{
			Vector3 prevPos = root.position;
			Quaternion prevRot = root.rotation;

			transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

			root.position = prevPos;
			root.rotation = prevRot;
		}

		UpdateController();
		if (TransformUpdated != null)
		{
			TransformUpdated(root);
		}
	}

	/// <summary>
	/// Jump! Must be enabled manually.
	/// </summary>
	public bool Jump()
	{
		if (!Controller.isGrounded)
			return false;

		MoveThrottle += new Vector3(0, transform.lossyScale.y * JumpForce, 0);

		return true;
	}

	/// <summary>
	/// Stop this instance.
	/// </summary>
	public void Stop()
	{
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}

	/// <summary>
	/// Gets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	/// <summary>
	/// Sets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	/// <summary>
	/// Gets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	/// <summary>
	/// Sets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	/// <summary>
	/// Gets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">Allow mouse rotation.</param>
	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = SkipMouseRotation;
	}

	/// <summary>
	/// Sets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">If set to <c>true</c> allow mouse rotation.</param>
	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		SkipMouseRotation = skipMouseRotation;
	}

	/// <summary>
	/// Gets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">Halt update movement.</param>
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}

	/// <summary>
	/// Sets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

	/// <summary>
	/// Resets the player look rotation when the device orientation is reset.
	/// </summary>
	public void ResetOrientation()
	{
		if (HmdResetsY && !HmdRotatesY)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			euler.y = InitialYRotation;
			transform.rotation = Quaternion.Euler(euler);
		}
	}
}
