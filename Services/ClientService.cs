namespace CuentasCorrientes.Services;

#region GetClientService
public interface IGetClientService
{
    Task<Client> GetClient(int clientId);
    Task<List<Client>> GetAllClients();
    Task<Client> GetClient(string clientName);
    Task<List<Client>> GetClientsByType(int clientTypeId);

}
public class GetClientService(IClientRepository repository) : IGetClientService
{
    public async Task<Client> GetClient(int clientId)
    {
        var client = await repository.GetClient(clientId);
        return client ?? throw new InvalidOperationException($"No se encontró Cliente con id {clientId}.");
    }
    public async Task<List<Client>> GetAllClients() => await repository.GetAllClients();
    public async Task<Client> GetClient(string clientName)
    {
        var client = await repository.GetClient(clientName);
        return client ?? throw new InvalidOperationException($"No se encontró Cliente con nombre {clientName}.");
    }
    public async Task<List<Client>> GetClientsByType(int clientTypeId) => await repository.GetClientsByType(clientTypeId);
}
#endregion

#region CreateClientService
public interface ICreateClientService
{
    Task CreateClient(Client client);
}
public class CreateClientService(IClientRepository repository) : ICreateClientService
{
    public async Task CreateClient(Client client)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client), "El Cliente no puede ser nulo.");
        await repository.CreateClient(client);
    }
}
#endregion

#region UpdateClientService
public interface IUpdateClientService
{
    Task UpdateClient(Client client);
}
public class UpdateClientService(IClientRepository repository) : IUpdateClientService
{
    public async Task UpdateClient(Client client)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client), "El Cliente no puede ser nulo.");
        await repository.UpdateClient(client);
    }
}
#endregion

#region DeleteClientService
public interface IDeleteClientService
{
    void DeleteClient(Client client);
}
public class DeleteClientService(IClientRepository repository) : IDeleteClientService
{
    public void DeleteClient(Client client)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client), "El Cliente no puede ser nulo.");
        repository.DeleteClient(client);
    }
}
#endregion