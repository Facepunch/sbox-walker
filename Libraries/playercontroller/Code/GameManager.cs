public sealed class GameManager : GameObjectSystem<GameManager>, IPlayerEvent, Component.INetworkListener, ISceneStartup
{
	public GameManager( Scene scene ) : base( scene )
	{
	}

	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{
		Log.Info( $"Walker: Loading scene {scene.ResourceName}" );
	}

	void ISceneStartup.OnHostInitialize()
	{
		//
		// TODO: We don't have a menu, but if we did we could put a special component in the menu
		// scene that we'd now be able to detect, and skip doing the stuff below.
		//

		//
		// Spawn the engine scene.
		// This scene is sent to clients when they join.
		//
		var slo = new SceneLoadOptions();
		slo.IsAdditive = true;
		slo.SetScene( "scenes/engine.scene" );
		Scene.Load( slo );

		// If we're not hosting a lobby, start hosting one
		// so that people can join this game.
		Networking.CreateLobby();
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		SpawnPlayerForConnection( channel );
	}

	public void SpawnPlayerForConnection( Connection channel )
	{
		// Find a spawn location for this player
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var playerGo = GameObject.Clone( "/player.prefab", new CloneConfig { Name = $"Player - {channel.DisplayName}", StartEnabled = true, Transform = startLocation } );
		var player = playerGo.Components.Get<Player>( true );
		playerGo.NetworkSpawn( channel );

		IPlayerEvent.Post( x => x.OnSpawned( player ) );
	}


	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}

		//
		// Failing that, spawn where we are
		//
		return Transform.Zero;
	}
}
