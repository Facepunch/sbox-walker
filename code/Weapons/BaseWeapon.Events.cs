public partial class BaseWeapon : Component
{
	public interface IWeaponEvent : ISceneEvent<IWeaponEvent>
	{
		public record struct AttackEvent( bool firstperson );

		void Attack( AttackEvent e );
	}
}
