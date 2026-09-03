using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Razor.Pages.Media;

[Authorize(Policy = Permissions.Media.Upload)]
[IgnoreAntiforgeryToken]
public class UploadModel(SettingService settingService) : PageModel
{
    public async Task<IActionResult> OnPostAsync(IFormFile? file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return new JsonResult(new { error = true, message = "No file uploaded." });
        }

        var uploadResult = await settingService.UploadMediaFileAsync(file, ct);
        if (uploadResult.IsError)
            return new JsonResult(new { error = true, message = uploadResult.Errors[0].Description });

        var (media, width, height) = uploadResult.Value;
        
        var canDelete = User.HasClaim("Permission", Permissions.Media.Delete);

        var deleteHtml = canDelete
            ? """<a class="remove" data-toggle="tooltip" data-placement="top" title="Remove"><i class="las la-trash-alt"></i></a>"""
            : string.Empty;

        var mediaHtml = $"""
                         <div class="block">
                             {deleteHtml}
                             <input type="radio" class="block-id" value="{media.Id}">
                             <div class="picture">
                                 <img src="/uploads/{media.Filename}" title="{media.Filename}">
                                 <input type="radio" id="{media.Filename}" value="{media.Filename}">
                                 <label for="{media.Filename}">{media.Filename}</label>
                             </div>
                             <label class="font-12">Image Size : {width}*{height}</label>
                         </div>
                         """;

        return new JsonResult(new { error = false, name = media.Filename, id = media.Id, media = mediaHtml });
    }
}
