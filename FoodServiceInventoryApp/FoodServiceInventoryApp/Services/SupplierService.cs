using FoodServiceInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly FoodServiceDbContext _context;

        public SupplierService(FoodServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers.OrderBy(s => s.CompanyName).ToListAsync();
        }

        public async Task<Supplier> GetSupplierByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<Supplier> AddSupplierAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task UpdateSupplierAsync(Supplier supplier)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSupplierAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SupplierExistsAsync(int id)
        {
            return await _context.Suppliers.AnyAsync(s => s.SupplierId == id);
        }

        public async Task<Supplier> GetSupplierByNameAsync(string name)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName.ToLower() == name.ToLower());
        }
    }
}