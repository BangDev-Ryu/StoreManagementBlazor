using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;

namespace StoreManagementBlazor.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Lấy danh sách phân trang + search + sort theo CategoryName
        public async Task<(List<Category> Categories, int TotalCount)> GetPagedAsync(
            int page = 1,
            int pageSize = 5,
            string? search = null,
            bool sortAsc = true)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.CategoryName != null && c.CategoryName.Contains(search));
            }

            int totalCount = await query.CountAsync();

            query = sortAsc ? query.OrderBy(c => c.CategoryName) : query.OrderByDescending(c => c.CategoryName);

            var categories = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<(bool success, string message)> CreateAsync(Category category)
        {
            if (category == null)
                return (false, "Category null");

            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return (false, "Tên danh mục không được để trống");

            bool exists = await _context.Categories.AnyAsync(c => c.CategoryName == category.CategoryName);
            if (exists) return (false, "Tên danh mục đã tồn tại");

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return (true, "Tạo danh mục thành công");
        }

        public async Task<(bool success, string message)> UpdateAsync(Category category)
        {
            var existing = await _context.Categories.FindAsync(category.CategoryId);
            if (existing == null) return (false, "Không tìm thấy danh mục");

            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return (false, "Tên danh mục không được để trống");

            existing.CategoryName = category.CategoryName;

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
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return (false, "Không tìm thấy danh mục");

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return (true, "Xóa danh mục thành công");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi xóa: " + ex.Message);
            }
        }
    }
}
