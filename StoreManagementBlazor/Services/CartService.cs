using StoreManagementBlazor.Models;

public class CartService
{
    private readonly List<OrderItem> _items = new();

    public IReadOnlyList<OrderItem> Items => _items;

    public void AddProduct(Product product, int quantity = 1)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == product.ProductId);

        if (item != null)
        {
            item.Quantity += quantity;
            if (item.Quantity <= 0)
            {
                _items.Remove(item);
                return;
            }
        }
        else
        {
            _items.Add(new OrderItem
            {
                ProductId = product.ProductId,
                Product = product,
                Price = product.Price,
                Quantity = quantity
            });
        }

        item = _items.First(x => x.ProductId == product.ProductId);
        item.Subtotal = item.Price * item.Quantity;
    }

    public void Increase(int productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null) return;

        item.Quantity++;
        item.Subtotal = item.Quantity * item.Price;
    }

    public void Decrease(int productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null) return;

        item.Quantity--;
        if (item.Quantity <= 0)
        {
            _items.Remove(item);
            return;
        }

        item.Subtotal = item.Quantity * item.Price;
    }

    public void Clear() => _items.Clear();

    public decimal TotalAmount => _items.Sum(x => x.Subtotal);
}
