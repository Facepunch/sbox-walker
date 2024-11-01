using Sandbox.Audio;

namespace Sandbox;

public sealed partial class PlayerController : Component
{
	SkinnedModelRenderer _renderer;

	[Property, FeatureEnabled( "Animator", Icon = "sports_martial_arts" )] public bool UseAnimatorControls { get; set; } = true;

	/// <summary>
	/// The body will usually be a child object with SkinnedModelRenderer
	/// </summary>
	[Property, Feature( "Animator" )]
	public SkinnedModelRenderer Renderer
	{
		get => _renderer;
		set
		{
			if ( _renderer == value ) return;

			DisableAnimationEvents();

			_renderer = value;

			EnableAnimationEvents();
		}
	}

	/// <summary>
	/// If true we'll show the "create body" button
	/// </summary>
	public bool ShowCreateBodyRenderer => UseAnimatorControls && Renderer is null;

	[Button( icon: "add" )]
	[Property, Feature( "Animator" ), Tint( EditorTint.Green ), ShowIf( "ShowCreateBodyRenderer", true )]
	public void CreateBodyRenderer()
	{
		var body = new GameObject( true, "Body" );
		body.Parent = GameObject;

		Renderer = body.AddComponent<SkinnedModelRenderer>();
		Renderer.Model = Model.Load( "models/citizen/citizen.vmdl" );
	}

	[Property, Feature( "Animator" )] public float RotationAngleLimit { get; set; } = 45.0f;
	[Property, Feature( "Animator" )] public float RotationSpeed { get; set; } = 1.0f;

	[Header( "Footsteps" )]
	[Property, Feature( "Animator" )] public bool EnableFootstepSounds { get; set; } = true;
	[Property, Feature( "Animator" )] public float FootstepVolume { get; set; } = 1;


	[Property, Feature( "Animator" )] public MixerHandle FootstepMixer { get; set; }


	void EnableAnimationEvents()
	{
		if ( Renderer is null ) return;
		Renderer.OnFootstepEvent += OnFootstepEvent;
	}

	void DisableAnimationEvents()
	{
		if ( Renderer is null ) return;
		Renderer.OnFootstepEvent -= OnFootstepEvent;
	}

	/// <summary>
	/// Update the animation for this renderer. This will update the body rotation etc too.
	/// </summary>
	public void UpdateAnimation( SkinnedModelRenderer renderer )
	{
		if ( !renderer.IsValid() ) return;

		renderer.LocalPosition = bodyDuckOffset;
		bodyDuckOffset = bodyDuckOffset.LerpTo( 0, Time.Delta * 5.0f );

		UpdateAnimationParameters( renderer );
		RotateRenderBody( renderer );
	}


	float _animRotationSpeed;
	TimeSince timeSinceRotationSpeedUpdate;

	void UpdateAnimationParameters( SkinnedModelRenderer renderer )
	{
		var rot = renderer.WorldRotation;

		var skidding = 0.0f;

		if ( WishVelocity.IsNearlyZero( 0.1f ) ) skidding = Velocity.Length.Remap( 0, 1000, 0, 1 );

		// velocity
		{
			var dir = WishVelocity;
			var forward = rot.Forward.Dot( dir );
			var sideward = rot.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			renderer.Set( "move_direction", angle );
			renderer.Set( "move_speed", Velocity.Length );
			renderer.Set( "move_groundspeed", Velocity.WithZ( 0 ).Length );
			renderer.Set( "move_y", sideward );
			renderer.Set( "move_x", forward );
			renderer.Set( "move_z", Velocity.z );
		}

		renderer.SetLookDirection( "aim_eyes", EyeAngles.Forward, 1 );
		renderer.SetLookDirection( "aim_head", EyeAngles.Forward, 1 );
		renderer.SetLookDirection( "aim_body", EyeAngles.Forward, 1 );

		renderer.Set( "b_swim", IsSwimming );
		renderer.Set( "b_grounded", IsOnGround || IsClimbing );
		renderer.Set( "b_climbing", IsClimbing );
		//
		renderer.Set( "skid", skidding );
		renderer.Set( "move_style", WishVelocity.WithZ( 0 ).Length > WalkSpeed + 20 ? 2 : 1 );

		float duck = Headroom.Remap( 50, 0, 0, 0.5f, true );
		if ( IsDucking )
		{
			duck *= 3.0f;
			duck += 1.0f;
		}

		renderer.Set( "duck", duck );

		if ( timeSinceRotationSpeedUpdate > 0.1f )
		{
			timeSinceRotationSpeedUpdate = 0;
			renderer.Set( "move_rotationspeed", _animRotationSpeed * 5 );
			_animRotationSpeed = 0;
		}
	}

	float _rotationVelocity;

	void RotateRenderBody( SkinnedModelRenderer renderer )
	{
		// ladder likes to have us facing it
		if ( Mode is Sandbox.Movement.MoveModeLadder ladderMode )
		{
			renderer.WorldRotation = Rotation.Lerp( renderer.WorldRotation, ladderMode.ClimbingRotation, Time.Delta * 5.0f );
			return;
		}

		var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

		var velocity = WishVelocity.WithZ( 0 );

		float rotateDifference = renderer.WorldRotation.Distance( targetAngle );

		// We're over the limit - snap it 
		if ( rotateDifference > RotationAngleLimit )
		{
			var delta = 0.999f - (RotationAngleLimit / rotateDifference);
			var newRotation = Rotation.Lerp( renderer.WorldRotation, targetAngle, delta );

			var a = newRotation.Angles();
			var b = renderer.WorldRotation.Angles();

			var yaw = MathX.DeltaDegrees( a.yaw, b.yaw );

			_animRotationSpeed += yaw;
			_animRotationSpeed = _animRotationSpeed.Clamp( -90, 90 );

			renderer.WorldRotation = newRotation;
		}

		if ( velocity.Length > 10 )
		{
			var newRotation = Rotation.Slerp( renderer.WorldRotation, targetAngle, Time.Delta * 2.0f * RotationSpeed * velocity.Length.Remap( 0, 100 ) );

			var a = newRotation.Angles();
			var b = renderer.WorldRotation.Angles();

			var yaw = MathX.DeltaDegrees( a.yaw, b.yaw );

			_animRotationSpeed += yaw;
			_animRotationSpeed = _animRotationSpeed.Clamp( -90, 90 );

			renderer.WorldRotation = newRotation;
		}
	}

	void UpdateBodyVisibility()
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

		var go = Renderer?.GameObject ?? GameObject;

		if ( go.IsValid() )
		{
			go.Tags.Set( "viewer", viewer );
		}

	}
}
