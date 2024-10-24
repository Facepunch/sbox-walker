﻿namespace Sandbox.Movement;

/// <summary>
/// The character is walking
/// </summary>
[Icon( "scuba_diving" ), Group( "Movement" ), Title( "MoveMode - Swim" )]
public partial class MoveModeSwim : MoveMode
{
	[Property]
	public int Priority { get; set; } = 10;

	[Property, Range( 0, 1 )]
	public float SwimLevel { get; set; } = 0.7f;

	/// <summary>
	/// Will will update this based on how much you're in a "water" tagged trigger
	/// </summary>
	public float WaterLevel { get; private set; }

	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		body.LinearDamping = 3.3f;
		body.AngularDamping = 1f;
	}

	public override int Score( BodyController controller )
	{
		if ( WaterLevel > SwimLevel ) return Priority;
		return -100;
	}

	public override void OnModeEnd( MoveMode next )
	{
		// jump when leaving the water
		if ( Input.Down( "Jump" ) )
		{
			Controller.Jump( Vector3.Up * 300 );
		}
	}

	protected override void OnFixedUpdate()
	{
		UpdateWaterLevel();
	}

	void UpdateWaterLevel()
	{
		var wt = WorldTransform;
		Vector3 head = wt.PointToWorld( new Vector3( 0, 0, Controller.BodyHeight ) );
		Vector3 foot = wt.Position;

		float waterLevel = 0;

		foreach ( var touch in Controller.Body.Touching )
		{
			if ( !touch.Tags.Contains( "water" ) ) continue;

			var waterSurface = touch.FindClosestPoint( head );
			var level = Vector3.InverseLerp( waterSurface, foot, head, true );
			level = (level * 100).CeilToInt() / 100.0f;

			if ( level > waterLevel )
				waterLevel = level;
		}

		if ( WaterLevel != waterLevel )
		{
			WaterLevel = waterLevel;
		}
	}
}
