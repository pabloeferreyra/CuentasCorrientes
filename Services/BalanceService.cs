namespace CuentasCorrientes.Services;

public class BalanceService (IBalanceRepository balanceRepository) : IBalanceService
{
    public async Task<List<Balance>> GetBalance(int year, int month)
    {
        return await balanceRepository.GetBalance(year, month);
    }
}
public interface IBalanceService
{
    Task<List<Balance>> GetBalance(int year, int month);
}
