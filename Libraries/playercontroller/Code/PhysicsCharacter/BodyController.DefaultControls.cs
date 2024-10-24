﻿namespace Sandbox;

public sealed partial class BodyController : Component
{
	/// <summary>
	/// The direction we're looking.
	/// </summary>
	[Sync]
	public Angles EyeAngles { get; set; }


	[Sync]
	public bool IsDucking { get; set; }

	float headHeight;

	TimeSince timeSinceJump = 0;

	[Property, Group( "Helpers" ), Order( 3000 )] public bool UseInputControls { get; set; } = true;
	[Property, Group( "Helpers" ), Order( 3000 )] public bool UseCameraControls { get; set; } = true;
	[Property, Group( "Helpers" ), Order( 3000 )] public bool UseAnimatorControls { get; set; } = true;

	[Property, Group( "🕹️ Input" ), ShowIf( "UseInputControls", true ), Order( 4000 )] public float WalkSpeed { get; set; } = 110;
	[Property, Group( "🕹️ Input" ), ShowIf( "UseInputControls", true ), Order( 4000 )] public float RunSpeed { get; set; } = 320;
	[Property, Group( "🕹️ Input" ), ShowIf( "UseInputControls", true ), Order( 4000 )] public float JumpSpeed { get; set; } = 300;
	[Property, Group( "🕹️ Input" ), ShowIf( "UseInputControls", true ), Order( 4000 )] public float DuckedHeight { get; set; } = 40;

	[Property, Group( "📷 Camera" ), ShowIf( "UseCameraControls", true ), Order( 5000 )] public float EyeDistanceFromTop { get; set; } = 8;
	[Property, Group( "📷 Camera" ), ShowIf( "UseCameraControls", true ), Order( 5000 )] public bool ThirdPerson { get; set; } = true;
	[Property, Group( "📷 Camera" ), ShowIf( "UseCameraControls", true ), Order( 5000 )] public bool HideBodyInFirstPerson { get; set; } = true;
	[Property, Group( "📷 Camera" ), ShowIf( "UseCameraControls", true ), Order( 5000 )] public bool RotateWithGround { get; set; } = true;
	[Property, Group( "📷 Camera" ), ShowIf( "UseCameraControls", true ), Order( 5000 )] public Vector3 CameraOffset { get; set; } = new Vector3( 256, 0, 12 );
	[Property, Group( "📷 Camera" ), ShowIf( "UseCameraControls", true ), Order( 5000 ), InputAction] public string ToggleCameraModeButton { get; set; } = "view";

	/// <summary>
	/// The body will usually be a child object with SkinnedModelRenderer
	/// </summary>
	[Property, Group( "🕺 Animator" ), ShowIf( "UseAnimatorControls", true ), Order( 5000 )] public SkinnedModelRenderer Renderer { get; set; }

	bool ShowCreateBodyRenderer => UseAnimatorControls && Renderer is null;

	[Button( icon: "🪄" )]
	[Property, Group( "🕺 Animator" ), ShowIf( nameof( ShowCreateBodyRenderer ), true ), Order( 5000 )]
	public void CreateBodyRenderer()
	{
		var body = new GameObject( true, "Body" );
		body.Parent = GameObject;

		Renderer = body.AddComponent<SkinnedModelRenderer>();
		Renderer.Model = Model.Load( "models/citizen/citizen.vmdl" );
	}

	[Property, Group( "🕺 Animator" ), ShowIf( "UseAnimatorControls", true ), Order( 5000 )] public float RotationAngleLimit { get; set; } = 45.0f;
	[Property, Group( "🕺 Animator" ), ShowIf( "UseAnimatorControls", true ), Order( 5000 )] public float RotationSpeed { get; set; } = 1.0f;


	protected override void OnUpdate()
	{
		UpdateGroundEyeRotation();

		if ( Scene.IsEditor )
			return;

		if ( !IsProxy )
		{
			if ( UseInputControls )
			{
				UpdateEyeAngles();
			}

			if ( UseCameraControls )
			{
				UpdateCameraPosition();
			}
		}

		UpdateVisibility();
		UpdateAnimation();
	}

	public interface IEvents : ISceneEvent<IEvents>
	{
		/// <summary>
		/// Our eye angles are changing. Allows you to change the sensitivity, or stomp all together.
		/// </summary>
		void OnEyeAngles( ref Angles angles ) { }

		/// <summary>
		/// Called after we've set the camera up
		/// </summary>
		void PostCameraSetup( CameraComponent cam ) { }
	}

	void UpdateEyeAngles()
	{
		var input = Input.AnalogLook;

		IEvents.PostToGameObject( GameObject, x => x.OnEyeAngles( ref input ) );

		var ee = EyeAngles;
		ee += input;
		ee.roll = 0;
		ee.pitch = ee.pitch.Clamp( -90, 90 );

		EyeAngles = ee;
	}

	float _eyez;
	float _cameraDistance = 100;

