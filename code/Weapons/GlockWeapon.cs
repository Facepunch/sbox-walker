public class GlockWeapon : BaseWeapon
{
	TimeUntil shootAllowed = 0;

	public override void OnControl( Player player )
	{
		base.OnControl( player );

		if ( shootAllowed > 0 )
			return;

		if ( Input.Pressed( "attack1" ) )
		{
			shootAllowed = 0.15f;
			ShootBullet( player );
		}

		if ( Input.Down( "attack2" ) )
		{
			shootAllowed = 0.45f;
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
		var ev = new IWeaponEvent.AttackEvent( ViewModel.IsValid() );
		IWeaponEvent.PostToGameObject( GameObject.Root, x => x.Attack( ev ) );

		var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.Run();

		DebugOverlay.Line( GetTracerOrigin().Position, tr.EndPosition, duration: 30 );

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -1, -2 ), Random.Shared.Float( -1, 1 ), 0 );
	}
}
