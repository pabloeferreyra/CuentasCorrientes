namespace CuentasCorrientes.Services.Repository;

public interface ICurrentAccountRepository
{
    Task<CurrentAccounts?> GetCurrentAccountById(int id);
    Task<List<CurrentAccounts>> GetAllCurrentAccounts();
    Task CreateCurrentAccount(CurrentAccounts currentAccount);
    Task UpdateCurrentAccount(CurrentAccounts currentAccount);
    void DeleteCurrentAccount(CurrentAccounts currentAccount);
    Task<List<CurrentAccounts>> GetCurrentAccountsByClientId(int clientId);
    Task<CurrentAccounts?> GetCurrentAccountByClientId(int clientId);
}
public class CurrentAccountRepository(LoggerService loggerService, ApplicationDbContext context) : BaseRepository<CurrentAccounts>(context), ICurrentAccountRepository
{
    public async Task CreateCurrentAccount(CurrentAccounts currentAccount)
    {
        if (currentAccount == null)
        {
            loggerService.Log("Attempted to create a null CurrentAccount.");
            throw new ArgumentNullException(nameof(currentAccount), "El CurrentAccount no puede ser nulo.");
        }
        loggerService.Log($"Creating CurrentAccount for Client ID: {currentAccount.ClientId}");
        await CreateAsync(currentAccount);
    }
    public void DeleteCurrentAccount(CurrentAccounts currentAccount)
    {
        if (currentAccount == null)
        {
            loggerService.Log("Attempted to delete a null CurrentAccount.");
            throw new ArgumentNullException(nameof(currentAccount), "El CurrentAccount no puede ser nulo.");
        }
        loggerService.Log($"Deleting CurrentAccount for Client ID: {currentAccount.ClientId}");
        Delete(currentAccount);
    }
    public async Task<List<CurrentAccounts>> GetAllCurrentAccounts()
    {
        return await FindAll().Include(c => c.Client).ToListAsync();
    }
    public async Task<CurrentAccounts?> GetCurrentAccountById(int id)
    {
        return await FindByCondition(ca => ca.Id == id).Include(c => c.Client).FirstOrDefaultAsync();
    }
    public async Task<List<CurrentAccounts>> GetCurrentAccountsByClientId(int clientId)
    {
        return await FindByCondition(ca => ca.ClientId == clientId).Include(ca => ca.Client).ToListAsync();
    }
    public async Task<CurrentAccounts?> GetCurrentAccountByClientId(int clientId)
    {
        CurrentAccounts accounts = await FindByCondition(ca => ca.ClientId == clientId)
            .Include(ca => ca.Client).Include(m => m.Movements).FirstOrDefaultAsync();
        accounts.Debt = accounts.Movements.Sum(m => m.Amount);
        return accounts;
    }

    public async Task UpdateCurrentAccount(CurrentAccounts currentAccount)
    {
        if (currentAccount == null)
        {
            loggerService.Log("Attempted to update a null CurrentAccount.");
            throw new ArgumentNullException(nameof(currentAccount), "El CurrentAccount no puede ser nulo.");
        }
        loggerService.Log($"Updating CurrentAccount for Client ID: {currentAccount.ClientId}");
        await UpdateAsync(currentAccount);
    }
}

