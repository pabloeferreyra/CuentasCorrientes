namespace CuentasCorrientes.Services.Repository;

public interface IMovementsRepository
{
    Task<List<Movements>> GetAllMovements();
    Task<Movements?> GetMovementById(int id);   
    Task<List<Movements>> GetMovementsByCurrentAccountId(int currentAccountId);
    Task CreateMovement(Movements movement);
    Task UpdateMovement(Movements movement);
    Task DeleteMovement(Movements movement);

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
        loggerService.Log($"Creating Movement for Account ID: {movement.CurrentAccountId}");
        
        movement.Amount = !movement.Debt ? -Math.Abs(movement.Amount) : Math.Abs(movement.Amount);

        await context.Movements.AddAsync(movement);
        await context.SaveChangesAsync();
        var current = await context.CurrentAccounts.FirstAsync(ca => ca.Id == movement.CurrentAccountId);
        if (current != null)
        {
            current.Debt += movement.Amount;
            context.CurrentAccounts.Update(current);
            await context.SaveChangesAsync();
        }
        
    }
    public async Task UpdateMovement(Movements movement)
    {
        if (movement == null)
        {
            loggerService.Log("Attempted to update a null Movement.");
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        loggerService.Log($"Updating Movement for Account ID: {movement.CurrentAccountId}");
        context.Movements.Update(movement);
        await context.SaveChangesAsync();
    }
    public async Task DeleteMovement(Movements movement)
    {
        if (movement == null)
        {
            loggerService.Log("Attempted to delete a null Movement.");
            throw new ArgumentNullException(nameof(movement), "El Movement no puede ser nulo.");
        }
        loggerService.Log($"Deleting Movement for Account ID: {movement.CurrentAccountId}");
        context.Movements.Remove(movement);
        context.SaveChanges();
        var account = await context.CurrentAccounts.Include(m => m.Movements).FirstAsync(ca => ca.Id == movement.CurrentAccountId);
        if(account != null)
        {
            account.Debt = account.Movements?.Sum(c => c.Amount) ?? 0;
            context.CurrentAccounts.Update(account);
            context.SaveChanges();
        }
    }
}
