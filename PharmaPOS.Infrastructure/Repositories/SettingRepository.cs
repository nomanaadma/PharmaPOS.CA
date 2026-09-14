using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.SettingAggregate;
using PharmaPOS.Infrastructure.Data;

namespace PharmaPOS.Infrastructure.Repositories;

public class SettingRepository(ApplicationDbContext context) : Repository<Setting>(context), ISettingRepository
{
	public async Task<Setting?> GetRawSettingsAsync(CancellationToken ct = default)
	{
		return await DbSet.FirstOrDefaultAsync(ct);
	}

	public async Task<Setting?> GetSettingsAsync(CancellationToken ct = default)
	{
		return await DbSet.AsNoTracking().OrderBy(s => s.Id).FirstOrDefaultAsync(ct);
	}
}
