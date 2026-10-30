namespace AIEnemyLibrary.Core
{
    /// <summary>
    /// Интерфейс для объектов, которые могут получать урон.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}