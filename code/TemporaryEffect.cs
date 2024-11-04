public sealed class TemporaryEffect : Component
{
	[Property]
	public float DestroyAfterSeconds = 1.0f;

	[Property]
	public bool WaitForChildEffects = true;

	TimeSince timeAlive;

	protected override void OnEnabled()
	{
		timeAlive = 0;
	}

	protected override void OnUpdate()
	{
		if ( WaitForChildEffects && HasActiveEffects() )
		{
			return;
		}

		if ( timeAlive > DestroyAfterSeconds )
		{
			DestroyGameObject();
		}
	}

	bool HasActiveEffects()
	{
		foreach ( var pe in GetComponentsInChildren<ITemporaryEffect>() )
		{
			if ( pe.IsActive )
				return true;
		}

		return false;
	}
}
