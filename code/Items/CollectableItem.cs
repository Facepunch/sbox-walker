public sealed class CollectableItem : Component, Component.IPressable
{
	/// <summary>
	/// This is only used for the stat name.
	/// </summary>
	[Property] public string GroupName = "collectable";

	/// <summary>
	/// What icon to show on the HUD for this collectable
	/// </summary>
	[Property] public string Icon = "🤼";

	[Broadcast]
	void Pickup( GameObject player )
	{
		GameObject.Enabled = false;

		// how many are left
		var c = Scene.GetAll<CollectableItem>().Where( x => x != this && x.GroupName == GroupName ).Count();

		Log.Info( $"Collectables remaining: {c}" );

		// set a stat if they collected them all
		if ( c == 0 )
		{

			Sandbox.Services.Stats.Map.SetValue( "collectall", 1 );
		}

	}

	bool IPressable.Press( IPressable.Event e )
	{
		Pickup( e.Source.GameObject );

		if ( !string.IsNullOrWhiteSpace( GroupName ) )
		{
			// Count how many total we've collected on this map. Just for fun.
			Sandbox.Services.Stats.Map.SetValue( $"collect.{GroupName}", 1 );
		}

		return true;
	}

	void IPressable.Release( IPressable.Event e )
	{
		// nothing needed
	}

	bool IPressable.CanPress( Sandbox.Component.IPressable.Event e ) => true;
}
