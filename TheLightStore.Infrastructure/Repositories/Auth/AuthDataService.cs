using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Interfaces;
using TheLightStore.Domain.Entities.Customers;
using TheLightStore.Domain.Entities.Employees;
using TheLightStore.Infrastructure.Persistence;

namespace TheLightStore.Infrastructure.Repositories.Auth;

public class AuthDataService : IAuthDataService
{
    private readonly DBContext _context;

    public AuthDataService(DBContext context)
    {
        _context = context;
    }
    
    public async Task<CustomerType?> GetCustomerTypeByNameAsync(string customerTypeName)
    {
        return await _context.CustomerTypes.FirstOrDefaultAsync(r => r.Name == "Khách Hàng Phổ Thông");
    }

    public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }
}
