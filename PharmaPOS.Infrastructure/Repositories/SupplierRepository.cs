using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.SupplierAggregate;
using PharmaPOS.Infrastructure.Data;

namespace PharmaPOS.Infrastructure.Repositories;

public class SupplierRepository(ApplicationDbContext context) : Repository<Supplier>(context), ISupplierRepository
{
	public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken ct = default)
	{
		return await DbSet.AnyAsync(s => s.Email == email && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
	}

	public async Task<List<LookupItem>> GetLookupListAsync(CancellationToken ct = default)
	{
		return await DbSet
			.OrderBy(s => s.Name)
			.Select(s => new LookupItem(s.Id.ToString(), s.Name))
			.ToListAsync(ct);
	}
	
	
	
}
