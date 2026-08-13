using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PharmaPOS_MVC.Controllers;

public abstract class BaseController : Controller
{
	private void Notify(string alert, string message )
	{
		var msg = JsonSerializer.Serialize(new { alert, value = message });
		HttpContext.Items["NotifyMessage"] = msg;
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
    
}
