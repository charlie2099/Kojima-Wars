public interface IDamageable
{
    void TakeDamageServerRpc(int damage);
    void HealDamageServerRpc(int heal);

    bool IsAlive();

    bool IsEnabled();

    bool IsRecalling();
}
