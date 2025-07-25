using CuentasCorrientes.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace CuentasCorrientes.Services.Repository;

public class BaseRepository<T>(ApplicationDbContext context) : IBaseRepository<T> where T : class
{
    protected ApplicationDbContext _context = context;

    public IQueryable<T> FindAll() => _context.Set<T>().AsNoTracking();
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).AsNoTracking();
    }

    public void Create(T entity)
    {
        _context.Set<T>().Add(entity);
        _context.SaveChanges();
    }

    public async Task CreateAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
        _context.SaveChanges();
    }
    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        _context.SaveChanges();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public List<T> CallStoredProcedure(string procedureName, params object[] parameters)
    {
        var sqlParameters = new List<SqlParameter>();
        var sqlParametersString = new StringBuilder();

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterName = $"@p{i}";
            var sqlParameter = new SqlParameter(parameterName, parameters[i]);
            sqlParameters.Add(sqlParameter);
            sqlParametersString.Append(parameters[i]);

            if (i != parameters.Length - 1)
            {
                sqlParametersString.Append(", ");
            }
        }

        var sql = $"select * from {procedureName}({sqlParametersString})";

        return [.. _context.Set<T>().FromSqlRaw(sql)];
    }

    public IQueryable<T> CallStoredProcedureDTO(string connectionString, string procedureName)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        var command = new NpgsqlCommand(procedureName, connection)
        {
            CommandType = CommandType.Text
        };

        var results = command.ExecuteReader();
        var mappedResults = MapResults(results);
        return mappedResults.AsQueryable();
    }

    private static List<T> MapResults(NpgsqlDataReader reader)
    {
        var results = new List<T>();
        var properties = typeof(T).GetProperties();

        while (reader.Read())
        {
            var instance = Activator.CreateInstance<T>();

            foreach (var property in properties)
            {
                if (reader[property.Name] != DBNull.Value)
                {
                    property.SetValue(instance, reader[property.Name]);
                }
            }

            results.Add(instance);
        }

        return results;
    }
}

public interface IBaseRepository<T>
{
    IQueryable<T> FindAll();
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);

    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    List<T> CallStoredProcedure(string procedureName, params object[] parameters);
    IQueryable<T> CallStoredProcedureDTO(string connectionString, string procedureName);
}