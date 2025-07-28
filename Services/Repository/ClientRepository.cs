namespace CuentasCorrientes.Services.Repository;

public interface IClientRepository
{
    Task<Client?> GetClient(int id);
    Task<List<Client>> GetAllClients();
    Task<Client?> GetClient(string clientName);
    Task<List<Client>> GetClientsByType(int clientTypeId);
    Task CreateClient(Client client);
    Task UpdateClient(Client client);
    void DeleteClient(Client client);
}
public class ClientRepository(LoggerService loggerService, ApplicationDbContext context) : BaseRepository<Client>(context), IClientRepository
{
    public Task CreateClient(Client client)
    {
        if (client == null)
        {
            loggerService.Log("Attempted to create a null Client.");
            throw new ArgumentNullException(nameof(client), "El Client no puede ser nulo.");
        }
        loggerService.Log($"Creating Client: {client.Name}");
        return CreateAsync(client);
    }

    public void DeleteClient(Client client)
    {
        if (client == null)
        {
            loggerService.Log("Attempted to delete a null Client.");
            throw new ArgumentNullException(nameof(client), "El Client no puede ser nulo.");
        }
        loggerService.Log($"Deleting Client: {client.Name}");
        Delete(client);
    }

    public Task<List<Client>> GetAllClients()
    {
        return FindAll().Include(c => c.ClientType).ToListAsync();
    }

    public Task<Client?> GetClient(int id)
    {
        return FindByCondition(c => c.Id == id).Include(c => c.ClientType).FirstOrDefaultAsync();
    }

    public Task<Client?> GetClient(string clientName)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            loggerService.Log("Attempted to get a Client with a null or empty name.");
            throw new ArgumentException("El nombre del cliente no puede ser nulo o vacío.", nameof(clientName));
        }
        return FindByCondition(c => c.Name == clientName).Include(c => c.ClientType).FirstOrDefaultAsync();
    }

    public Task<List<Client>> GetClientsByType(int clientTypeId)
    {
        if (clientTypeId <= 0)
        {
            loggerService.Log("Attempted to get Clients with an invalid ClientTypeId.");
            throw new ArgumentException("El ClientTypeId debe ser mayor que cero.", nameof(clientTypeId));
        }
        return FindByCondition(c => c.ClientTypeId == clientTypeId).Include(c => c.ClientType).ToListAsync();
    }

    public Task UpdateClient(Client client)
    {
        if (client == null)
        {
            loggerService.Log("Attempted to update a null Client.");
            throw new ArgumentNullException(nameof(client), "El Client no puede ser nulo.");
        }
        loggerService.Log($"Updating Client: {client.Name}");
        return UpdateAsync(client);
    }
}
