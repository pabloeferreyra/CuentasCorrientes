using CuentasCorrientes.Models;
using CuentasCorrientes.Services.Repository;

namespace CuentasCorrientes.Services;

#region GetClientTypeService
public interface IGetClientTypeService
{
    Task<ClientType> GetClientType(int clientId);
    Task<List<ClientType>> GetAllClientTypes();
}
public class GetClientTypeService(IClientTypeRepository repository) : IGetClientTypeService
{
    private readonly IClientTypeRepository _repository = repository;

    public async Task<ClientType> GetClientType(int clientId)
    {
        var clientType = await _repository.GetClientTypeById(clientId);
        return clientType is null ? throw new InvalidOperationException($"No se encontró ClientType con id {clientId}.") : clientType;
    }

    public async Task<List<ClientType>> GetAllClientTypes() => await _repository.GetAllClientTypes();
}
#endregion

#region CreateClientTypeService
public interface ICreateClientTypeService
{
    Task CreateClientType(ClientType clientType);
}

public class CreateClientTypeService(IClientTypeRepository repository) : ICreateClientTypeService
{
    private readonly IClientTypeRepository _repository = repository;
    public async Task CreateClientType(ClientType clientType)
    {
        if (clientType is null)
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        await _repository.CreateClientType(clientType);
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
    private readonly IClientTypeRepository _repository = repository;
    public async Task UpdateClientType(ClientType clientType)
    {
        if (clientType is null)
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        await _repository.UpdateClientType(clientType);
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
    private readonly IClientTypeRepository _repository = repository;
    public void DeleteClientType(ClientType clientType)
    {
        if (clientType is null)
            throw new ArgumentNullException(nameof(clientType), "El ClientType no puede ser nulo.");
        _repository.DeleteClientType(clientType);
    }
}
#endregion
