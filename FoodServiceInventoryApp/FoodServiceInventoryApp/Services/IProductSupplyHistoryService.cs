using FoodServiceInventoryApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public interface IProductSupplyHistoryService
    {
        Task<IEnumerable<ProductSupplyHistory>> GetAllSupplyRecordsAsync();
        Task<ProductSupplyHistory> GetSupplyRecordByIdAsync(int id);
        Task AddSupplyRecordAsync(ProductSupplyHistory record);
        Task UpdateSupplyRecordAsync(ProductSupplyHistory record);
        Task DeleteSupplyRecordAsync(int id);
        Task<IEnumerable<ProductSupplyHistory>> GetSupplyRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ProductSupplyHistory>> GetSupplyRecordsFilteredAsync(DateTime? startDate = null, DateTime? endDate = null, int? productId = null, int? supplierId = null, int? categoryId = null);
    }
}