using Mapster;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Settings.Queries.GetRawSettings;

public class GetRawSettingsQueryHandler(
	ISettingRepository settingRepo) : IRequestHandler<GetRawSettingsQuery, SettingResponse?>
{
	public async Task<SettingResponse?> Handle(GetRawSettingsQuery query, CancellationToken ct)
	{
		var settings = await settingRepo.GetRawSettingsAsync(ct);
		var settingResponse = settings?.Adapt<SettingResponse>();
		return settingResponse;
	}
}
