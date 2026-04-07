
using SV22T1020158.Models.Catalog;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020158.DataLayers.Interfaces
{
    public interface IProductAttributeRepository
    {
        Task<IList<ProductAttribute>> ListAsync(int productID);

        Task<long> AddAsync(ProductAttribute data);

        Task<bool> UpdateAsync(ProductAttribute data);

        Task<bool> DeleteAsync(long attributeID);
    }
}