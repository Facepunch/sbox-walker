
public sealed class PlayerUse : Component
{
	[RequireComponent] public PlayerController PlayerController { get; set; }
	[RequireComponent] public Player Player { get; set; }

	IPressable pressed;

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		//Gizmo.Draw.LineSphere( Vector3.Zero, Radius );
	}

	protected override void OnUpdate()
	{
		if ( !Player.Network.IsOwner )
			return;

		var button = TryGetLookedAt( 0.0f );
		if ( button is null ) return;

		button = TryGetLookedAt( 2.0f );
		if ( button is null ) return;

		if ( Input.Pressed( "use" ) )
		{
			button.Press( new IPressable.Event( this ) );
			pressed = button;
		}

		if ( Input.Released( "use" ) )
		{
			pressed.Release( new IPressable.Event( this ) );
		}
	}

	protected override void OnDisabled()
	{
		if ( pressed is not null )
		{
			pressed.Release( new IPressable.Event( this ) );
		}

		base.OnDisabled();
	}

	IPressable TryGetLookedAt( float radius )
	{
		var eyeTrace = Scene.Trace
						.Ray( Scene.Camera.Transform.World.ForwardRay, 200 )
						.IgnoreGameObjectHierarchy( GameObject )
						.Radius( radius )
						.Run();

		if ( !eyeTrace.Hit ) return default;
		if ( !eyeTrace.GameObject.IsValid() ) return default;

		var button = eyeTrace.GameObject.Components.Get<IPressable>();
		if ( button is null ) return default;

		return button;
	}
}
