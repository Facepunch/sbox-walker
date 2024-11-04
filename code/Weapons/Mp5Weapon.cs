public class Mp5Weapon : BaseWeapon
{
	public override void OnControl( Player player )
	{
		base.OnControl( player );

		if ( Input.Down( "attack1" ) )
		{
			// ATTACK
		}
	}
}
