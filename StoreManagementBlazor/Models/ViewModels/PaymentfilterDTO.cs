// StoreManagementBlazor.Models.ViewModels/PaymentFilterDTO.cs

using System.ComponentModel.DataAnnotations;

namespace StoreManagementBlazor.Models.ViewModels
{
    public class PaymentFilterDTO
    {
        public string SearchOrderId { get; set; } = "";
        public string SearchCustomer { get; set; } = "";
        public string SearchDate { get; set; } = ""; // Format dd/MM/yyyy
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string Method { get; set; } = "all"; 
        public string SortBy { get; set; } = "id_desc";
        
        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}