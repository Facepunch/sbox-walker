public sealed class TriggerTeleport : Component, Component.ITriggerListener
{
	[Property] public GameObject Target { get; set; }
	[Property] public Action<GameObject> OnTeleported { get; set; }

	protected override void DrawGizmos()
	{
		if ( !Target.IsValid() )
			return;

		Gizmo.Draw.Arrow( 0, WorldTransform.PointToLocal( Target.WorldPosition ) );
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var go = other.GameObject;

		if ( !IsValidTarget( ref go ) ) return;

		go.WorldPosition = Target.WorldPosition;
		go.Transform.ClearInterpolation();

		DoTeleportedEvent( go );
	}

	bool IsValidTarget( ref GameObject go )
	{
		go = go.Root;
		if ( go.IsProxy ) return false;
		return true;
	}

	[Rpc.Broadcast]
	void DoTeleportedEvent( GameObject obj )
	{
		OnTeleported?.Invoke( obj );
	}
}
