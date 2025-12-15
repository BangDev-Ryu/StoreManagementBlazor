using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;

public class CartService
{
    private readonly ApplicationDbContext _context;

    // Nếu cần customerId, lấy từ login/user service, không truyền qua constructor
    private int _customerId => 1; // TODO: lấy từ authentication

    // Giỏ hàng hiện tại
    public List<CartItem> Items { get; set; } = new List<CartItem>();

    // Danh sách sản phẩm được chọn để thanh toán
    private List<CartItem> _selectedItems = new List<CartItem>();
    public void SetSelectedItems(List<CartItem> items) => _selectedItems = items;
    public List<CartItem> GetSelectedItems() => _selectedItems;

    public decimal TotalAmount => Items.Sum(item => item.Price * item.Quantity);

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == _customerId && c.Status == "pending");

        Items = cart?.CartItems.ToList() ?? new List<CartItem>();
        return Items;
    }

    public async Task AddProductAsync(Product product, int quantity = 1)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == _customerId && c.Status == "pending");

        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = _customerId,
                Status = "pending",
                TotalAmount = 0
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == product.ProductId);
        if (item != null)
        {
            item.Quantity += quantity;
            item.Subtotal = item.Quantity * item.Price;
        }
        else
        {
            item = new CartItem
            {
                CartId = cart.CartId,
                ProductId = product.ProductId,
                Price = product.Price,
                Quantity = quantity,
                Subtotal = product.Price * quantity
            };
            _context.CartItems.Add(item);
        }

        cart.TotalAmount = cart.CartItems.Sum(ci => ci.Subtotal);

        await _context.SaveChangesAsync();
        await GetCartItemsAsync();
    }

    public async Task IncreaseAsync(int productId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == _customerId && c.Status == "pending");

        var item = cart?.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (item != null)
        {
            item.Quantity++;
            item.Subtotal = item.Quantity * item.Price;
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.Subtotal);
            await _context.SaveChangesAsync();
            await GetCartItemsAsync();
        }
    }

    public async Task DecreaseAsync(int productId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == _customerId && c.Status == "pending");

        var item = cart?.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (item != null)
        {
            item.Quantity--;
            if (item.Quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Subtotal = item.Quantity * item.Price;
            }

            cart.TotalAmount = cart.CartItems.Sum(ci => ci.Subtotal);
            await _context.SaveChangesAsync();
            await GetCartItemsAsync();
        }
    }

    public async Task ClearAsync()
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == _customerId && c.Status == "pending");

        if (cart != null)
        {
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.TotalAmount = 0;
            await _context.SaveChangesAsync();
            Items.Clear();
        }
    }

    public async Task RemoveSelectedItemsAsync()
    {
        if (_selectedItems == null || !_selectedItems.Any())
            return;

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == _customerId && c.Status == "pending");

        if (cart != null)
        {
            foreach (var item in _selectedItems)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == item.ProductId);
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                }
            }

            cart.TotalAmount = cart.CartItems.Sum(ci => ci.Subtotal);
            await _context.SaveChangesAsync();
        }

        _selectedItems.Clear();
        await GetCartItemsAsync();
    }

}
