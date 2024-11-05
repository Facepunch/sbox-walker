﻿public abstract class WeaponModel : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public GameObject MuzzleTransform { get; set; }
	[Property] public GameObject EjectTransform { get; set; }
	[Property] public GameObject MuzzleEffect { get; set; }
	[Property] public GameObject EjectBrass { get; set; }
	[Property] public GameObject TracerEffect { get; set; }

	public void Deploy()
	{
		Renderer?.Set( "b_deploy", true );
	}

	public Transform GetTracerOrigin()
	{
		if ( MuzzleTransform.IsValid() )
			return MuzzleTransform.WorldTransform;

		return WorldTransform;
	}

	public void DoTracerEffect( Vector3 hitPoint )
	{
		if ( !TracerEffect.IsValid() ) return;

		var effect = TracerEffect.Clone( new CloneConfig { Transform = GetTracerOrigin().WithScale( 1 ), StartEnabled = true } );

		if ( effect.GetComponentInChildren<Tracer>() is Tracer tracer )
		{
			tracer.EndPoint = hitPoint;
		}
	}

	public void DoEjectBrass()
	{
		if ( !EjectBrass.IsValid() ) return;
		if ( !EjectTransform.IsValid() ) return;

		var effect = EjectBrass.Clone( new CloneConfig { Transform = EjectTransform.WorldTransform.WithScale( 1 ), StartEnabled = true } );
		effect.WorldRotation = effect.WorldRotation * new Angles( 90, 0, 0 );

		var ejectDirection = (EjectTransform.WorldRotation.Forward * 2 + EjectTransform.WorldRotation.Right + Vector3.Random * 0.55f).Normal;

		effect.GetComponentInChildren<Rigidbody>().Velocity = ejectDirection * 500;
	}

	public void DoMuzzleEffect()
	{
		if ( !MuzzleEffect.IsValid() ) return;
		if ( !MuzzleTransform.IsValid() ) return;

		MuzzleEffect.Clone( new CloneConfig { Parent = MuzzleTransform, Transform = global::Transform.Zero, StartEnabled = true } );
	}
}
