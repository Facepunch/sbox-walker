public sealed class GameManager : GameObjectSystem<GameManager>, IPlayerEvent, Component.INetworkListener
{
	public GameManager( Scene scene ) : base( scene )
	{
		Listen( Stage.SceneLoaded, 0, OnSceneLoaded, "SceneLoaded" );
	}

	void OnSceneLoaded()
	{
		if ( !Networking.IsActive )
		{
			Networking.CreateLobby();
		}
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
