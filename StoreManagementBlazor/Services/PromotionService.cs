using Microsoft.EntityFrameworkCore;
using StoreManagementBlazor.Models;

namespace StoreManagementBlazor.Services
{
    public class PromotionService
    {
        private readonly ApplicationDbContext _context;

        public PromotionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promotion>> GetAll()
        {
            return await _context.Promotions
                .OrderByDescending(p=> p.StartDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Promotion?> GetPromotionById(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        public async Task CreatePromotion(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePromotion(Promotion promotion)
        {
            var existingPromo = await _context.Promotions.FindAsync(promotion.PromoId);
            if (existingPromo != null)
            {
                // Cập nhật giá trị
                existingPromo.PromoCode = promotion.PromoCode;
                existingPromo.Description = promotion.Description;
                existingPromo.DiscountType = promotion.DiscountType;
                existingPromo.DiscountValue = promotion.DiscountValue;
                existingPromo.StartDate = promotion.StartDate;
                existingPromo.EndDate = promotion.EndDate;
                existingPromo.MinOrderAmount = promotion.MinOrderAmount;
                existingPromo.UsageLimit = promotion.UsageLimit;
                existingPromo.Status = promotion.Status;

                // Lưu xuống DB
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeletePromotion(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo != null)
            {
                _context.Promotions.Remove(promo);
                await _context.SaveChangesAsync();
            }
        }
    }
}
