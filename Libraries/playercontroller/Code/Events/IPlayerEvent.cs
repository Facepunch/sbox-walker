/// <summary>
/// Holds player information like health
/// </summary>
public interface IPlayerEvent : ISceneEvent<IPlayerEvent>
{
	void OnSpawned( Player player ) { }

	void OnJump( Player player ) { }
	void OnLand( Player player, float distance, Vector3 velocity ) { }
	void OnTakeDamage( Player player, float damage ) { }
	void OnDied( Player player ) { }
	void OnSuicide( Player player ) { }
	void OnWeaponAdded( Player player, BaseWeapon weapon ) { }
	void OnWeaponDropped( Player player, BaseWeapon weapon ) { }

	void OnCameraMove( Player player, ref Angles angles ) { }
	void OnCameraSetup( Player player, CameraComponent camera ) { }
	void OnCameraPostSetup( Player player, CameraComponent camera ) { }
}
