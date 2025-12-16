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

                // 🔥 AUTO GENERATE BARCODE
                if (string.IsNullOrWhiteSpace(product.Barcode))
                {
                    product.Barcode = await GenerateBarcodeAsync();
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<Product>> SearchFilterAsync(
    string? keyword,
    int? categoryId,
    int? supplierId,
    decimal? minPrice,
    decimal? maxPrice
)
        {
            IQueryable<Product> query = _context.Products;

            // 🔍 Tìm theo tên + barcode
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(keyword) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(keyword))
                );
            }

            // 📂 Danh mục
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            // 🚚 Nhà cung cấp
            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId);

            // 💰 Giá từ
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            // 💰 Giá đến
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
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
        // ========================
        // AUTO GENERATE BARCODE
        // ========================
        private async Task<string> GenerateBarcodeAsync()
        {
            const long START = 8900000000000;

            var barcodes = await _context.Products
                .Where(p => p.Barcode != null)
                .Select(p => p.Barcode!)
                .ToListAsync();

            var maxBarcode = barcodes
                .Where(b => long.TryParse(b, out _))
                .Select(long.Parse)
                .DefaultIfEmpty(START)
                .Max();

            return (maxBarcode + 1).ToString();
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