	void UpdateVisibility()
	{
		if ( !UseCameraControls ) return;
		if ( Scene.Camera is not CameraComponent cam ) return;

		// we we looking through this GameObject?
		bool viewer = !ThirdPerson;
		viewer = viewer && HideBodyInFirstPerson;
		viewer = viewer && !IsProxy;

		if ( !IsProxy && _cameraDistance < 20 )
		{
			viewer = true;
		}

		if ( IsProxy )
		{
			viewer = false;
		}

		GameObject.Tags.Set( "viewer", viewer );
	}

	void UpdateCameraPosition()
	{
		if ( !UseCameraControls ) return;
		if ( Scene.Camera is not CameraComponent cam ) return;

		if ( !string.IsNullOrWhiteSpace( ToggleCameraModeButton ) )
		{
			if ( Input.Pressed( ToggleCameraModeButton ) )
				ThirdPerson = !ThirdPerson;
		}

		var rot = EyeAngles.ToRotation();
		cam.WorldRotation = rot;

		var eyePosition = WorldPosition + Vector3.Up * (BodyHeight - EyeDistanceFromTop);

		if ( IsOnGround && _eyez != 0 )
			eyePosition.z = _eyez.LerpTo( eyePosition.z, Time.Delta * 50 );

		_eyez = eyePosition.z;



		if ( ThirdPerson )
		{
			var cameraDelta = rot.Forward * -CameraOffset.x + rot.Up * CameraOffset.z;

			// clip the camera
			var tr = Scene.Trace.FromTo( eyePosition, eyePosition + cameraDelta )
							.IgnoreGameObjectHierarchy( GameObject )
							.Radius( 8 )
							.Run();

			// smooth the zoom in and out
			if ( tr.StartedSolid )
			{
				_cameraDistance = _cameraDistance.LerpTo( cameraDelta.Length, Time.Delta * 100.0f );
			}
			else if ( tr.Distance < _cameraDistance )
			{
				_cameraDistance = _cameraDistance.LerpTo( tr.Distance, Time.Delta * 200.0f );
			}
			else
			{
				_cameraDistance = _cameraDistance.LerpTo( tr.Distance, Time.Delta * 2.0f );
			}


			eyePosition = eyePosition + cameraDelta.Normal * _cameraDistance;
		}

		GameObject.Tags.Set( "viewer", _cameraDistance < 20 || (!ThirdPerson && HideBodyInFirstPerson) );
		cam.WorldPosition = eyePosition;
		cam.FieldOfView = Preferences.FieldOfView;

		IEvents.PostToGameObject( GameObject, x => x.PostCameraSetup( cam ) );
	}

	protected override void OnFixedUpdate()
	{

		if ( Scene.IsEditor ) return;


		{
			var tr = TraceBody( WorldPosition, WorldPosition + Vector3.Up * 100, 0.75f );
			headHeight = tr.Distance;
		}

		if ( IsProxy ) return;
		if ( !UseInputControls ) return;

		InputMove();
		UpdateDucking();
		InputJump();
	}

	void InputMove()
	{
		var rot = EyeAngles.ToRotation();

		WishVelocity = rot * Input.AnalogMove;

		if ( IsSwimming && Input.Down( "jump" ) ) WishVelocity += Vector3.Up;

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( Mode is Sandbox.Movement.MoveModeLadder ladderMode )
		{
			WishVelocity = new Vector3( 0, 0, Input.AnalogMove.x );

			WishVelocity *= 340.0f;

			if ( Input.Down( "jump" ) )
			{
				// Jump away from ladder
				Jump( ladderMode.ClimbingRotation.Backward * 200 );
			}
		}
		else
		{
			if ( Input.Down( "Run" ) ) WishVelocity *= RunSpeed;
			else WishVelocity *= WalkSpeed;
		}
	}

	float unduckedHeight = -1;
	Vector3 bodyDuckOffset = 0;

	void UpdateDucking()
	{
		var wantsDuck = Input.Down( "duck" );
		if ( wantsDuck == IsDucking ) return;

		unduckedHeight = MathF.Max( unduckedHeight, BodyHeight );
		var unduckDelta = unduckedHeight - DuckedHeight;

		// Can we unduck?
		if ( !wantsDuck )
		{
			if ( !IsOnGround )
				return;

			if ( headHeight < unduckDelta )
				return;
		}

		IsDucking = wantsDuck;

		if ( wantsDuck )
		{
			BodyHeight = DuckedHeight;

			// if we're not on the ground, keep out head in the same position
			if ( !IsOnGround )
			{
				WorldPosition += Vector3.Up * unduckDelta;
				Transform.ClearInterpolation();
				bodyDuckOffset = Vector3.Up * -unduckDelta;
			}
		}
		else
		{
			BodyHeight = unduckedHeight;
		}
	}

	void InputJump()
	{
		if ( TimeSinceGrounded > 0.33f ) return; // been off the ground for this many seconds, don't jump
		if ( !Input.Pressed( "Jump" ) ) return; // not pressing jump
		if ( timeSinceJump < 0.5f ) return; // don't jump too often

		timeSinceJump = 0;
		Jump( Vector3.Up * JumpSpeed );
		OnJumped();
	}

