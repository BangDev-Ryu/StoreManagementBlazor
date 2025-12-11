using Microsoft.EntityFrameworkCore;
//using StoreManagementBlazor.Data;
using StoreManagementBlazor.Models;
using System;

namespace StoreManagementBlazor.Services
{
    public class CustomerService
    {
        private readonly ApplicationDbContext _db;

        public CustomerService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(List<Customer> Customers, int TotalCount)> GetAll(
            string? search = null,
            string? sortBy = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _db.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name != null && c.Name.Contains(search));

            query = sortBy switch
            {
                "name_asc" => query.OrderBy(c => c.Name),
                "name_desc" => query.OrderByDescending(c => c.Name),
                _ => query.OrderBy(c => c.CustomerId)
            };

            int totalCount = await query.CountAsync();

            var customers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (customers, totalCount);
        }

        public async Task<Customer?> GetById(int id)
        {
            return await _db.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public async Task<(bool success, string message)> Create(Customer customer)
        {
            customer.CreatedAt = DateTime.Now;

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return (true, "Thêm khách hàng thành công");
        }

        public async Task<(bool success, string message)> Update(Customer customer)
        {
            var exist = await _db.Customers.FindAsync(customer.CustomerId);
            if (exist == null)
                return (false, "Không tìm thấy khách hàng");

            exist.Name = customer.Name;
            exist.Email = customer.Email;
            exist.Phone = customer.Phone;
            exist.Address = customer.Address;

            await _db.SaveChangesAsync();
            return (true, "Cập nhật thành công");
        }

        public async Task<bool> Delete(int id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return false;

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
