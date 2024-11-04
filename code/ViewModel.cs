using static BaseWeapon;

public sealed class ViewModel : Component, IWeaponEvent
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public GameObject MuzzleTransform { get; set; }
	[Property] public GameObject EjectTransform { get; set; }
	[Property] public GameObject MuzzleEffect { get; set; }
	[Property] public GameObject EjectBrass { get; set; }

	protected override void OnUpdate()
	{
		WorldPosition = Scene.Camera.WorldPosition;
		WorldRotation = Scene.Camera.WorldRotation;

		UpdateAnimation();
	}

	void UpdateAnimation()
	{
		var playerController = GetComponentInParent<PlayerController>();
		if ( playerController is null ) return;

		Renderer.Set( "b_grounded", playerController.IsOnGround );
		Renderer.Set( "move_bob", playerController.Velocity.Length.Remap( 0, playerController.RunSpeed ) );
	}

	void IWeaponEvent.Attack( IWeaponEvent.AttackEvent e )
	{
		Renderer?.Set( "b_attack", true );

		if ( MuzzleEffect.IsValid() )
		{
			MuzzleEffect.Clone( new CloneConfig { Parent = MuzzleTransform, Transform = global::Transform.Zero, StartEnabled = true } );
		}

		if ( EjectBrass.IsValid() && EjectTransform.IsValid() )
		{
			var effect = EjectBrass.Clone( new CloneConfig { Transform = EjectTransform.WorldTransform.WithScale( 1 ), StartEnabled = true } );
			effect.WorldRotation = effect.WorldRotation * new Angles( 90, 0, 0 );

			var ejectDirection = (EjectTransform.WorldRotation.Forward * 2 + EjectTransform.WorldRotation.Right + Vector3.Random * 0.55f).Normal;

			effect.GetComponentInChildren<Rigidbody>().Velocity = ejectDirection * 500;
		}

		foreach ( var cameffect in Scene.GetAll<PlayerCameraEffects>() )
		{
			cameffect.AddPunch( new Vector3( Random.Shared.Float( -20, -25 ), Random.Shared.Float( -20, 0 ), 0 ), 1.0f, 3, 1.0f );
			cameffect.AddShake( 0.5f, 1.0f );
		}
	}

	public Transform GetTracerOrigin()
	{
		if ( MuzzleTransform.IsValid() )
			return MuzzleTransform.WorldTransform;

		return WorldTransform;
	}
}
