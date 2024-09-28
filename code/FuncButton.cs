[Alias( "Button" )]
public sealed class FuncButton : Component, Component.IPressable
{
	public delegate Task ButtonDelegate( GameObject presser );

	[Sync] public bool State { get; set; }

	[Property] public ButtonDelegate OnButtonPressed { get; set; }
	[Property] public ButtonDelegate OnButtonReleased { get; set; }


	[Property] public SoundEvent PressSound { get; set; }
	[Property] public SoundEvent ReleaseSound { get; set; }

	[Property] public float ResetTime { get; set; } = 1.0f;

	[Property] public bool Move { get; set; }
	[Property] public Vector3 MoveDelta { get; set; }
	[Property] public Curve PressCurve { get; set; } = new Curve( new Curve.Frame( 0, 0 ), new Curve.Frame( 1, 1 ) );
	[Property] public float PressTime { get; set; } = 0.3f;
	[Property] public Curve ReleaseCurve { get; set; } = new Curve( new Curve.Frame( 0, 0 ), new Curve.Frame( 1, 1 ) );
	[Property] public float ReleaseTime { get; set; } = 0.3f;

	TimeUntil timeUntilPressable = 0;

	Vector3 initialPos;

	protected override void OnStart()
	{
		base.OnStart();

		initialPos = Transform.LocalPosition;
	}

	[Broadcast]
	public void Press( GameObject presser )
	{
		if ( presser.Network.Owner != Rpc.Caller )
			return;

		if ( timeUntilPressable > 0 )
			return;

		OnButtonPressed?.Invoke( presser );
		timeUntilPressable = ResetTime;
		Sound.Play( PressSound, Transform.Position );

		if ( IsProxy )
			return;

		Invoke( ResetTime, ResetButton );

		AnimatePositionTo( initialPos + Transform.LocalRotation * MoveDelta, PressCurve, PressTime );
	}

	public void ResetButton()
	{
		AnimatePositionTo( initialPos, ReleaseCurve, ReleaseTime );
	}

	async void AnimatePositionTo( Vector3 pos, Curve curve, float time )
	{
		float d = 0;
		Vector3 start = Transform.LocalPosition;

		while ( time > d )
		{
			var delta = d.Remap( 0, time );

			Transform.LocalPosition = Vector3.Lerp( start, pos, curve.Evaluate( delta ) );

			d += Time.Delta;

			await Task.FrameEnd();
		}

		Transform.LocalPosition = pos;
	}

	[Broadcast]
	public void Release( GameObject presser )
	{
		if ( presser.Network.Owner != Rpc.Caller )
			return;

		OnButtonReleased?.Invoke( presser );
		Sound.Play( ReleaseSound, Transform.Position );

		if ( IsProxy )
			return;
	}


	[ActionGraphNode( "example.addone" ), Pure]
	[Title( "GetPlayer" ), Group( "Examples" ), Icon( "exposure_plus_1" )]
	public static Player GetPlayer( object value = default )
	{
		if ( value is GameObject o )
		{
			return o.Components.Get<Player>();
		}

		return null;
	}

	bool IPressable.Press( IPressable.Event e )
	{
		if ( timeUntilPressable > 0 )
			return false;

		Press( e.Source.GameObject );
		return true;
	}

	void IPressable.Release( IPressable.Event e )
	{
		Release( e.Source.GameObject );
	}
}
