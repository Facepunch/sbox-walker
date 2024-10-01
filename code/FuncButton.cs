[Alias( "Button" ), EditorHandle( Icon = "touch_app" )]
public sealed class FuncButton : Component, Component.IPressable
{
	public delegate Task ButtonDelegate( GameObject presser );
	public delegate Task ButtonToggleDelegate( bool state );

	[Sync] public bool State { get; set; }

	[Property] public ButtonDelegate OnButtonPressed { get; set; }
	[Property] public ButtonDelegate OnButtonReleased { get; set; }
	[Property] public ButtonToggleDelegate OnStateChanged { get; set; }

	[Property, Group( "Opening" ), Order( 1 )] public Action OnOpenStart { get; set; }
	[Property, Group( "Opening" ), Order( 1 )] public Action OnOpenEnd { get; set; }
	[Property, Group( "Opening" ), Order( 1 )] public float OpenDuration { get; set; } = 1.0f;
	[Property, Group( "Opening" ), Order( 1 )] public Curve OpenMovementCurve { get; set; } = new Curve( new Curve.Frame( 0, 0 ), new Curve.Frame( 1, 1 ) );

	[Property, Group( "Closing" ), Order( 2 )] public Action OnCloseStart { get; set; }
	[Property, Group( "Closing" )] public Action OnCloseEnd { get; set; }
	[Property, Group( "Closing" )] public float CloseDuration { get; set; } = 1.0f;
	[Property, Group( "Closing" )] public Curve CloseMovementCurve { get; set; } = new Curve( new Curve.Frame( 0, 0 ), new Curve.Frame( 1, 1 ) );

	[Property] public bool AutoReset { get; set; } = true;
	[Property, ShowIf( "AutoReset", true )] public float ResetTime { get; set; } = 1.0f;

	[Property, Group( "Movement" ), Order( 0 )] public bool Move { get; set; }
	[Property, Group( "Movement" )] public Vector3 MoveDelta { get; set; }

	Vector3 initialPos;

	[Sync] public bool IsMoving { get; set; }

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

		if ( IsMoving )
			return;

		OnButtonPressed?.Invoke( presser );

		if ( IsProxy )
			return;

		Open();
	}

	[Broadcast]
	void StateChanged( bool state )
	{
		OnStateChanged?.Invoke( state );
	}

	async void Open()
	{
		OnOpenStart?.Invoke();
		IsMoving = true;

		await AnimatePositionTo( initialPos + Transform.LocalRotation * MoveDelta, OpenMovementCurve, OpenDuration );

		StateChanged( true );
		OnOpenEnd?.Invoke();

		if ( AutoReset && ResetTime >= 0.0f )
		{
			await Task.DelaySeconds( ResetTime );
			Close();
		}

		IsMoving = false;
	}

	async void Close()
	{
		OnCloseStart?.Invoke();

		IsMoving = true;

		await AnimatePositionTo( initialPos, CloseMovementCurve, CloseDuration );

		StateChanged( false );
		IsMoving = false;

		OnCloseEnd?.Invoke();
	}

	async Task AnimatePositionTo( Vector3 pos, Curve curve, float time )
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
		if ( IsMoving )
			return false;

		Press( e.Source.GameObject );
		return true;
	}

	void IPressable.Release( IPressable.Event e )
	{
		Release( e.Source.GameObject );
	}

	bool IPressable.CanPress( Sandbox.Component.IPressable.Event e )
	{
		if ( IsMoving ) return false;
		return true;
	}
}
