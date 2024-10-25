/// <summary>
/// Responsible for taking inputs from the player and moving them.
/// </summary>
[Group( "Walker" )]
[Title( "Walker - Player Controller" )]
public sealed class PlayerController : Component, BodyController.IEvents
{
	[RequireComponent]
	public BodyController BodyController { get; set; }

	public Vector3 EyePosition => WorldPosition + Vector3.Up * EyeHeight;
	[Sync] public Vector3 WishVelocity { get; set; }

	public bool WishCrouch;
	public float EyeHeight = 64;

	void BodyController.IEvents.OnEyeAngles( ref Angles ang )
	{
		var player = Components.Get<Player>();
		var angles = ang;
		Scene.RunEvent<IPlayerEvent>( x => x.OnCameraMove( player, ref angles ) );
		ang = angles;
	}

	void BodyController.IEvents.PostCameraSetup( CameraComponent camera )
	{
		var player = Components.Get<Player>();
		IPlayerEvent.Post( x => x.OnCameraSetup( player, camera ) );
		IPlayerEvent.Post( x => x.OnCameraPostSetup( player, camera ) );
	}

	void BodyController.IEvents.OnLanded( float distance, Vector3 impactVelocity )
	{
		var player = Components.Get<Player>();
		IPlayerEvent.Post( x => x.OnLand( player, distance, impactVelocity ) );
	}
}
