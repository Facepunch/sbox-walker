public class Mp5Weapon : BaseWeapon
{
	[Property]
	public float TimeBetweenShots { get; set; } = 0.1f;

	[Property]
	public Vector2 AimCone { get; set; } = 0.1f;

	TimeUntil shootAllowed = 0;

	public override void OnControl( Player player )
	{
		base.OnControl( player );

		if ( shootAllowed > 0 )
			return;

		if ( Input.Down( "attack1" ) )
		{
			shootAllowed = TimeBetweenShots;
			ShootBullet( player );
		}
	}

	public Transform GetTracerOrigin()
	{
		if ( ViewModel.IsValid() )
		{
			var vm = ViewModel.GetComponent<ViewModel>();
			if ( vm.IsValid() )
			{
				return vm.GetTracerOrigin();
			}
		}

		if ( WorldModel.IsValid() )
		{
			var wm = WorldModel.GetComponent<WorldModel>();
			if ( wm.IsValid() )
			{
				return wm.GetTracerOrigin();
			}
		}

		var pc = GetComponentInParent<PlayerController>();
		if ( pc.IsValid() )
			return pc.EyeTransform;

		return WorldTransform;
	}

	public void ShootBullet( Player player )
	{
		var forward = player.EyeTransform.Rotation.Forward;

		forward += AimCone.x * player.EyeTransform.Rotation.Right * Random.Shared.Float( -1, 1 ) * 0.1f;
		forward += AimCone.y * player.EyeTransform.Rotation.Up * Random.Shared.Float( -1, 1 ) * 0.1f;

		forward = forward.Normal;

		var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay with { Forward = forward }, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.Run();

		ShootEffects( tr.EndPosition );

		if ( tr.Hit )
		{
			var impact = GameObject.Clone( "weapons/common/effects/impact_default.prefab" );
			impact.WorldPosition = tr.EndPosition + tr.Normal;
			impact.WorldRotation = Rotation.LookAt( tr.Normal );
			impact.SetParent( tr.GameObject, true );

			var decal = GameObject.Clone( "weapons/common/effects/decal_bullet_default.prefab" );
			decal.WorldPosition = tr.EndPosition + tr.Normal;
			decal.WorldRotation = Rotation.LookAt( -tr.Normal );
			decal.WorldScale = 1;
			decal.SetParent( tr.GameObject, true );
		}

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -0.2f, -0.3f ), Random.Shared.Float( -0.1f, 0.1f ), 0 );

		if ( !player.Controller.ThirdPerson )
		{
			foreach ( var cameffect in Scene.GetAll<PlayerCameraEffects>() )
			{
				cameffect.AddPunch( new Vector3( Random.Shared.Float( -10, -15 ), Random.Shared.Float( -10, 0 ), 0 ), 1.0f, 3, 0.5f );
				cameffect.AddShake( 0.3f, 1.2f );
			}
		}
	}

	[Broadcast]
	public void ShootEffects( Vector3 hitpoint )
	{
		var ev = new IWeaponEvent.AttackEvent( ViewModel.IsValid(), hitpoint );
		IWeaponEvent.PostToGameObject( GameObject.Root, x => x.Attack( ev ) );
	}
}
