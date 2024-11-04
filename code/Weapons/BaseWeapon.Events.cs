public partial class BaseWeapon : Component
{
	public interface IWeaponEvent : ISceneEvent<IWeaponEvent>
	{
		public record struct AttackEvent( bool firstperson, Vector3 HitPoint );

		void Attack( AttackEvent e );
	}
}
