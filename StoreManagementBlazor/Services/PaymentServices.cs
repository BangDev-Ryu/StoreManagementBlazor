// StoreManagementBlazor.Services/PaymentsService.cs

using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;
using StoreManagementBlazor.Models.ViewModels;
using System.Globalization;

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
        // I. Trang Index – Danh sách / Lọc / Sắp xếp / Phân trang
        // ====================================================================================
        public async Task<PagedResult<Payment>> GetPaymentsAsync(PaymentFilterDTO filter)
        {
            var query = _db.Payments
                .Include(p => p.Order!)
                    .ThenInclude(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchOrderId)
                && int.TryParse(filter.SearchOrderId, out int orderId))
            {
                query = query.Where(p => p.OrderId == orderId);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchCustomer))
            {
                query = query.Where(p =>
                    p.Order != null &&
                    p.Order.Customer != null &&
                    p.Order.Customer.Name.Contains(filter.SearchCustomer));
            }

            if (!string.IsNullOrWhiteSpace(filter.Method) && filter.Method != "all")
            {
                query = query.Where(p => p.PaymentMethod == filter.Method);
            }

            if (filter.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchDate)
                && DateTime.TryParseExact(
                    filter.SearchDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                var nextDay = date.AddDays(1);
                query = query.Where(p => p.PaymentDate >= date && p.PaymentDate < nextDay);
            }

            query = filter.SortBy switch
            {
                "id_asc" => query.OrderBy(p => p.PaymentId),
                "id_desc" => query.OrderByDescending(p => p.PaymentId),

                "date_asc" => query.OrderBy(p => p.PaymentDate),
                "date_desc" => query.OrderByDescending(p => p.PaymentDate),

                "amount_asc" => query.OrderBy(p => p.Amount),
                "amount_desc" => query.OrderByDescending(p => p.Amount),
                _ => query.OrderByDescending(p => p.PaymentId)
            };

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
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };
        }

        // ====================================================================================
        // II. Chi tiết
        // ====================================================================================
        public async Task<Payment?> GetPaymentDetailsAsync(int id)
        {
            return await _db.Payments
                .Include(p => p.Order!)
                    .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // ====================================================================================
        // III. THANH TOÁN ĐƠN HÀNG 
        // ====================================================================================
        public async Task<(bool success, string message)> PayOrderAsync(int orderId, string method)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var order = await _db.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return (false, "Không tìm thấy đơn hàng!");

                // ❌ Không cho thanh toán lại
                if (order.Status == "paid")
                    return (false, "Đơn hàng đã được thanh toán!");

                // 1️⃣ Tạo payment
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.TotalAmount ?? 0m,
                    PaymentMethod = method,
                    PaymentDate = DateTime.Now
                };

                _db.Payments.Add(payment);

                // 2️⃣ 🔥 UPDATE STATUS ORDER → PAID
                order.Status = "paid";
                _db.Orders.Update(order);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Thanh toán đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi thanh toán: {ex.Message}");
            }
        }

        // ====================================================================================
        // IV. XÓA PAYMENT → ĐƠN HÀNG QUAY VỀ PENDING
        // ====================================================================================
        public async Task<(bool success, string message)> DeletePaymentAsync(int id)
        {
            var payment = await _db.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return (false, $"Không tìm thấy giao dịch thanh toán #{id}!");

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var order = payment.Order;

                if (order != null)
                {
                    order.Status = "pending";
                    _db.Orders.Update(order);
                }

                _db.Payments.Remove(payment);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Đã xóa thanh toán #{id} và cập nhật đơn hàng về 'Chưa thanh toán'");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}");
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
