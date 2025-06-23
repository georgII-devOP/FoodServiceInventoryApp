using FoodServiceInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public class ProductSupplyHistoryService : IProductSupplyHistoryService
    {
        private readonly FoodServiceDbContext _context;

        public ProductSupplyHistoryService(FoodServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductSupplyHistory>> GetAllSupplyRecordsAsync()
        {
            return await _context.ProductSupplyHistories
                                 .Include(psh => psh.Product)
                                 .Include(psh => psh.Supplier)
                                 .ToListAsync();
        }

        public async Task<ProductSupplyHistory> GetSupplyRecordByIdAsync(int id)
        {
            return await _context.ProductSupplyHistories
                                 .Include(psh => psh.Product)
                                 .Include(psh => psh.Supplier)
                                 .FirstOrDefaultAsync(psh => psh.SupplyRecordId == id);
        }

        public async Task AddSupplyRecordAsync(ProductSupplyHistory record)
        {
            _context.ProductSupplyHistories.Add(record);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSupplyRecordAsync(ProductSupplyHistory record)
        {
            _context.Entry(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSupplyRecordAsync(int id)
        {
            var record = await _context.ProductSupplyHistories.FindAsync(id);
            if (record != null)
            {
                _context.ProductSupplyHistories.Remove(record);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductSupplyHistory>> GetSupplyRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ProductSupplyHistories
                                 .Include(psh => psh.Product)
                                 .Include(psh => psh.Supplier)
                                 .Where(psh => psh.SupplyDate >= startDate && psh.SupplyDate <= endDate)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<ProductSupplyHistory>> GetSupplyRecordsFilteredAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? productId = null,
            int? supplierId = null,
            int? categoryId = null)
        {
            IQueryable<ProductSupplyHistory> query = _context.ProductSupplyHistories
                                                            .Include(psh => psh.Product)
                                                            .Include(psh => psh.Supplier);

            if (startDate.HasValue)
            {
                query = query.Where(s => s.SupplyDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                DateTime endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.SupplyDate <= endOfDay);
            }
            if (productId.HasValue && productId.Value != 0)
            {
                query = query.Where(s => s.ProductId == productId.Value);
            }
            if (supplierId.HasValue && supplierId.Value != 0)
            {
                query = query.Where(s => s.SupplierId == supplierId.Value);
            }
            if (categoryId.HasValue && categoryId.Value != 0)
            {
                query = query.Where(s => s.Product.CategoryId == categoryId.Value);
            }

            return await query.OrderByDescending(s => s.SupplyDate).ToListAsync();
        }
    }
}