

public sealed class PlayerUse : Component, PlayerController.IEvents
{
	[RequireComponent] public Player Player { get; set; }

	Rigidbody carrying;
	Transform carryTransform;
	Transform carryOriginalTransform;

	public bool Interative;
	public string TooltipIcon;
	public string Tooltip;

	/// <summary>
	/// Hook into the PlayerController's use system and tell it we want to 
	/// interact with RigidBodies that we can carry
	/// </summary>
	Component PlayerController.IEvents.GetUsableComponent( GameObject go )
	{
		var rb = go.GetComponent<Rigidbody>();
		if ( CanCarry( rb ) )
		{
			return rb;
		}

		return default;
	}

	/// <summary>
	/// Can carry rigidbodies that are networked and we can take ownership of
	/// </summary>
	private bool CanCarry( Rigidbody rb )
	{
		if ( !rb.IsValid() ) return false;
		if ( !rb.Network.Active ) return false;
		if ( rb.Network.OwnerTransfer != OwnerTransfer.Takeover ) return false;

		return true;
	}

	void UpdateTooltips( Component lookingAt, Component pressed )
	{

		if ( !lookingAt.IsValid() || pressed.IsValid() )
		{
			Tooltip = null;
			Interative = false;
			return;
		}

		var tt = lookingAt.GetComponent<Tooltip>();
		if ( tt is not null )
		{
			Tooltip = tt.Text;
			TooltipIcon = tt.Icon;
			Interative = true;
			return;
		}

		if ( lookingAt is Rigidbody rbb && CanCarry( rbb ) )
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

	void PlayerController.IEvents.StartPressing( Sandbox.Component target )
	{
		var rb = target.GetComponent<Rigidbody>();
		if ( CanCarry( rb ) )
		{
			StartCarry( rb );
		}
	}

	void PlayerController.IEvents.StopPressing( Sandbox.Component target )
	{
		StopCarrying();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		UpdateTooltips( Player.Controller.Hovered, Player.Controller.Pressed );
	}

	private void StartCarry( Rigidbody rb )
	{
		if ( !rb.Network.TakeOwnership() )
			return;

		StopCarrying();

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

		if ( carrying.IsValid() && !carrying.IsProxy )
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
