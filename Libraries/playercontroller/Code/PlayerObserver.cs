using Sandbox;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;

public sealed class PlayerObserver : Component
{
	Angles EyeAngles;
	TimeSince timeSinceStarted;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		EyeAngles = Scene.Camera.Transform.Rotation;
		timeSinceStarted = 0;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		var corpse = Scene.GetAllComponents<PlayerCorpse>()
					.Where( x => x.Connection == Network.Owner )
					.OrderByDescending( x => x.Created )
					.FirstOrDefault();

		if ( corpse.IsValid() )
		{
			RotateAround( corpse );
		}

		if ( Input.Pressed( "jump" ) )
		{
			Respawn();
		}
	}

	[Broadcast( Permission = NetPermission.OwnerOnly )]
	public void Respawn()
	{
		if ( !Networking.IsHost ) return;
		Log.Info( $"Respawning Player {Network.OwnerId}" );

		var player = GameObject.Clone( "player.prefab" );
		player.Name = $"Player - {Network.Owner.DisplayName}";
		player.Transform.World = FindSpawnPoint();
		player.NetworkSpawn( Network.Owner );

		GameObject.Destroy();
	}

	Transform FindSpawnPoint()
	{
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}

		return global::Transform.Zero;
	}

	private void RotateAround( PlayerCorpse target )
	{
		// Find the corpse eyes

		if ( !target.Components.Get<SkinnedModelRenderer>().TryGetBoneTransform( "head", out var tx ) )
		{
			tx.Position = target.GameObject.GetBounds().Center;
		}

		var e = EyeAngles;
		e += Input.AnalogLook;
		e.pitch = e.pitch.Clamp( -90, 90 );
		e.roll = 0.0f;
		EyeAngles = e;

		var center = tx.Position;
		var targetPos = center - EyeAngles.Forward * 150.0f;

		var tr = Scene.Trace.FromTo( center, targetPos ).Radius( 1.0f ).WithoutTags( "ragdoll" ).Run();


		Scene.Camera.Transform.Position = Vector3.Lerp( Scene.Camera.Transform.Position, tr.EndPosition, timeSinceStarted, true );

		Scene.Camera.Transform.Rotation = EyeAngles;
	}
}
