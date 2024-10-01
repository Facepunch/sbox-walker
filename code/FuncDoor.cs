[EditorHandle( Icon = "touch_app" )]
public sealed class FuncDoor : Component, Component.IPressable
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

	[Property, Group( "Movement" )] public Vector3 Pivot { get; set; }
	[Property, Group( "Movement" )] public Angles TargetRotation { get; set; } = new Angles( 0, 90, 0 );

	Transform initialTransform;

	[Sync] public bool IsMoving { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		initialTransform = LocalTransform;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		using ( Gizmo.Scope( "Tool", new Transform( Pivot ) ) )
		{
			if ( Gizmo.Control.Position( "pivot", 0, out var newPivot ) )
			{
				Pivot += newPivot;
			}
		}

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

		if ( State )
		{
			if ( !AutoReset ) Close();
			return;
		}

		Open();
	}

	[Broadcast]
	void StateChanged( bool state )
	{
		OnStateChanged?.Invoke( state );
		State = state;
	}

	async void Open()
	{
		OnOpenStart?.Invoke();
		IsMoving = true;

		await AnimateRotationTo( TargetRotation, OpenMovementCurve, OpenDuration );

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

		await AnimateRotationTo( Rotation.Identity, CloseMovementCurve, CloseDuration );

		StateChanged( false );
		IsMoving = false;

		OnCloseEnd?.Invoke();
	}

	async Task AnimateRotationTo( Rotation rot, Curve curve, float time )
	{
		float d = 0;
		var start = LocalTransform;
		var localPivot = start.PointToWorld( Pivot );

		var targetTx = initialTransform.RotateAround( LocalTransform.PointToWorld( Pivot ), rot );

		var targetRot = initialTransform.Rotation * rot;

		//rot = rot * start.Rotation.Inverse;

		while ( time > d )
		{
			var delta = d.Remap( 0, time );

			var t = curve.Evaluate( delta );

			var rotDelta = Rotation.Lerp( start.Rotation, targetRot, t );

			var tx = start.RotateAround( localPivot, rotDelta * start.Rotation.Inverse );

			LocalTransform = tx;

			d += Time.Delta;

			await Task.FrameEnd();
		}

		LocalTransform = targetTx;
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
