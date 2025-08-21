namespace CuentasCorrientes.Services.Repository;

public interface IClientTypeRepository
{
    Task<ClientType?> GetClientTypeById(int id);
    Task<List<ClientType>> GetAllClientTypes();
    Task CreateClientType(ClientType clientType);
    Task UpdateClientType(ClientType clientType);
    void DeleteClientType(ClientType clientType);
}


public class ClientTypeRepository(LoggerService loggerService, ApplicationDbContext context) : BaseRepository<ClientType>(context), IClientTypeRepository
{
    public async Task<ClientType?> GetClientTypeById(int id)
    {
        return await FindByCondition(ct => ct.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<ClientType>> GetAllClientTypes()
    {
        return await FindAll().ToListAsync();
    }

    public async Task CreateClientType(ClientType clientType)
    {
        if (clientType == null)
        {
            loggerService.Log("Attempted to create a null ClientType.");
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        }
        loggerService.Log($"Creating ClientType: {clientType.Name}");
        await CreateAsync(clientType);
    }

    public async Task UpdateClientType(ClientType clientType)
    {   
        if (clientType == null)
        {
            loggerService.Log("Attempted to update a null ClientType.");
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        }
        loggerService.Log($"Updating ClientType: {clientType.Name}");
        await UpdateAsync(clientType);
    }

    public void DeleteClientType(ClientType clientType)
    {
        if (clientType == null)
        {
            loggerService.Log("Attempted to delete a null ClientType.");
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        }
        loggerService.Log($"Deleting ClientType: {clientType.Name}");
        Delete(clientType);
    }
}