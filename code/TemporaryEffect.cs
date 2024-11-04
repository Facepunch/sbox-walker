public sealed class TemporaryEffect : Component
{
	[Property]
	public float DestroyAfterSeconds = 1.0f;

	[Property]
	public bool WaitForParticleSystems = true;

	TimeSince timeAlive;

	protected override void OnEnabled()
	{
		timeAlive = 0;
	}

	protected override void OnUpdate()
	{
		if ( WaitForParticleSystems && HasActiveParticles() )
		{
			return;
		}

		if ( timeAlive > DestroyAfterSeconds )
		{
			DestroyGameObject();
		}
	}

	bool HasActiveParticles()
	{
		foreach ( var pe in GetComponentsInChildren<ParticleEffect>() )
		{
			if ( pe.ParticleCount > 0 )
				return true;
		}

		return false;
	}
}
