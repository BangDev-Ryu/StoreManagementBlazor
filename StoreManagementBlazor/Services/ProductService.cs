using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;

namespace StoreManagementBlazor.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================================================
        // ===================== PRODUCT ====================
        // ==================================================

        // GET ALL PRODUCTS
        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // GET PRODUCT BY ID
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        // ADD PRODUCT
        public async Task<bool> AddAsync(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // UPDATE PRODUCT
        public async Task<bool> UpdateAsync(Product product)
        {
            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // DELETE PRODUCT
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var product = await GetByIdAsync(id);
                if (product == null) return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // SEARCH PRODUCT (NAME + BARCODE)
        public async Task<List<Product>> SearchAsync(string keyword)
        {
            keyword = keyword?.ToLower() ?? "";

            return await _context.Products
                .Where(p =>
                    p.ProductName.ToLower().Contains(keyword) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(keyword))
                )
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ==================================================
        // ===================== CATEGORY ===================
        // ==================================================

        // GET ALL CATEGORIES (FOR DROPDOWN)
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        // GET CATEGORY NAME BY ID (OPTIONAL)
        public async Task<string?> GetCategoryNameAsync(int? categoryId)
        {
            if (!categoryId.HasValue) return null;

            return await _context.Categories
                .Where(c => c.CategoryId == categoryId.Value)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync();
        }

        // ==================================================
        // ===================== SUPPLIER ===================
        // ==================================================

        // GET ALL SUPPLIERS (FOR DROPDOWN)
        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        // GET SUPPLIER NAME BY ID (OPTIONAL)
        public async Task<string?> GetSupplierNameAsync(int? supplierId)
        {
            if (!supplierId.HasValue) return null;

            return await _context.Suppliers
                .Where(s => s.SupplierId == supplierId.Value)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
        }
    }
}
