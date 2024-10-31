/// <summary>
/// Apply fall damage to the player
/// </summary>
public class PlayerFallDamage : Component, IPlayerEvent
{
	[RequireComponent] public Player Player { get; set; }

	/// <summary>
	/// Falling over this distance is considered a damaging fall
	/// </summary>
	[Property] public float MinimumFallDistance { get; set; } = 200;

	/// <summary>
	/// If you fall this distance it's death
	/// </summary>
	[Property] public float DeathFallDistance { get; set; } = 800;

	/// <summary>
	/// Multiply damage amount by this much
	/// </summary>
	[Property] public float DamageMultiplier { get; set; } = 1.0f;

	void IPlayerEvent.OnLand( float distance, Vector3 velocity )
	{
		if ( IsProxy ) return;

		var damageScale = MathX.Remap( distance, MinimumFallDistance, DeathFallDistance, 0, 1 );
		int damageAmount = (int)(damageScale * 100 * DamageMultiplier);
		if ( damageAmount < 1 ) return;

		// play smashed legs on the ground sound

		Player.TakeDamage( damageAmount );
	}
}
