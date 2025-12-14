using StoreManagementBlazor.Models;

public class CartService
{
    private List<OrderItem> _items = new();

    public IReadOnlyList<OrderItem> Items => _items;

    public void AddProduct(Product product, int quantity = 1)
    {
        var existing = _items.FirstOrDefault(x => x.ProductId == product.ProductId);
        if (existing != null)
        {
            existing.Quantity += quantity;
            existing.Subtotal = existing.Quantity * existing.Price;
        }
        else
        {
           _items.Add(new OrderItem
            {
                ProductId = product.ProductId,
                Product = product,   
                Price = product.Price,
                Quantity = quantity,
                Subtotal = product.Price * quantity
            });     

        }
    }

    public void RemoveProduct(int productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
            _items.Remove(item);
    }
    

    public void Clear() => _items.Clear();

    public decimal TotalAmount => _items.Sum(x => x.Subtotal);

    
}
