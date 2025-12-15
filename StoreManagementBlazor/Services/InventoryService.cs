using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;

namespace StoreManagementBlazor.Services
{
    public class InventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Inventory>> GetAll()
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .ToListAsync();
        }

        public async Task<Inventory?> GetInventoryById(int id)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.InventoryId == id);
        }



        public async Task UpdateInventory(Inventory inventory)
        {
            var existing = await _context.Inventories.FirstOrDefaultAsync(i => i.InventoryId == inventory.InventoryId);
            if (existing == null) return;

            existing.Quantity = inventory.Quantity;
            existing.UpdatedAt = inventory.UpdatedAt;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteInventory(int id)
        {
            var invent = await _context.Inventories.FindAsync(id);
            if (invent != null)
            {
                _context.Inventories.Remove(invent);
                await _context.SaveChangesAsync();
            }
        }
    }
}
