using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StoreManagementBlazor.Models;
using StoreManagementBlazor.Models.ViewModels;
using StoreManagementBlazor.Services;
using System.Globalization;


namespace StoreManagementBlazor.Services
{
    public class OrdersService
    {
        private readonly ApplicationDbContext _db;

        public OrdersService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ====================================================================================
        // I. Hỗ trợ Trang Index (Danh sách, Lọc, Sắp xếp, Phân trang)
        // ====================================================================================

        public async Task<PagedResult<OrderListDTO>> GetOrdersAsync(OrderFilterDTO filter)
        {
            var query = _db.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            // Lọc theo Tên khách
            if (!string.IsNullOrWhiteSpace(filter.SearchCustomer))
            {
                query = query.Where(o => o.Customer != null && o.Customer.Name.Contains(filter.SearchCustomer));
            }
            
            // Lọc theo Trạng thái
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(o => o.Status == filter.Status);
            }

            // Lọc theo Ngày tạo (Logic từ OrdersController.cs: Index)
            if (DateTime.TryParseExact(filter.SearchDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var searchDate))
            {
                var nextDay = searchDate.AddDays(1);
                query = query.Where(o => o.OrderDate >= searchDate && o.OrderDate < nextDay);
            }

            // Lọc theo Khoảng giá (Logic từ OrdersController.cs: Index)
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= filter.MinPrice.Value);
            }
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= filter.MaxPrice.Value);
            }

            // Sắp xếp (Logic từ OrdersController.cs: Index)
            query = filter.SortBy.ToLower() switch
            {
                "price" => query.OrderByDescending(o => o.TotalAmount).ThenByDescending(o => o.OrderDate),
                "id" => query.OrderByDescending(o => o.OrderId),
                "date" or _ => query.OrderByDescending(o => o.OrderDate),
            };

            // Phân trang
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);
            
            if (filter.Page < 1) filter.Page = 1;
            if (filter.Page > totalPages && totalPages > 0) filter.Page = totalPages;

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OrderListDTO
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Khách lẻ",
                    OrderDate = o.OrderDate,
                    Status = o.Status ?? "pending",
                    TotalAmount = o.TotalAmount ?? 0m,
                    DiscountAmount = o.DiscountAmount ?? 0m
                })
                .ToListAsync();

            return new PagedResult<OrderListDTO>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        
        // ====================================================================================
        // II. Hỗ trợ Trang Create (Đã có logic trong file Create.razor cũ)
        // ====================================================================================

        public async Task<(List<Customer>, List<ProductSelectionDTO>)> GetCustomerAndProductDataAsync()
        {
            var customers = await _db.Customers.OrderBy(c => c.Name).ToListAsync();
            // Logic từ OrdersController.cs: PrepareViewBags
            var products = await _db.Products
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductSelectionDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price
                })
                .ToListAsync();

            return (customers, products);
        }

        public async Task<(decimal rawTotal, decimal discount, string message, bool isValid, int? promotionId)>
            CalculateCartTotalsAsync(List<CartItemDTO> cartItems, string? promoCode)
        {
            // ================== 1. TÍNH TỔNG TIỀN GỐC ==================
            decimal rawTotal = 0m;

            if (!cartItems.Any())
                return (0m, 0m, string.Empty, false, null);

            var productPrices = await _db.Products
                .Where(p => cartItems.Select(i => i.ProductId).Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => p.Price);

            foreach (var item in cartItems)
            {
                if (productPrices.TryGetValue(item.ProductId, out var price))
                {
                    rawTotal += price * item.Quantity;
                }
            }

            // ================== 2. KHỞI TẠO KẾT QUẢ ==================
            bool isValid = false;
            string message = "";
            int? promotionId = null;
            decimal discountAmount = 0m;

            // ================== 3. KHÔNG NHẬP MÃ ==================
            if (string.IsNullOrWhiteSpace(promoCode))
            {
                return (rawTotal, 0m, "Không sử dụng mã khuyến mãi.", false, null);
            }

            promoCode = promoCode.Trim();

            // ================== 4. KIỂM TRA MÃ ==================
            var promotion = await _db.Promotions.FirstOrDefaultAsync(p =>
                p.PromoCode == promoCode &&
                p.Status == "active"
            );
            if (promotion == null)
            {
                return (rawTotal, 0m, "Mã khuyến mãi không tồn tại.", false, null);
            }

            if (promotion.StartDate > DateTime.Now || promotion.EndDate < DateTime.Now)
            {
                return (rawTotal, 0m, "Mã khuyến mãi đã hết hạn.", false, null);
            }

            if (rawTotal < (promotion.MinOrderAmount ?? 0m))
            {
                return (
                    rawTotal,
                    0m,
                    $"Đơn hàng tối thiểu {(promotion.MinOrderAmount ?? 0m):N0} ₫ để áp dụng mã.",
                    false,
                    null
                );
            }

            // ================== 5. TÍNH GIẢM GIÁ ==================
            var discountValue = promotion.DiscountValue ?? 0m;

            if (promotion.DiscountType?.ToLower() == "percent")
            {
                discountAmount = rawTotal * (discountValue / 100m);
            }
            else
            {
                discountAmount = discountValue;
            }

            // ================== 6. HỢP LỆ ==================
            isValid = true;
            promotionId = promotion.PromoId;
            message = $"Áp dụng mã {promotion.PromoCode} thành công.";

            return (rawTotal, discountAmount, message, isValid, promotionId);
        }

        public async Task<(bool success, string message, int? newOrderId)> CreateOrderAsync(OrderCreationDTO creationData)
{
    // Bắt đầu giao dịch để đảm bảo tính toàn vẹn dữ liệu
    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
        // 1. Tải dữ liệu cần thiết: Khách hàng, Khuyến mãi, Sản phẩm
        if (creationData.CustomerId.HasValue && !await _db.Customers.AnyAsync(c => c.CustomerId == creationData.CustomerId))
            return (false, "Khách hàng không tồn tại.", null);

        var promotion = creationData.PromoCode != null 
            ? await _db.Promotions.FirstOrDefaultAsync(p => p.PromoCode == creationData.PromoCode && p.Status == "active" && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now) 
            : null;

        var productIds = creationData.CartItems.Select(i => i.ProductId).ToList();
        
        // CHỈ LẤY PRODUCTS (Không dùng Include(p => p.Inventories) nữa)
        var products = await _db.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync();
        
        // LẤY INVENTORIES RIÊNG và ánh xạ theo ProductId
        var inventoriesMap = await _db.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .ToDictionaryAsync(i => i.ProductId, i => i);

        if (products.Count != creationData.CartItems.Count)
            return (false, "Một số sản phẩm trong giỏ hàng không tồn tại.", null);
        
        // 2. Kiểm tra tồn kho và tính toán tổng tiền
        decimal subTotal = 0;
        decimal discountAmount = 0;
        var orderItems = new List<OrderItem>();
        
        foreach (var cartItem in creationData.CartItems)
        {
            var product = products.First(p => p.ProductId == cartItem.ProductId);
            
            // Lấy tồn kho từ map
            inventoriesMap.TryGetValue(cartItem.ProductId, out var inventory);

            // A. Tái kiểm tra Tồn kho
            if (inventory == null || inventory.Quantity < cartItem.Quantity)
            {
                return (false, $"Sản phẩm {product.ProductName} chỉ còn {inventory?.Quantity ?? 0} sản phẩm trong kho. Vui lòng giảm số lượng.", null);
            }

            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                Price = product.Price, 
                Subtotal = product.Price * cartItem.Quantity
            };
            orderItems.Add(orderItem);
            subTotal += orderItem.Subtotal;
        }

        // 3. Xử lý khuyến mãi (Áp dụng logic kiểm tra MinOrderAmount)
        if (promotion != null)
        {
            discountAmount = CalculateDiscount(subTotal, promotion);
            
            if (discountAmount > 0)
            {
                // Cập nhật lượt dùng khuyến mãi (Tăng used_count)
                promotion.UsedCount = (promotion.UsedCount ?? 0) + 1;
                _db.Promotions.Update(promotion);
            }
            else
            {
                 promotion = null; 
            }
        }
        
        decimal totalAmount = subTotal - discountAmount;
        if (totalAmount < 0) totalAmount = 0;

        // 4. Tạo Order
        var newOrder = new Order
        {
            CustomerId = creationData.CustomerId,
            OrderDate = DateTime.Now,
            Status = "pending", 
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            PromoId = promotion?.PromoId,
            Items = orderItems 
        };

        _db.Orders.Add(newOrder);

        // 5. Trừ tồn kho
        foreach (var item in creationData.CartItems)
        {
            // Trừ tồn kho trực tiếp từ map đã fetch
            if (inventoriesMap.TryGetValue(item.ProductId, out var inventory))
            {
                inventory.Quantity -= item.Quantity;
                _db.Inventories.Update(inventory);
            }
        }

        // 6. Lưu thay đổi và Hoàn tất giao dịch
        await _db.SaveChangesAsync(); 
        await transaction.CommitAsync();

        return (true, $"Tạo đơn hàng #{newOrder.OrderId} thành công.", newOrder.OrderId);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return (false, $"Lỗi hệ thống trong quá trình tạo đơn hàng. Vui lòng kiểm tra log. Chi tiết: {ex.Message}", null); 
    }
}

