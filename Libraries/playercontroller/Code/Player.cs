using static Sandbox.Component;

/// <summary>
/// Holds player information like health
/// </summary>
public sealed class Player : Component, IDamageable
{
	public static Player FindLocalPlayer()
	{
		return Game.ActiveScene.GetAllComponents<Player>().Where( x => !x.IsProxy ).FirstOrDefault();
	}

	[RequireComponent]
	public PlayerController PlayerController { get; set; }

	[Property]
	public GameObject Body { get; set; }

	[Property, Range( 0, 100 )]
	[Sync] public float Health { get; set; } = 100;

	public bool IsDead => Health <= 0;

	public Transform EyeTransform
	{
		get
		{
			return Scene.Camera.WorldTransform;

			//var tx = new Transform( PlayerController.EyePosition );
			//tx.Rotation = PlayerController.EyeAngles;
			//	return tx;
		}
	}

	/// <summary>
	/// Creates a ragdoll but it isn't enabled
	/// </summary>
	[Broadcast]
	void CreateRagdoll()
	{
		var ragdoll = GetComponent<BodyController>().CreateRagdoll();
		if ( !ragdoll.IsValid() ) return;

		var corpse = ragdoll.AddComponent<PlayerCorpse>();
		corpse.Connection = Network.Owner;
		corpse.Created = DateTime.Now;
	}

	[Broadcast( NetPermission.OwnerOnly )]
	void CreateRagdollAndGhost()
	{
		if ( !Networking.IsHost ) return;

		var go = new GameObject( false, "Observer" );
		go.Components.Create<PlayerObserver>();
		go.NetworkSpawn( Rpc.Caller );
	}

	public void TakeDamage( float amount )
	{
		if ( IsProxy ) return;
		if ( Health < 0 ) return;

		Health -= amount;

		IPlayerEvent.Post( x => x.OnTakeDamage( this, amount ) );

		if ( Health < 0 )
		{
			Health = 0;
			Death();
		}
	}

	void Death()
	{
		CreateRagdoll();
		CreateRagdollAndGhost();

		IPlayerEvent.Post( x => x.OnDied( this ) );

		GameObject.Destroy();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsProxy )
			OnControl();
	}

	void OnControl()
	{
		if ( Input.Pressed( "die" ) )
		{
			IPlayerEvent.Post( x => x.OnSuicide( this ) );
			Health = 0;
			Death();
		}
	}

	void IDamageable.OnDamage( in DamageInfo damage )
	{
		TakeDamage( damage.Damage );
	}
}