	[Broadcast]
	public void OnJumped()
	{
		if ( UseAnimatorControls && Renderer.IsValid() )
		{
			Renderer.Set( "b_jump", true );
		}
	}

	void UpdateAnimation()
	{
		if ( !UseAnimatorControls ) return;
		if ( !Renderer.IsValid() ) return;
		if ( Scene.IsEditor ) return;

		Renderer.LocalPosition = bodyDuckOffset;
		bodyDuckOffset = bodyDuckOffset.LerpTo( 0, Time.Delta * 5.0f );

		UpdateAnimationParameters();
		RotateRenderBody();
	}

	float _animRotationSpeed;

	void UpdateAnimationParameters()
	{
		var rot = Renderer.WorldRotation;

		var skidding = 0.0f;

		if ( WishVelocity.IsNearlyZero( 0.1f ) ) skidding = Velocity.Length.Remap( 0, 1000, 0, 1 );

		// velocity
		{
			var dir = WishVelocity;
			var forward = rot.Forward.Dot( dir );
			var sideward = rot.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			Renderer.Set( "move_direction", angle );
			Renderer.Set( "move_speed", Velocity.Length );
			Renderer.Set( "move_groundspeed", Velocity.WithZ( 0 ).Length );
			Renderer.Set( "move_y", sideward );
			Renderer.Set( "move_x", forward );
			Renderer.Set( "move_z", Velocity.z );
		}

		Renderer.SetLookDirection( "aim_eyes", EyeAngles.Forward, 1 );
		Renderer.SetLookDirection( "aim_head", EyeAngles.Forward, 1 );
		Renderer.SetLookDirection( "aim_body", EyeAngles.Forward, 1 );

		Renderer.Set( "b_swim", IsSwimming );
		Renderer.Set( "b_grounded", IsOnGround || IsClimbing );
		Renderer.Set( "b_climbing", IsClimbing );
		Renderer.Set( "move_rotationspeed", _animRotationSpeed );
		Renderer.Set( "skid", skidding );
		Renderer.Set( "move_style", WishVelocity.WithZ( 0 ).Length > WalkSpeed ? 2 : 1 );

		float duck = headHeight.Remap( 50, 0, 0, 0.5f, true );
		if ( IsDucking )
		{
			duck *= 3.0f;
			duck += 1.0f;
		}

		Renderer.Set( "duck", duck );
	}

	void RotateRenderBody()
	{
		_animRotationSpeed = 0;

		// ladder likes to have us facing it
		if ( Mode is Sandbox.Movement.MoveModeLadder ladderMode )
		{
			Renderer.WorldRotation = Rotation.Lerp( Renderer.WorldRotation, ladderMode.ClimbingRotation, Time.Delta * 5.0f );
			return;
		}

		var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

		var velocity = WishVelocity.WithZ( 0 );

		if ( velocity.Length > 50.0f )
		{
			targetAngle = Rotation.LookAt( velocity, Vector3.Up );
		}

		float rotateDifference = Renderer.WorldRotation.Distance( targetAngle );

		if ( rotateDifference > RotationAngleLimit || velocity.Length > 50.0f )
		{
			var newRotation = Rotation.Lerp( Renderer.WorldRotation, targetAngle, Time.Delta * 4.0f * RotationSpeed );

			// We won't end up actually moving to the targetAngle, so calculate how much we're actually moving
			var angleDiff = Renderer.WorldRotation.Angles() - newRotation.Angles(); // Rotation.Distance is unsigned
			_animRotationSpeed = angleDiff.yaw / Time.Delta;

			Renderer.WorldRotation = newRotation;
		}
	}

	Transform localGroundTransform;
	int groundHash;

	void UpdateGroundEyeRotation()
	{
		if ( GroundObject is null )
		{
			groundHash = default;
			return;
		}

		if ( !RotateWithGround )
		{
			groundHash = default;
			return;
		}

		var hash = HashCode.Combine( GroundObject );

		// Get out transform locally to the ground object
		var localTransform = GroundObject.WorldTransform.ToLocal( WorldTransform );

		// Work out the rotation delta chance since last frame
		var delta = localTransform.Rotation.Inverse * localGroundTransform.Rotation;

		// we only care about the yaw
		var deltaYaw = delta.Angles().yaw;

		//DebugDrawSystem.Current.Text( WorldPosition, $"{delta.Angles().yaw}" );

		// If we're on the same ground and we've rotated
		if ( hash == groundHash && deltaYaw != 0 )
		{
			// rotate the eye angles
			EyeAngles = EyeAngles.WithYaw( EyeAngles.yaw + deltaYaw );

			// rotate the body to avoid it animating to the new position
			if ( UseAnimatorControls && Renderer.IsValid() )
			{
				Renderer.WorldRotation *= new Angles( 0, deltaYaw, 0 );
			}
		}

		// Keep for next frame
		groundHash = hash;
		localGroundTransform = localTransform;
	}
}
