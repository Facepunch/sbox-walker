using static BaseWeapon;

public sealed class WorldModel : WeaponModel, IWeaponEvent
{
	void IWeaponEvent.Attack( IWeaponEvent.AttackEvent e )
	{
		Renderer?.Set( "b_attack", true );

		if ( e.firstperson )
			return;

		DoMuzzleEffect();
		DoEjectBrass();
		DoTracerEffect( e.HitPoint );
	}


}
