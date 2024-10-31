/// <summary>
/// Deals damage to objects inside
/// </summary>
[Category( "Gameplay" ), Icon( "medical_services" ), EditorHandle( Icon = "🤕" )]
public sealed class TriggerHurt : Component
{
	public delegate Task TriggerDelegate( Collider collider, GameObject gameobject );

	[RequireComponent]
	public Collider Collider { get; set; }

	/// <summary>
	/// If not empty, the target must have one of these tags
	/// </summary>
	[Property, Group( "Damage" )] public TagSet DamageTags { get; set; } = new();

	/// <summary>
	/// How much damage to apply
	/// </summary>
	[Property, Group( "Damage" )] public float Damage { get; set; } = 10.0f;

	/// <summary>
	/// The delay between applying the damage
	/// </summary>
	[Property, Group( "Damage" )] public float Rate { get; set; } = 1.0f;

	/// <summary>
	/// If not empty, the target must have one of these tags
	/// </summary>
	[Property, Group( "Target" )] public TagSet Include { get; set; } = new();

	/// <summary>
	/// If not empty, the target must not have one of these tags
	/// </summary>
	[Property, Group( "Target" )] public TagSet Exclude { get; set; } = new();

	TimeSince timeSinceDamage = 0.0f;

	protected override void OnFixedUpdate()
	{
		if ( timeSinceDamage < Rate )
			return;

		timeSinceDamage = 0;

		var damageInfo = new DamageInfo( Damage, GameObject, GameObject );
		damageInfo.Tags.Add( DamageTags );

		foreach ( var touching in Collider.Touching.SelectMany( x => x.GetComponentsInParent<IDamageable>().Distinct() ) )
		{
			var target = touching as Component;

			if ( !Exclude.IsEmpty && target.GameObject.Tags.HasAny( Exclude ) ) continue;
			if ( !Include.IsEmpty && !target.GameObject.Tags.HasAny( Include ) ) continue;

			touching.OnDamage( damageInfo );

		}
	}
}