private decimal CalculateDiscount(decimal subTotal, Promotion promotion)
{
    if (promotion == null)
        return 0m;

    // Check min order
    if (promotion.MinOrderAmount.HasValue &&
        subTotal < promotion.MinOrderAmount.Value)
    {
        return 0m;
    }

    decimal discount = 0m;

    if (promotion.DiscountType == "percent" &&
        promotion.DiscountValue.HasValue)
    {
        discount = subTotal * (promotion.DiscountValue.Value / 100m);
    }
    else if (promotion.DiscountType == "fixed" &&
             promotion.DiscountValue.HasValue)
    {
        discount = promotion.DiscountValue.Value;
    }

    // Không cho giảm vượt tổng tiền
    if (discount > subTotal)
        discount = subTotal;

    return discount;
}



        // ====================================================================================
        // III. Hỗ trợ Trang Details (Chi tiết đơn hàng)
        // ====================================================================================

        public async Task<OrderDetailsDTO?> GetOrderDetailsAsync(int id)
        {
            // Eager loading các entity liên quan (Logic từ OrdersController.cs: Details)
            var order = await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Promotion)
                .Include(o => o.Payment) 
                .Include(o => o.Items!)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return null;

            return new OrderDetailsDTO
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer?.Name ?? "Khách lẻ",
                OrderDate = order.OrderDate,
                Status = order.Status ?? "pending",
                TotalAmount = order.TotalAmount ?? 0m,
                DiscountAmount = order.DiscountAmount ?? 0m,
                PromoCode = order.Promotion?.PromoCode,
                Payment = order.Payment,
                Items = order.Items?.Select(i => new OrderItemDTO
                {
                    ProductName = i.Product?.ProductName ?? "Sản phẩm đã xóa",
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Subtotal = i.Subtotal
                }).ToList() ?? new()
            };
        }

        // ====================================================================================
        // IV. Hỗ trợ Thanh toán nhanh (OrdersController.cs: ProcessOrderPayment)
        // ====================================================================================

        public async Task<(bool success, string message)> MarkOrderAsPaidAsync(int id, string paymentMethod)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return (false, "Đơn hàng không tồn tại!");
            if (order.Status == "completed") return (false, "Đơn hàng đã được thanh toán rồi!");
            
            // Tạo bản ghi Payment
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = order.TotalAmount ?? 0m,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);

            // Cập nhật trạng thái Order
            order.Status = "completed";
            await _db.SaveChangesAsync();

            return (true, $"Thanh toán thành công đơn hàng #{order.OrderId} bằng {paymentMethod}!");
        }

        // ====================================================================================
        // V. Hỗ trợ Trang Delete (Xóa và Hoàn trả tồn kho/khuyến mãi)
        // ====================================================================================

        public async Task<(bool success, string message)> DeleteOrderAsync(int id)
        {
            // Logic hoàn trả tồn kho và lượt dùng khuyến mãi (Logic từ OrdersController.cs: DeleteConfirmed)
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Promotion) 
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return (false, "Đơn hàng không tồn tại.");
            
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Hoàn trả tồn kho
                if (order.Items != null)
                {
                    var productIds = order.Items.Select(i => i.ProductId);
                    var inventories = await _db.Inventories
                        .Where(i => productIds.Contains(i.ProductId))
                        .ToDictionaryAsync(i => i.ProductId, i => i);

                    foreach (var item in order.Items)
                    {
                        if (item.ProductId.HasValue && inventories.TryGetValue(item.ProductId.Value, out var inventory))
                        {
                            inventory.Quantity += item.Quantity;
                        }
                    }
                }

                // Hoàn trả lượt dùng mã khuyến mãi
                if (order.PromoId.HasValue)
                {
                    var promotion = await _db.Promotions.FirstOrDefaultAsync(p => p.PromoId == order.PromoId.Value);
                    if (promotion != null && promotion.UsedCount > 0)
                    {
                        promotion.UsedCount -= 1;
                    }
                }

                // Xóa các Payment, OrderItems và Order
                var payments = await _db.Payments.Where(p => p.OrderId == id).ToListAsync();
                _db.Payments.RemoveRange(payments);
                _db.OrderItems.RemoveRange(order.Items!);
                _db.Orders.Remove(order);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Xóa đơn hàng #{id} thành công và đã hoàn trả tồn kho/khuyến mãi.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex); // hoặc logger
                return (false, "Có lỗi hệ thống.");
            }
        }
    }
}