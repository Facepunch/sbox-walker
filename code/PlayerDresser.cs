[Group( "Walker" )]
[Title( "Walker - Player Dresser" )]
public sealed class PlayerDresser : Component, Component.INetworkSpawn
{
	[Property]
	public SkinnedModelRenderer BodyRenderer { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( owner.GetUserData( "avatar" ) );
		clothing.Apply( BodyRenderer );
	}
}
