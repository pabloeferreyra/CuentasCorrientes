using System;

namespace CuentasCorrientes.Services.Repository;

public class BalanceRepository(LoggerService loggerService, ApplicationDbContext context) : BaseRepository<Client>(context), IBalanceRepository
{
    public async Task<List<Balance>> GetBalance(int year, int month)
    {
        try
        {
            var balances = await _context.Set<Balance>()
            .FromSqlInterpolated($"SELECT * FROM get_movements_with_client({year}, {month})")
            .ToListAsync();
            return balances;
        }
        catch (Exception ex)
        {
            loggerService.Log($"Error in GetBalance: {ex.Message}");
            return [];
        }
    }
}

public interface IBalanceRepository
{
    Task<List<Balance>> GetBalance(int year, int month);
}