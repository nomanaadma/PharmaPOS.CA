using System.Text.Json;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PharmaPOS.MVC.Controllers;

public abstract class BaseController : Controller
{
	public override void OnActionExecuted(ActionExecutedContext context)
	{
		var isRedirect = context.Result is RedirectResult or RedirectToActionResult or RedirectToRouteResult or LocalRedirectResult;

		var setInCurrentRequest = HttpContext.Items.ContainsKey("NotifyMessageCurrentRequest");

		if (!isRedirect && setInCurrentRequest)
		{
			TempData.Remove("Message");
		}

		base.OnActionExecuted(context);
	}
	private void Notify(string alert, string message )
	{
		var msg = JsonSerializer.Serialize(new { alert, value = message });
		HttpContext.Items["NotifyMessage"] = msg;
		HttpContext.Items["NotifyMessageCurrentRequest"] = true;
		TempData["Message"] = msg;
	}
	
    protected void NotifySuccess(string message)
    {
	    Notify("success", message);
    }

    protected void NotifyError(string message)
    {
	    Notify("error", message);
    }

    protected void NotifyWarning(string message)
    {
	    Notify("warning", message);
    }

    /// <summary>
    /// Generic error handler that maps ErrorOr errors to appropriate HTTP responses.
    /// </summary>
    /// <param name="errors">The list of errors from the service/domain layer.</param>
    /// <param name="formViewName">The view name to return to when a model is provided (e.g. "BillingForm").</param>
    /// <param name="model">Optional form model to redisplay on validation/conflict errors.</param>
    /// <param name="conflictFieldMap">
    /// Optional dictionary mapping specific error codes to ModelState field names.
    /// When a Conflict error matches a key, it is added to ModelState for the mapped field
    /// instead of being shown as a toast notification.
    /// Example: <c>new() { ["Supplier.EmailNotUnique"] = "Email" }</c>
    /// </param>
    protected IActionResult HandleErrors(
	    List<Error> errors,
	    string formViewName,
	    object? model = null,
	    Dictionary<string, string>? conflictFieldMap = null)
    {
	    var firstError = errors[0];

	    switch (firstError.Type)
	    {
		    case ErrorType.NotFound:
			    NotifyWarning(firstError.Description);
			    return NotFound();

		    case ErrorType.Validation:
		    case ErrorType.Conflict:
			    // If this specific error code has a field mapping and we have a model, 
			    // add it as a ModelState error so it renders next to the field.
			    if (model != null
			        && conflictFieldMap != null
			        && conflictFieldMap.TryGetValue(firstError.Code, out var fieldName))
			    {
				    ModelState.AddModelError(fieldName, firstError.Description);
			    }
			    else
			    {
				    NotifyError(firstError.Description);
			    }

			    return model != null
				    ? View(formViewName, model)
				    : RedirectToAction("Index");

		    default:
			    NotifyError("An unexpected error occurred. Please try again.");
			    return model != null
				    ? View(formViewName, model)
				    : RedirectToAction("Index");
	    }
    }
}
