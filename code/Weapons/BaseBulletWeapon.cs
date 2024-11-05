public partial class BaseBulletWeapon : BaseWeapon
{
	[Property]
	public SoundEvent ShootSound { get; set; }

	[Broadcast]
	public void ShootEffects( Vector3 hitpoint, bool hit, Vector3 normal, GameObject hitObject )
	{
		var ev = new IWeaponEvent.AttackEvent( ViewModel.IsValid(), hitpoint );
		IWeaponEvent.PostToGameObject( GameObject.Root, x => x.Attack( ev ) );

		if ( ShootSound.IsValid() )
		{
			GameObject.PlaySound( ShootSound );
		}

		if ( hit )
		{
			var impact = GameObject.Clone( "weapons/common/effects/impact_default.prefab" );
			impact.WorldPosition = hitpoint + normal;
			impact.WorldRotation = Rotation.LookAt( normal );
			impact.SetParent( hitObject, true );

			var decal = GameObject.Clone( "weapons/common/effects/decal_bullet_default.prefab" );
			decal.WorldPosition = hitpoint + normal;
			decal.WorldRotation = Rotation.LookAt( -normal );
			decal.WorldScale = 1;
			decal.SetParent( hitObject, true );
		}
	}
}
