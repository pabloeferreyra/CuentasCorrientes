using CuentasCorrientes.Helpers;

namespace CuentasCorrientes.Services;

#region GetMovementService
public interface  IGetMovementService
{
    Task<List<Movements>> GetAllMovements();
    Task<Movements?> GetMovementById(int id);
    Task<List<Movements>> GetMovementsByCurrentAccountId(int currentAccountId);
}

public class GetMovementService(IMovementsRepository repository) : IGetMovementService
{
    public async Task<List<Movements>> GetAllMovements() => await repository.GetAllMovements();
    public async Task<Movements?> GetMovementById(int id) => await repository.GetMovementById(id);
    public async Task<List<Movements>> GetMovementsByCurrentAccountId(int currentAccountId) =>
        await repository.GetMovementsByCurrentAccountId(currentAccountId);
}

#endregion

#region CreateMovementService
public interface ICreateMovementService
{
    Task CreateMovement(Movements movement);
}

public class CreateMovementService(IMovementsRepository repository, ICurrentAccountRepository account, LoggerService loggerService) : ICreateMovementService
{
    public async Task CreateMovement(Movements movement)
    {
        if (movement == null)
        {
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        try
        {
            await repository.CreateMovement(movement);
        }
        catch (Exception ex)
        {
            loggerService.Log($"Error al crear el Movement: {ex.InnerException}");
            return;
        }
    }
}
#endregion

#region UpdateMovementService
public interface IUpdateMovementService
{
    Task UpdateMovement(Movements movement);
}

public class UpdateMovementService(IMovementsRepository repository) : IUpdateMovementService
{
    public async Task UpdateMovement(Movements movement)
    {
        if (movement == null)
        {
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        try
        {
            await repository.UpdateMovement(movement);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al actualizar el Movement: {ex.InnerException}");
        }
    }
}
#endregion

#region DeleteMovementService
public interface IDeleteMovementService
{
    Task DeleteMovement(Movements movement);
}

public class DeleteMovementService(IMovementsRepository repository, ICurrentAccountRepository account) : IDeleteMovementService
{
    public async Task DeleteMovement(Movements movement)
    {
        if (movement == null)
        {
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        try
        {
            await repository.DeleteMovement(movement);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al eliminar el Movement: {ex.InnerException}");
        }
    }
}
#endregion