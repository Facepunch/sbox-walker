/// <summary>
/// Called only on the Player's GameObject
/// </summary>
public interface IPlayerEvent : ISceneEvent<IPlayerEvent>
{
	void OnSpawned() { }

	void OnJump() { }
	void OnLand( float distance, Vector3 velocity ) { }
	void OnTakeDamage( float damage ) { }
	void OnDied() { }
	void OnSuicide() { }
	void OnWeaponAdded( BaseWeapon weapon ) { }
}

/// <summary>
/// Broadcasted to everything when the local player takes actions
/// </summary>
public interface ILocalPlayerEvent : ISceneEvent<ILocalPlayerEvent>
{
	void OnJump() { }
	void OnLand( float distance, Vector3 velocity ) { }
	void OnTakeDamage( float damage ) { }
	void OnWeaponAdded( BaseWeapon weapon ) { }

	void OnCameraMove( ref Angles angles ) { }
	void OnCameraSetup( CameraComponent camera ) { }
	void OnCameraPostSetup( CameraComponent camera ) { }
}
