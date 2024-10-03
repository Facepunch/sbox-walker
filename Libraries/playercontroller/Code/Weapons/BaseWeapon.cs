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

	protected override void OnUpdate()
	{
		GameObject.NetworkInterpolation = false;

		var owner = GameObject.Components.GetInAncestorsOrSelf<Player>();
		if ( owner is null ) return;

		var body = owner.Body.Components.Get<SkinnedModelRenderer>();
		body.Set( "holdtype", (int)HoldType );

		var obj = body.GetBoneObject( ParentBone );
		if ( obj is not null )
		{
			GameObject.Parent = obj;
			GameObject.LocalTransform = BoneOffset.WithScale( 1 );
		}

		if ( IsProxy )
			return;

		OnControl( owner );
	}

	public virtual void OnControl( Player player )
	{

	}
}
