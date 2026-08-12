using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Extensions;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

public static class BillingMapper
{
    public static IEnumerable<object> MapToBills(IEnumerable<Bill> bills, bool canView, bool canEdit, bool canDelete)
    {
        var canAction = canView || canEdit || canDelete;
        
        var actionTemplateBuilder = new StringBuilder();

        if (canAction)
        {
	        if (canView || canEdit)
	        {
		        actionTemplateBuilder.Append("""
                 <div class="dropdown d-inline-block">
                     <a class="text-primary edit dropdown-toggle" data-toggle="dropdown">
                         <i class="las la-ellipsis-h"></i>
                     </a>
                     <ul class="dropdown-menu dropdown-menu-right">
                 """);

		        if (canView)
		        {
			        actionTemplateBuilder.Append("""
                     <li>
                         <a href="/Medicines/Billing/View/{0}">
                             <i class="las la-laptop pr-2"></i>View
                         </a>
                     </li>
                     """);
		        }

		        if (canEdit)
		        {
			        actionTemplateBuilder.Append("""
                     <li>
                         <a href="/Medicines/Billing/Edit/{0}">
                             <i class="las la-edit pr-2"></i>Edit
                         </a>
                     </li>
                     """);
		        }

		        actionTemplateBuilder.Append("""
                     </ul>
                 </div>
                 """);
	        }

	        if (canDelete)
	        {
		        actionTemplateBuilder.Append("""
                 <a class="table-delete text-danger delete" data-toggle="tooltip" title="Delete">
                                     <i class="las la-trash-alt"></i>
                                     <input type="hidden" value="{0}">
                 </a>
                 """);
	        }
        }
        
        // 2. Convert the builder to a final template string ONCE.
        var actionTemplate = actionTemplateBuilder.ToString();

        return bills.Select(mb =>
        {
	        
	        // Simply replace the placeholder with the actual ID.
	        // If there are no actions, string.Replace is never called on an empty string anyway.
	        var actionHtml = canAction 
		        ? actionTemplate.Replace("{0}", mb.Id.ToString()) 
		        : string.Empty;

            return new
            {
                id = mb.Id,
                name = mb.Name,
                subtotal = mb.Subtotal.ToCrF2(),
                tax = mb.Tax.ToCrF2(),
                discount_value = mb.DiscountValue.ToCrF2(),
                amount = mb.Amount.ToCrF2(),
                bill_date = mb.BillDate.Format(),
                action = actionHtml.ToString()
            };
        }).ToList();
    }

    public static BillingFormModel MapToBillingForm(Bill bill, IEnumerable<SelectListItem> paymentMethods)
    {
        return new BillingFormModel
        {
            Id = bill.Id,
            CustomerId = bill.CustomerId,
            Name = bill.Name,
            Email = bill.Email,
            Mobile = bill.Mobile,
            Method = bill.MethodId,
            BillDate = bill.BillDate,
            Subtotal = bill.Subtotal,
            Tax = bill.Tax,
            DiscountValue = bill.DiscountValue,
            Amount = bill.Amount,
            Note = bill.Note,
            PaymentMethods = paymentMethods,
            Items = bill.Items.Select(bi => new BillingItemFormModel
            {
                Id = bi.Id,
                MedicineId = bi.MedicineId,
                Name = bi.Medicine.Name,
                Batch = bi.BatchId,
                BatchName = bi.Batch.BatchName,
                Expiry = bi.Batch.Expiry,
                Qty = bi.Pricing.Qty,
                Saleprice = bi.Pricing.SalePrice,
                Gross = bi.Pricing.Gross,
                Discounttype = bi.Pricing.DiscountType,
                Discount = bi.Pricing.Discount,
                Discountvalue = bi.Pricing.DiscountValue,
                Tax = bi.Pricing.Tax,
                Taxprice = bi.Pricing.TaxPrice,
                Price = bi.Pricing.Price,
                Batches = bi.Medicine.Batches.Select(mb => new BillingItemBatchesFormModel
                {
                    Id = mb.Id,
                    Qty = mb.Pricing.Qty,
                    Batch = mb.BatchName,
                    Sold = mb.Sold
                }).ToList()
            }).ToList()
        };
    }

    public static BillingViewModel MapToBillDetailsViewModel(Bill bill)
    {
        return new BillingViewModel
        {
            Id = bill.Id,
            Name = bill.Name,
            Email = bill.Email,
            Mobile = bill.Mobile,
            BillDate = bill.BillDate,
            PaymentMethodName = bill.PaymentMethod?.Name,
            Subtotal = bill.Subtotal,
            DiscountValue = bill.DiscountValue,
            Tax = bill.Tax,
            Amount = bill.Amount,
            Note = bill.Note,
            Items = bill.Items.Select(i => new BillingItemViewModel
            {
                MedicineName = i.Medicine?.Name ?? "",
                BatchName = i.Batch?.BatchName ?? "",
                Expiry = i.Batch?.Expiry ?? "",
                Qty = i.Pricing.Qty,
                SalePrice = i.Pricing.SalePrice,
                Gross = i.Pricing.Gross,
                DiscountValue = i.Pricing.DiscountValue,
                TaxPrice = i.Pricing.TaxPrice,
                Price = i.Pricing.Price
            }).ToList()
        };
    }
}
