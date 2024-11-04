public partial class BaseWeapon : Component
{
	void CreateWorldModel()
	{
		DestroyWorldModel();

		if ( WorldModelPrefab is null )
			return;

		var player = GetComponentInParent<PlayerController>();
		if ( player is null || player.Renderer is null ) return;

		var parentBone = player.Renderer.GetBoneObject( ParentBone );

		WorldModel = WorldModelPrefab.Clone( new CloneConfig { Parent = parentBone, StartEnabled = false, Transform = global::Transform.Zero } );
		WorldModel.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.NotNetworked;
		WorldModel.Enabled = true;
	}

	void DestroyWorldModel()
	{
		WorldModel?.Destroy();
		WorldModel = default;
	}
}
