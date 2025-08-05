namespace CuentasCorrientes.Services.Repository;

public interface IMovementsRepository
{
    Task<List<Movements>> GetAllMovements();
    Task<Movements?> GetMovementById(int id);   
    Task<List<Movements>> GetMovementsByCurrentAccountId(int currentAccountId);
    Task CreateMovement(Movements movement);
    Task UpdateMovement(Movements movement);
    void DeleteMovement(Movements movement);

}

public class MovementsRepository(LoggerService loggerService, ApplicationDbContext context) : IMovementsRepository
{
    public async Task<List<Movements>> GetAllMovements()
    {
        return await context.Movements.Include(m => m.CurrentAccount).ToListAsync();
    }
    public async Task<Movements?> GetMovementById(int id)
    {
        return await context.Movements.Include(m => m.CurrentAccount)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
    public async Task<List<Movements>> GetMovementsByCurrentAccountId(int currentAccountId)
    {
        return await context.Movements.Include(m => m.CurrentAccount)
            .Where(m => m.CurrentAccountId == currentAccountId).ToListAsync();
    }
    public async Task CreateMovement(Movements movement)
    {
        if (movement == null)
        {
            loggerService.Log("Attempted to create a null Movement.");
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        loggerService.Log($"Creating Movement for Current Account ID: {movement.CurrentAccountId}");
        movement.Date = DateTime.UtcNow.Date;
        await context.Movements.AddAsync(movement);
        await context.CurrentAccounts.FirstAsync(ca => ca.Id == movement.CurrentAccountId)
            .ContinueWith(ca => 
            {
                if (ca.Result != null)
                {
                    ca.Result.Debt += movement.Amount;
                    context.CurrentAccounts.Update(ca.Result);
                }
            });
        await context.SaveChangesAsync();
    }
    public async Task UpdateMovement(Movements movement)
    {
        if (movement == null)
        {
            loggerService.Log("Attempted to update a null Movement.");
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        loggerService.Log($"Updating Movement for Current Account ID: {movement.CurrentAccountId}");
        context.Movements.Update(movement);
        await context.SaveChangesAsync();
    }
    public void DeleteMovement(Movements movement)
    {
        if (movement == null)
        {
            loggerService.Log("Attempted to delete a null Movement.");
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        loggerService.Log($"Deleting Movement for Current Account ID: {movement.CurrentAccountId}");
        context.Movements.Remove(movement);
        context.SaveChanges();
    }
}
