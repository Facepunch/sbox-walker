[Group( "Walker" )]
[Title( "Walker - Player Dresser" )]
public sealed class PlayerDresser : Component, Component.INetworkSpawn
{
	[Property]
	public SkinnedModelRenderer BodyRenderer { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( owner.GetUserData( "avatar" ) );
		clothing.Apply( BodyRenderer );
	}
}

public record struct SpringDampVector3( Vector3 Current, Vector3 Target, float SmoothTime, float Frequency = 2.0f, float Damping = 0.5f )
{
	public Vector3 Velocity;

	public void Update( float timeDelta )
	{
		Current = SpringDamp( Current, Target, ref Velocity, SmoothTime, timeDelta, Frequency, Damping );
	}

	public static Vector3 SpringDamp( Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float deltaTime, float frequency = 2.0f, float damping = 0.5f )
	{
		if ( smoothTime <= 0.0f ) return target;
		if ( deltaTime <= 0.0f ) return current;

		// Angular frequency (how fast the spring oscillates)
		float omega = frequency * MathF.PI * 2.0f;

		// Damping factor to control how much oscillation decays over time
		float dampingFactor = damping * omega;

		// Compute the velocity using spring physics
		Vector3 force = omega * omega * (target - current) - 2.0f * dampingFactor * velocity;
		velocity += force * deltaTime;

		// Update position
		return current + velocity * deltaTime;
	}
}

public record struct SmoothDampVector3( Vector3 Current, Vector3 Target, float SmoothTime )
{
	public Vector3 Velocity;

	public void Update( float timeDelta )
	{
		Current = Vector3.SmoothDamp( Current, Target, ref Velocity, SmoothTime, timeDelta );
	}
}

