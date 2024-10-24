﻿namespace Sandbox.Movement;

/// <summary>
/// The character is climbing up a ladder
/// </summary>
[Icon( "hiking" ), Group( "Movement" ), Title( "MoveMode - Ladder" )]
public partial class MoveModeLadder : MoveMode
{
	[Property]
	public int Priority { get; set; } = 5;

	/// <summary>
	/// A list of tags we can climb up - when they're on triggers
	/// </summary>
	[Property]
	public TagSet ClimbableTags { get; set; }

	/// <summary>
	/// The GameObject we're climbing. This will usually be a ladder trigger.
	/// </summary>
	public GameObject ClimbingObject { get; set; }

	/// <summary>
	/// When climbing, this is the rotation of the wall/ladder you're climbing, where
	/// Forward is the direction to look at the ladder, and Up is the direction to climb.
	/// </summary>
	public Rotation ClimbingRotation { get; set; }


	public MoveModeLadder()
	{
		ClimbableTags = new TagSet();
		ClimbableTags.Add( "ladder" );
	}

	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		body.LinearDamping = 10.0f;
		body.AngularDamping = 1f;
	}

	public override int Score( BodyController controller )
	{
		if ( ClimbingObject.IsValid() ) return Priority;
		return -100;
	}

	public override void PostPhysicsStep()
	{
		UpdatePositionOnLadder();
	}


	void UpdatePositionOnLadder()
	{
		if ( !ClimbingObject.IsValid() ) return;

		var pos = Controller.WorldPosition;

		// work out ideal position
		var ladderPos = ClimbingObject.WorldPosition;
		var ladderUp = ClimbingObject.WorldRotation.Up;

		Line ladderLine = new Line( ladderPos - ladderUp * 1000, ladderPos + ladderUp * 1000 );

		var idealPos = ladderLine.ClosestPoint( pos );

		// Get just the left/right
		var delta = (idealPos - pos);
		delta = delta.SubtractDirection( ClimbingObject.WorldRotation.Forward );

		if ( delta.Length > 0.01f )
		{
			Controller.Body.Velocity = Controller.Body.Velocity.AddClamped( delta * 5.0f, delta.Length * 10.0f );
		}
	}

	protected override void OnFixedUpdate()
	{
		ScanForLadders();
	}

	void ScanForLadders()
	{
		var wt = WorldTransform;
		Vector3 head = wt.PointToWorld( new Vector3( 0, 0, Controller.BodyHeight ) );
		Vector3 foot = wt.Position;

		GameObject ladderObject = default;

		foreach ( var touch in Controller.Body.Touching )
		{
			if ( !touch.Tags.HasAny( ClimbableTags ) )
				continue;

			// already on it, no need to do any checks
			if ( ClimbingObject == touch.GameObject )
			{
				ladderObject = touch.GameObject;
				continue;
			}

			// Don't start climbing this ladder if it's below us, and we're not already climbing it

			var ladderSurface = touch.FindClosestPoint( head );
			var level = Vector3.InverseLerp( ladderSurface, foot, head, true );


			if ( ClimbingObject != touch.GameObject && level < 0.5f )
				continue;

			ladderObject = touch.GameObject;
			break;

		}

		if ( ladderObject == ClimbingObject )
			return;

		ClimbingObject = ladderObject;

		if ( ClimbingObject.IsValid() )
		{
			// work out rotation to the ladder. We could be climbing up the front or back of this thing.

			var directionToLadder = ClimbingObject.WorldPosition - WorldPosition;

			ClimbingRotation = ClimbingObject.WorldRotation;

			if ( directionToLadder.Dot( ClimbingRotation.Forward ) < 0 )
			{
				ClimbingRotation *= new Angles( 0, 180, 0 );
			}
		}
	}
}
