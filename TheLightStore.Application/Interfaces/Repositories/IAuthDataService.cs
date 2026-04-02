using TheLightStore.Domain.Entities.Customers;
using TheLightStore.Domain.Entities.Employees;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IAuthDataService
{
    Task<CustomerType?> GetCustomerTypeByNameAsync(string customerTypeName);
    Task<Employee?> GetEmployeeByUserIdAsync(string userId);
    Task AddCustomerAsync(Customer customer);
    Task SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
