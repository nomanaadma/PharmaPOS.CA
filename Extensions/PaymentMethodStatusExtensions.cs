using PharmaPOS_MVC.Domain.Enums;

namespace PharmaPOS_MVC.Extensions;

public static class PaymentMethodStatusExtensions
{
	public static string GetCssClass(this PaymentMethodStatus status)
	{
		return status == PaymentMethodStatus.Active ? "label-success" : "label-danger";
	}
}
