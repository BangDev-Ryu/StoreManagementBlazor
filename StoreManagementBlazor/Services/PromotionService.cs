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
                .OrderBy(p=> p.StartDate)
                .ToListAsync();
        }

        public async Task<Promotion?> GetPromotionById(int id)
        {
            return await _context.Promotions
                        .FirstOrDefaultAsync(p => p.PromoId == id);
        }

        public async Task<(bool success, string message)> CreatePromotion(Promotion promotion)
        {
            bool exists = await _context.Promotions
                                        .AnyAsync(p => p.PromoCode == promotion.PromoCode);

            if (exists)
            {
                return (false, "Mã khuyến mãi đã tồn tại.");
            }

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return (true, "Tạo khuyến mãi thành công.");
        }
        private bool IsEqual(Promotion oldP, Promotion newP)
        {
            return
                oldP.PromoCode == newP.PromoCode &&
                oldP.Description == newP.Description &&
                oldP.DiscountType == newP.DiscountType &&
                oldP.DiscountValue == newP.DiscountValue &&
                oldP.StartDate.Date == newP.StartDate.Date &&
                oldP.EndDate.Date == newP.EndDate.Date &&
                oldP.MinOrderAmount == newP.MinOrderAmount &&
                oldP.UsageLimit == newP.UsageLimit &&
                oldP.Status == newP.Status;
        }

        public async Task<(bool success, string message)> UpdatePromotion(Promotion promotion)
        {
            var existingPromo = await _context.Promotions.FindAsync(promotion.PromoId);

            if (existingPromo == null)
                return (false, "Không tìm thấy khuyến mãi.");

            if (IsEqual(existingPromo, promotion))
                return (true, "Không có thay đổi nào để cập nhật.");

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

            return (true, "Cập nhật thành công.");
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
