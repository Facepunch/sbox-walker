/// <summary>
/// Holds player information like health
/// </summary>
public interface IPlayerEvent : ISceneEvent<IPlayerEvent>
{
	void OnJump( Player player ) { }
	void OnLand( Player player, float distance, Vector3 velocity ) { }
	void OnTakeDamage( Player player, float damage ) { }
	void OnDied( Player player ) { }
}
