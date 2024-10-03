

public sealed class PlayerUse : Component
{
	[RequireComponent] public PlayerController PlayerController { get; set; }
	[RequireComponent] public Player Player { get; set; }

	IPressable pressed;
	Rigidbody carrying;
	Transform carryTransform;
	Transform carryOriginalTransform;

	public bool Interative;
	public string TooltipIcon;
	public string Tooltip;

	protected override void OnUpdate()
	{
		if ( !Player.Network.IsOwner )
			return;

		var lookingAt = TryGetLookedAt( 0.0f );
		lookingAt ??= TryGetLookedAt( 2.0f );
		lookingAt ??= TryGetLookedAt( 4.0f );
		lookingAt ??= TryGetLookedAt( 8.0f );


		if ( Input.Pressed( "use" ) )
		{
			if ( lookingAt is IPressable button )
			{
				button.Press( new IPressable.Event( this ) );
				pressed = button;
			}

			if ( lookingAt is Rigidbody rb )
			{
				StartCarry( rb );
			}
		}

		if ( Input.Released( "use" ) )
		{
			if ( pressed is not null )
			{
				pressed.Release( new IPressable.Event( this ) );
				pressed = default;
			}

			if ( carrying is not null )
			{
				StopCarrying();
			}
		}

		if ( pressed is not null || carrying.IsValid() )
		{
			Interative = false;
			return;
		}

		if ( lookingAt is IPressable btn )
		{
			var c = (Component)btn;
			var tt = c.GetComponent<Tooltip>();
			if ( tt is not null )
			{
				Tooltip = tt.Text;
				TooltipIcon = tt.Icon;
			}
			else
			{
				Tooltip = $"Use";
				TooltipIcon = "pan_tool_alt";
			}

			Interative = true;
		}
		else if ( lookingAt is Rigidbody rbb && CanCarry( rbb ) )
		{
			Tooltip = "Pick Up";
			TooltipIcon = "back_hand";
			Interative = true;
		}
		else
		{
			Tooltip = null;
			Interative = false;
		}
	}

	private void StartCarry( Rigidbody rb )
	{
		if ( !rb.Network.TakeOwnership() )
			return;

		carrying = rb;
		carryOriginalTransform = rb.Transform.World;
		carryTransform = Player.EyeTransform.ToLocal( rb.Transform.World );
	}

	void StopCarrying()
	{
		if ( carrying.IsValid() )
		{
			carrying.Network.DropOwnership();
		}

		carrying = default;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( carrying.IsValid() )
		{
			var targetTransform = Player.EyeTransform.ToWorld( carryTransform );
			targetTransform.Rotation = carryOriginalTransform.Rotation.Angles().WithYaw( targetTransform.Rotation.Angles().yaw );

			var distance = Vector3.DistanceBetween( targetTransform.Position, carrying.WorldPosition );

			if ( distance > 50.0f )
			{
				StopCarrying();
				return;
			}

			var mass = carrying.PhysicsBody.Mass;
			var moveSpeed = mass.Remap( 50, 3000, 0.05f, 2.0f, true );

			carrying.PhysicsBody.SmoothMove( targetTransform, moveSpeed, Time.Delta );
		}
	}

	private bool CanCarry( Rigidbody rb )
	{
		if ( !rb.IsValid() ) return false;
		if ( !rb.Network.Active ) return false;

		return true;
	}

	protected override void OnDisabled()
	{
		if ( pressed is not null )
		{
			pressed.Release( new IPressable.Event( this ) );
		}

		base.OnDisabled();
	}

	object TryGetLookedAt( float radius )
	{
		var eyeTrace = Scene.Trace
						.Ray( Scene.Camera.Transform.World.ForwardRay, 150 )
						.IgnoreGameObjectHierarchy( GameObject )
						.Radius( radius )
						.Run();

		if ( !eyeTrace.Hit ) return default;
		if ( !eyeTrace.GameObject.IsValid() ) return default;

		var button = eyeTrace.GameObject.Components.Get<IPressable>();
		if ( button is not null && button.CanPress( new IPressable.Event( this ) ) ) return button;

		var rigidbody = eyeTrace.GameObject.Components.Get<Rigidbody>();
		if ( CanCarry( rigidbody ) ) return rigidbody;

		return default;
	}

}


public class Tooltip : Component
{
	[Property] public string Text { get; set; }
	[Property] public string Icon { get; set; }
}
