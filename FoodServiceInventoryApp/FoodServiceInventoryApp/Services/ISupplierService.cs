using FoodServiceInventoryApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier> GetSupplierByIdAsync(int id);
        Task<Supplier> AddSupplierAsync(Supplier supplier);
        Task UpdateSupplierAsync(Supplier supplier);
        Task DeleteSupplierAsync(int id);
        Task<bool> SupplierExistsAsync(int id);
        Task<Supplier> GetSupplierByNameAsync(string name);
    }
}