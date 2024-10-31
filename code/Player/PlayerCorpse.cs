/// <summary>
/// A corpse. Clientside only. Automatically destroyed after a period of time.
/// </summary>
public class PlayerCorpse : Component
{
	public Connection Connection { get; set; }
	public DateTime Created { get; set; }

	protected override void OnEnabled()
	{
		Invoke( 60.0f, GameObject.Destroy );
	}
}
