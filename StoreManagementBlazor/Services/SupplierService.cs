using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;

namespace StoreManagementBlazor.Services
{
    public class SupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Lấy danh sách phân trang + search + sort theo Name
        public async Task<(List<Supplier> Suppliers, int TotalCount)> GetPagedAsync(
            int page = 1,
            int pageSize = 5,
            string? search = null,
            bool sortAsc = true)
        {
            var query = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    (s.Name != null && s.Name.Contains(search)) ||
                    (s.Email != null && s.Email.Contains(search)) ||
                    (s.Phone != null && s.Phone.Contains(search)) ||
                    (s.Address != null && s.Address.Contains(search))
                );
            }

            int totalCount = await query.CountAsync();

            query = sortAsc ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name);

            var suppliers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (suppliers, totalCount);
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<(bool success, string message)> CreateAsync(Supplier supplier)
        {
            if (supplier == null) return (false, "Supplier null");

            bool exists = await _context.Suppliers.AnyAsync(s => s.Email == supplier.Email);
            if (exists) return (false, "Email đã tồn tại");

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return (true, "Tạo nhà cung cấp thành công");
        }
        public async Task<(List<Supplier> Suppliers, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? search = null)
        {
            var query = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    (s.Name != null && s.Name.Contains(search)) ||
                    (s.Email != null && s.Email.Contains(search)) ||
                    (s.Phone != null && s.Phone.Contains(search)) ||
                    (s.Address != null && s.Address.Contains(search))
                );
            }

            int totalCount = await query.CountAsync();

            query = query.OrderBy(s => s.Name);

            var suppliers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (suppliers, totalCount);
        }


        public async Task<(bool success, string message)> UpdateAsync(Supplier supplier)
        {
            var existing = await _context.Suppliers.FindAsync(supplier.SupplierId);
            if (existing == null) return (false, "Không tìm thấy nhà cung cấp");

            existing.Name = supplier.Name;
            existing.Email = supplier.Email;
            existing.Phone = supplier.Phone;
            existing.Address = supplier.Address;

            try
            {
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi lưu dữ liệu: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return (false, "Không tìm thấy nhà cung cấp");

            try
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                return (true, "Xóa nhà cung cấp thành công");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi xóa: " + ex.Message);
            }
        }
    }
}
