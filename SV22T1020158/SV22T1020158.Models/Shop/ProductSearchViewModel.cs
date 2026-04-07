using SV22T1020158.Models.Common;
using SV22T1020158.Models.Catalog;

namespace SV22T1020158.Shop.Models
{
    public class ProductSearchViewModel
    {
        public ProductSearchInput SearchInput { get; set; } = new();
        public PagedResult<Product> SearchResult { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}