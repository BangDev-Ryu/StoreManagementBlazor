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

        public async Task<(List<Promotion> Promotions, int TotalCount)> GetAll(
            string? searchText = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 5)
        {
            var query = _context.Promotions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(p => p.PromoCode.Contains(searchText));

            if (startDate.HasValue)
                query = query.Where(p => p.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.EndDate <= endDate.Value);

            int totalCount = await query.CountAsync();

            var data = await query
                .Where(p => p.EndDate > DateTime.Now)
                .OrderBy(p => p.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
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

            if(promotion.StartDate < DateTime.Now.Date)
            {
                return (false, "Ngày bắt đầu không được nhỏ hơn ngày hiện tại.");
            }

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return (true, "Tạo khuyến mãi thành công.");
        }

        public async Task<(bool success, string message)> UpdatePromotion(Promotion promotion)
        {
            var existingPromo = await _context.Promotions
                                      .FirstOrDefaultAsync(p => p.PromoId == promotion.PromoId);

            if (existingPromo == null)
                return (false, "Không tìm thấy khuyến mãi.");

            var today = DateTime.Now.Date;

            if (existingPromo.StartDate >= today)
            {
                if (promotion.StartDate < today && promotion.StartDate != existingPromo.StartDate)
                {
                    return (false, "Chương trình chưa diễn ra, ngày bắt đầu phải từ hôm nay trở đi.");
                }
            }
            else
            {
                if (promotion.StartDate != existingPromo.StartDate)
                {
                    return (false, "Không thể thay đổi ngày bắt đầu của chương trình đang diễn ra.");
                }
            }

            existingPromo.PromoCode = promotion.PromoCode;
            existingPromo.Description = promotion.Description;
            existingPromo.DiscountType = promotion.DiscountType;
            existingPromo.DiscountValue = promotion.DiscountValue;
            existingPromo.StartDate = promotion.StartDate;
            existingPromo.EndDate = promotion.EndDate;
            existingPromo.MinOrderAmount = promotion.MinOrderAmount;
            existingPromo.UsageLimit = promotion.UsageLimit;
            existingPromo.Status = promotion.Status;

            try
            {
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi lưu dữ liệu: " + ex.Message);
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
