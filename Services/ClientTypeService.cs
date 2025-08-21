namespace CuentasCorrientes.Services;

#region GetClientTypeService
public interface IGetClientTypeService
{
    Task<ClientType> GetClientType(int clientId);
    Task<List<ClientType>> GetAllClientTypes();
}
public class GetClientTypeService(IClientTypeRepository repository) : IGetClientTypeService
{
    public async Task<ClientType> GetClientType(int clientId)
    {
        var clientType = await repository.GetClientTypeById(clientId);
        return clientType is null ? throw new InvalidOperationException($"No se encontró ClientType con id {clientId}.") : clientType;
    }

    public async Task<List<ClientType>> GetAllClientTypes() => await repository.GetAllClientTypes();
}
#endregion

#region CreateClientTypeService
public interface ICreateClientTypeService
{
    Task CreateClientType(ClientType clientType);
}

public class CreateClientTypeService(IClientTypeRepository repository) : ICreateClientTypeService
{
    public async Task CreateClientType(ClientType clientType)
    {
        if (clientType is null)
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        await repository.CreateClientType(clientType);
    }
}
#endregion

#region UpdateClientTypeService
public interface IUpdateClientTypeService
{
    Task UpdateClientType(ClientType clientType);
}

public class UpdateClientTypeService(IClientTypeRepository repository) : IUpdateClientTypeService
{
    public async Task UpdateClientType(ClientType clientType)
    {
        if (clientType is null)
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        await repository.UpdateClientType(clientType);
    }
}
#endregion

#region DeleteClientTypeService
public interface IDeleteClientTypeService
{
    void DeleteClientType(ClientType clientType);
}

public class DeleteClientTypeService(IClientTypeRepository repository) : IDeleteClientTypeService
{
    public void DeleteClientType(ClientType clientType)
    {
        if (clientType is null)
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        repository.DeleteClientType(clientType);
    }
}
#endregion
