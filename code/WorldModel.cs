using static BaseWeapon;

public sealed class WorldModel : Component, IWeaponEvent
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public GameObject MuzzleTransform { get; set; }
	[Property] public GameObject EjectTransform { get; set; }
	[Property] public GameObject MuzzleEffect { get; set; }
	[Property] public GameObject EjectBrass { get; set; }

	void IWeaponEvent.Attack( IWeaponEvent.AttackEvent e )
	{
		Renderer?.Set( "b_attack", true );

		if ( e.firstperson )
			return;

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
	}

	public Transform GetTracerOrigin()
	{
		if ( MuzzleTransform.IsValid() )
			return MuzzleTransform.WorldTransform;

		return WorldTransform;
	}
}
