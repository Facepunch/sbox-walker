
using Sandbox.Diagnostics;

public sealed class PlayerInventory : Component, IPlayerEvent, ILocalPlayerEvent
{
	[RequireComponent] public Player Player { get; set; }


	public List<BaseWeapon> Weapons => Scene.Components.GetAll<BaseWeapon>( FindMode.EverythingInSelfAndDescendants ).Where( x => x.Network.OwnerId == Network.OwnerId ).OrderBy( x => x.InventorySlot ).ThenBy( x => x.InventoryOrder ).ToList();

	public BaseWeapon ActiveWeapon { get; private set; }

	public void GiveDefaultWeapons()
	{
		Pickup( "weapons/hands/hands.prefab" );
		Pickup( "weapons/camera/camera.prefab" );
		Pickup( "weapons/glock/glock.prefab" );
		Pickup( "weapons/mp5/mp5.prefab" );
	}

	void Pickup( string prefabName )
	{
		var prefab = GameObject.Clone( prefabName, new CloneConfig { Parent = GameObject, StartEnabled = false } );
		prefab.NetworkSpawn( false, Network.Owner );

		var weapon = prefab.Components.Get<BaseWeapon>( true );
		Assert.NotNull( weapon );

		IPlayerEvent.PostToGameObject( Player.GameObject, e => e.OnWeaponAdded( weapon ) );
		ILocalPlayerEvent.Post( e => e.OnWeaponAdded( weapon ) );
	}

	protected override void OnUpdate()
	{
		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.OnPlayerUpdate( Player );
		}
	}

	public void SwitchWeapon( BaseWeapon weapon )
	{
		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.GameObject.Enabled = false;
		}

		ActiveWeapon = weapon;

		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.GameObject.Enabled = true;
		}
	}

	void IPlayerEvent.OnSpawned()
	{
		GiveDefaultWeapons();
	}

	void ILocalPlayerEvent.OnCameraMove( ref Angles angles )
	{
		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.OnCameraMove( Player, ref angles );
		}
	}

	void ILocalPlayerEvent.OnCameraPostSetup( Sandbox.CameraComponent camera )
	{
		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.OnCameraSetup( Player, camera );
		}
	}
}
