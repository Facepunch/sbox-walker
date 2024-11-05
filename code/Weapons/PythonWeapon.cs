public class PythonWeapon : BaseBulletWeapon
{
	TimeUntil shootAllowed = 0;

	[Property]
	public float Damage { get; set; } = 12.0f;

	public override void OnControl( Player player )
	{
		base.OnControl( player );

		if ( shootAllowed > 0 )
			return;

		if ( Input.Pressed( "attack1" ) )
		{
			shootAllowed = 1.2f;
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
		var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.Run();

		ShootEffects( tr.EndPosition, tr.Hit, tr.Normal, tr.GameObject );

		//DebugOverlay.Line( GetTracerOrigin().Position, tr.EndPosition, duration: 30 );

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -2, -3 ), Random.Shared.Float( -2, 2 ), 0 );


		if ( !player.Controller.ThirdPerson )
		{
			foreach ( var cameffect in Scene.GetAll<PlayerCameraEffects>() )
			{
				cameffect.AddPunch( new Vector3( Random.Shared.Float( -50, -65 ), Random.Shared.Float( -20, 0 ), 0 ), 4.0f, 1, 0.5f );
				cameffect.AddShake( 1.5f, 1.0f );
			}
		}
	}

}
