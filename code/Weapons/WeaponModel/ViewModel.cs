using static BaseWeapon;

public sealed class ViewModel : WeaponModel, IWeaponEvent
{
	protected override void OnUpdate()
	{
		WorldPosition = Scene.Camera.WorldPosition;
		WorldRotation = Scene.Camera.WorldRotation;

		UpdateAnimation();
	}

	void UpdateAnimation()
	{
		var playerController = GetComponentInParent<PlayerController>();
		if ( playerController is null ) return;

		Renderer.Set( "b_grounded", playerController.IsOnGround );
		Renderer.Set( "move_bob", playerController.Velocity.Length.Remap( 0, playerController.RunSpeed ) );
	}

	void IWeaponEvent.Attack( IWeaponEvent.AttackEvent e )
	{
		Renderer?.Set( "b_attack", true );

		DoMuzzleEffect();
		DoEjectBrass();
		DoTracerEffect( e.HitPoint );
	}
}
