using Sandbox.Citizen;

public class BaseWeapon : Component
{
	[Property, Range( 0, 4 )] public int InventorySlot { get; set; } = 0;
	[Property] public int InventoryOrder { get; set; } = 0;
	[Property] public string DisplayName { get; set; } = "My Weapon";
	[Property] public string DisplayIcon { get; set; } = "photo_camera";
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.HoldItem;
	[Property] public string ParentBone { get; set; } = "hold_r";
	[Property] public Transform BoneOffset { get; set; } = new Transform( 0 );

	public virtual void OnPlayerUpdate( Player player )
	{
		if ( player is null ) return;

		var body = player.Body.Components.Get<SkinnedModelRenderer>();
		body.Set( "holdtype", (int)HoldType );

		GameObject.NetworkInterpolation = false;

		var obj = body.GetBoneObject( ParentBone );
		if ( obj is not null )
		{
			GameObject.Parent = obj;
			GameObject.LocalTransform = BoneOffset.WithScale( 1 );
		}

		if ( IsProxy )
			return;

		OnControl( player );
	}

	public virtual void OnControl( Player player )
	{
	}

	public virtual void OnCameraSetup( Player player, Sandbox.CameraComponent camera )
	{
	}

	public virtual void OnCameraMove( Player player, ref Angles angles )
	{

	}
}
