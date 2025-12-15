// StoreManagementBlazor.Services/PaymentsService.cs

using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;
using StoreManagementBlazor.Models.ViewModels;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace StoreManagementBlazor.Services
{
    public class PaymentsService
    {
        private readonly ApplicationDbContext _db;

        public PaymentsService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ====================================================================================
        // I. Hỗ trợ Trang Index (Danh sách, Lọc, Sắp xếp, Phân trang)
        // ====================================================================================

        public async Task<PagedResult<Payment>> GetPaymentsAsync(PaymentFilterDTO filter)
{
            var query = _db.Payments
                .Include(p => p.Order!)
                    .ThenInclude(o => o.Customer)
                .AsQueryable();

            // 1. Lọc theo Mã đơn hàng
            if (!string.IsNullOrWhiteSpace(filter.SearchOrderId) && int.TryParse(filter.SearchOrderId, out int orderId))
                query = query.Where(p => p.OrderId == orderId);

            // 2. Lọc theo Tên khách hàng
            if (!string.IsNullOrWhiteSpace(filter.SearchCustomer))
                query = query.Where(p => p.Order != null && p.Order.Customer != null && p.Order.Customer.Name.Contains(filter.SearchCustomer));

            // 3. Lọc theo Phương thức
            if (!string.IsNullOrWhiteSpace(filter.Method) && filter.Method != "all")
                query = query.Where(p => p.PaymentMethod == filter.Method);

            // 4. Lọc theo Khoảng tiền
            if (filter.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= filter.MinAmount.Value);
            
            if (filter.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

            // 5. Lọc theo Ngày
            if (!string.IsNullOrWhiteSpace(filter.SearchDate) && DateTime.TryParseExact(filter.SearchDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                // Lọc trong ngày đó (từ 00:00:00 đến 23:59:59)
                var nextDay = date.AddDays(1);
                query = query.Where(p => p.PaymentDate >= date && p.PaymentDate < nextDay);
            }
            
            // 6. Sắp xếp (Chỉ hỗ trợ Id_desc/Date_desc như Controller cũ)
            query = filter.SortBy switch
            {
                "id_asc" => query.OrderBy(p => p.PaymentId),
                "date_asc" => query.OrderBy(p => p.PaymentDate),
                _ => query.OrderByDescending(p => p.PaymentId), // Mặc định: id_desc
            };

            // 7. Phân trang
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);
            
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Payment>
            {
                Items = items,
                TotalItems = totalItems,
                Page = filter.Page,          // ✅ ĐÚNG MODEL
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };
        }

        // ====================================================================================
        // II. Hỗ trợ Trang Chi tiết & Xóa
        // ====================================================================================
        
        public async Task<Payment?> GetPaymentDetailsAsync(int id)
        {
            return await _db.Payments
                .Include(p => p.Order!)
                    .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // Logic Xóa (Dựa trên PaymentsController.cs)
        public async Task<(bool success, string message)> DeletePaymentAsync(int id)
        {
            var payment = await _db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return (false, $"Không tìm thấy giao dịch thanh toán #{id}!");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var order = payment.Order;

                // 1. Cập nhật trạng thái Order về "pending" nếu đã "completed"
                if (order != null && order.Status == "completed")
                {
                    order.Status = "pending";
                    _db.Orders.Update(order);
                }

                // 2. Xóa Payment
                _db.Payments.Remove(payment);
                
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Đã xóa thành công thanh toán #{id} và cập nhật trạng thái đơn hàng #{order?.OrderId} về 'Chờ thanh toán'!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Ghi log chi tiết lỗi tại đây
                return (false, $"Lỗi hệ thống khi xóa thanh toán #{id}: {ex.Message}");
            }
        }

        public async Task<(bool success, string message, int orderId)> CreateOrderWithPaymentAsync(
        string userId,
        List<CartItem> cartItems,
        decimal discountAmount,
        string paymentMethod)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            int? customerId = null;
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedId))
            {
                customerId = parsedId;
            }

            // 1. Tạo Order
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = "pending",
                TotalAmount = cartItems.Sum(i => i.Subtotal) - discountAmount,
                DiscountAmount = discountAmount
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(); // để có OrderId

            // 2. Tạo OrderItem
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Subtotal = item.Subtotal
                };
                _db.OrderItems.Add(orderItem);

                // Trừ tồn kho nếu cần
                var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                if (inventory != null)
                {
                    inventory.Quantity -= item.Quantity;
                    _db.Inventories.Update(inventory);
                }
            }

            // 3. Tạo Payment
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = order.TotalAmount ?? 0m,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);

            // 4. Commit
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Thanh toán thành công đơn hàng #{order.OrderId}", order.OrderId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Lỗi khi lưu đơn hàng: {ex.Message}", 0);
        }
    }


    }
}