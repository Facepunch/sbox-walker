using Sandbox.Diagnostics;
using Sandbox.Utility;

public class PlayerCameraEffects : Component, IPlayerEvent, ILocalPlayerEvent
{
	[RequireComponent]
	public Player Player { get; set; }

	List<BaseCameraShake> effects = new();

	protected override void OnUpdate()
	{
		foreach ( var effect in effects )
		{
			effect.Update();
		}

		effects.RemoveAll( x => x.IsDone );
	}

	void IPlayerEvent.OnJump()
	{
		if ( IsProxy ) return;

		var punch = new CameraPunch( new Vector3( -20, 0, 0 ), 0.5f, 2.0f, 1.0f );
		effects.Add( punch );
	}

	public void AddPunch( Vector3 target, float time, float frequency, float damp )
	{
		var punch = new CameraPunch( target, time, frequency, damp );
		effects.Add( punch );
	}

	public void AddShake( float amount, float time )
	{
		var punch = new CameraShake( amount, time );
		effects.Add( punch );
	}

	void IPlayerEvent.OnLand( float distance, Vector3 velocity )
	{
		if ( IsProxy ) return;
		if ( Player.Controller.ThirdPerson ) return;

		var punch = new CameraPunch( new Vector3( 0.3f * distance, Random.Shared.Float( -1, 1 ), Random.Shared.Float( -1, 1 ) ), 1.0f, 1.5f, 0.7f );
		effects.Add( punch );
	}

	void ILocalPlayerEvent.OnCameraPostSetup( Sandbox.CameraComponent camera )
	{
		if ( IsProxy ) return;

		foreach ( var effect in effects )
		{
			effect.ModifyCamera( camera );
		}

		MovementEffects( camera );
	}

	float roll;

	private void MovementEffects( CameraComponent camera )
	{
		if ( Player.Controller.ThirdPerson ) return;

		Assert.NotNull( Player );
		Assert.NotNull( Player.Controller );

		//if ( pc.BodyController.IsOnGround )
		//{
		//	distance += pc.BodyController.Velocity.Length * Time.Delta;
		//}

		var scaler = Player.Controller.WishVelocity.Length.Remap( 0, Player.Controller.RunSpeed, 0, 1 );

		// bob
		// undone: made me feel sick
		//camera.Transform.Position += Vector3.Up * scaler * MathF.Sin( distance * 0.06f ) * 0.2f;

		// side movement
		var r = Player.Controller.WishVelocity.Dot( Player.EyeTransform.Left ) / -100.0f;
		roll = MathX.Lerp( roll, r, Time.Delta * 8.0f, true );

		camera.WorldRotation *= new Angles( 0, 0, roll );
	}

	abstract class BaseCameraShake
	{
		public virtual void Update() { }
		public virtual bool IsDone => true;
		public abstract void ModifyCamera( CameraComponent cc );
	}

	class CameraPunch : BaseCameraShake
	{
		float deathTime;
		float lifeTime = 0.0f;

		public CameraPunch( Vector3 target, float time, float frequency, float damp )
		{
			damping.Current = target;
			damping.Target = 0;
			damping.SmoothTime = time;
			damping.Frequency = frequency;
			damping.Damping = damp;

			deathTime = damping.SmoothTime;
		}

		public override bool IsDone => deathTime <= 0;

		Vector3.SpringDamped damping;

		public override void Update()
		{
			deathTime -= Time.Delta;
			lifeTime += Time.Delta;

			damping.Update( Time.Delta );
		}

		public override void ModifyCamera( CameraComponent cc )
		{
			var amount = lifeTime.Remap( 0, 0.3f, 0, 1 );

			cc.WorldRotation *= new Angles( damping.Current * amount );
		}
	}

	class CameraShake : BaseCameraShake
	{
		float lifeTime;
		float deathTime;
		float amount = 0.0f;

		public CameraShake( float amount, float time )
		{
			this.amount = amount;
			deathTime = time;
			lifeTime = time;
		}

		public override bool IsDone => deathTime <= 0;

		Vector3.SpringDamped damping;

		public override void Update()
		{
			deathTime -= Time.Delta;
			damping.Update( Time.Delta );
		}

		public override void ModifyCamera( CameraComponent cc )
		{
			var x = Noise.Perlin( Time.Now * 1000.0f, 2345 );
			var y = Noise.Perlin( Time.Now * 1000.0f, 21 );
			var z = Noise.Perlin( Time.Now * 1000.0f, 865 );

			var delta = MathX.Remap( deathTime, 0, lifeTime, 0, 1 );

			cc.WorldRotation *= new Angles( x, y, z ) * delta * amount;
		}
	}

}
