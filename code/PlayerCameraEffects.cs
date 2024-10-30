using Sandbox.Diagnostics;

public class PlayerCameraEffects : Component, IPlayerEvent
{
	List<BaseCameraShake> effects = new();

	protected override void OnUpdate()
	{
		foreach ( var effect in effects )
		{
			effect.Update();
		}

		effects.RemoveAll( x => x.IsDone );
	}

	void IPlayerEvent.OnJump( Player player )
	{
		if ( player.IsProxy ) return;
		if ( player.Controller.ThirdPerson ) return;

		var punch = new CameraPunch( new Vector3( -20, 0, 0 ), 0.5f, 2.0f, 1.0f );
		effects.Add( punch );
	}

	void IPlayerEvent.OnLand( Player player, float distance, Vector3 velocity )
	{
		if ( player.IsProxy ) return;
		if ( player.Controller.ThirdPerson ) return;

		var punch = new CameraPunch( new Vector3( 0.3f * distance, Random.Shared.Float( -1, 1 ), Random.Shared.Float( -1, 1 ) ), 1.0f, 1.5f, 0.7f );
		effects.Add( punch );
	}

	void IPlayerEvent.OnCameraPostSetup( Player player, Sandbox.CameraComponent camera )
	{
		if ( player.IsProxy ) return;

		foreach ( var effect in effects )
		{
			effect.ModifyCamera( camera );
		}

		MovementEffects( player, camera );
	}

	float roll;

	private void MovementEffects( Player player, CameraComponent camera )
	{
		if ( player.Controller.ThirdPerson ) return;

		Assert.NotNull( player );
		Assert.NotNull( player.Controller );

		//if ( pc.BodyController.IsOnGround )
		//{
		//	distance += pc.BodyController.Velocity.Length * Time.Delta;
		//}

		var scaler = player.Controller.WishVelocity.Length.Remap( 0, player.Controller.RunSpeed, 0, 1 );

		// bob
		// undone: made me feel sick
		//camera.Transform.Position += Vector3.Up * scaler * MathF.Sin( distance * 0.06f ) * 0.2f;

		// side movement
		var r = player.Controller.WishVelocity.Dot( player.EyeTransform.Left ) / -100.0f;
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

}
