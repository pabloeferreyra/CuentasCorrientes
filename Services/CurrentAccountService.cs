namespace CuentasCorrientes.Services;

#region GetCurrentAccountService
public interface IGetCurrentAccountService
{
    Task<CurrentAccounts?> GetCurrentAccountById(int id);
    Task<List<CurrentAccounts>> GetAllCurrentAccounts();
    Task<List<CurrentAccounts>> GetCurrentAccountsByClientId(int clientId);
    Task<CurrentAccounts?> GetCurrentAccountByClientId(int clientId);
}
public class GetCurrentAccountService(ICurrentAccountRepository repository) : IGetCurrentAccountService
{
    public async Task<CurrentAccounts?> GetCurrentAccountById(int id)
    {
        var currentAccount = await repository.GetCurrentAccountById(id);
        return currentAccount ?? throw new InvalidOperationException($"No se encontró Cuenta Corriente con id {id}.");
    }
    public async Task<List<CurrentAccounts>> GetAllCurrentAccounts() => await repository.GetAllCurrentAccounts();
    public async Task<List<CurrentAccounts>> GetCurrentAccountsByClientId(int clientId) => await repository.GetCurrentAccountsByClientId(clientId);
    public async Task<CurrentAccounts?> GetCurrentAccountByClientId(int clientId) => await repository.GetCurrentAccountByClientId(clientId);
}
#endregion

#region CreateCurrentAccountService
public interface ICreateCurrentAccountService
{
    Task CreateCurrentAccount(CurrentAccounts currentAccount);
}
public class CreateCurrentAccountService(ICurrentAccountRepository repository) : ICreateCurrentAccountService
{
    public async Task CreateCurrentAccount(CurrentAccounts currentAccount)
    {
        if (currentAccount == null)
        {
            throw new ArgumentNullException(nameof(currentAccount), "El CurrentAccount no puede ser nulo.");
        }
        await repository.CreateCurrentAccount(currentAccount);
    }
}
#endregion

#region UpdateCurrentAccountService
public interface IUpdateCurrentAccountService
{
    Task UpdateCurrentAccount(CurrentAccounts currentAccount);
}
public class UpdateCurrentAccountService(ICurrentAccountRepository repository) : IUpdateCurrentAccountService
{
    public async Task UpdateCurrentAccount(CurrentAccounts currentAccount)
    {
        if (currentAccount == null)
        {
            throw new ArgumentNullException(nameof(currentAccount), "El CurrentAccount no puede ser nulo.");
        }
        await repository.UpdateCurrentAccount(currentAccount);
    }
}
#endregion

#region DeleteCurrentAccountService
public interface IDeleteCurrentAccountService
{
    void DeleteCurrentAccount(CurrentAccounts currentAccount);
}
public class DeleteCurrentAccountService(ICurrentAccountRepository repository) : IDeleteCurrentAccountService
{
    public void DeleteCurrentAccount(CurrentAccounts currentAccount)
    {
        if (currentAccount == null)
        {
            throw new ArgumentNullException(nameof(currentAccount), "El CurrentAccount no puede ser nulo.");
        }
        repository.DeleteCurrentAccount(currentAccount);
    }
}
#endregion
