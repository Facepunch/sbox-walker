using Sandbox.Citizen;

public partial class BaseWeapon : Component
{
	[Property, Feature( "Inventory" ), Range( 0, 4 )] public int InventorySlot { get; set; } = 0;
	[Property, Feature( "Inventory" )] public int InventoryOrder { get; set; } = 0;
	[Property, Feature( "Inventory" )] public string DisplayName { get; set; } = "My Weapon";
	[Property, Feature( "Inventory" )] public string DisplayIcon { get; set; } = "photo_camera";
	[Property, Feature( "Inventory" ), TextArea] public string DisplaySvg { get; set; } = "";

	[Property, Feature( "ViewModel" )] public GameObject ViewModelPrefab { get; set; }

	[Property, Feature( "WorldModel" )] public GameObject WorldModelPrefab { get; set; }
	[Property, Feature( "WorldModel" )] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.HoldItem;
	[Property, Feature( "WorldModel" )] public string ParentBone { get; set; } = "hold_r";

	public GameObject ViewModel { get; protected set; }
	public GameObject WorldModel { get; protected set; }

	protected override void OnEnabled()
	{
		CreateWorldModel();
	}

	protected override void OnDisabled()
	{
		DestroyWorldModel();
		DestroyViewModel();
	}


	public virtual void OnPlayerUpdate( Player player )
	{
		if ( player is null ) return;

		if ( !player.Controller.ThirdPerson )
		{
			CreateViewModel();
		}
		else
		{
			DestroyViewModel();
		}

		var body = player.Body.Components.Get<SkinnedModelRenderer>();
		body.Set( "holdtype", (int)HoldType );

		GameObject.NetworkInterpolation = false;

		//var obj = body.GetBoneObject( ParentBone );
		//if ( obj is not null )
		//{
		//	GameObject.Parent = obj;
		//	GameObject.LocalTransform = global::Transform.Zero;
		//}

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
