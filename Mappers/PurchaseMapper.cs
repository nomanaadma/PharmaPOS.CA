using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

public static class PurchaseMapper
{
    public static IEnumerable<object> MapToPurchases(IEnumerable<Purchase> purchases, bool canView, bool canEdit, bool canDelete)
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
                         <a href="/Medicines/Purchase/View/{0}">
                             <i class="las la-laptop pr-2"></i>View
                         </a>
                     </li>
                     """);
		        }

		        if (canEdit)
		        {
			        actionTemplateBuilder.Append("""
                     <li>
                         <a href="/Medicines/Purchase/Edit/{0}">
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
        

        return purchases.Select(mp =>
        {
	        var actionHtml = canAction 
		        ? actionTemplate.Replace("{0}", mp.Id.ToString()) 
		        : string.Empty;
            
            return new
            {
                id = mp.Id,
                name = mp.Supplier.Name,
                total = "Rs " + mp.Total.ToString("F2"),
                tax = "Rs " + mp.Tax.ToString("F2"),
                discount_value = "Rs " + mp.DiscountValue.ToString("F2"),
                amount = "Rs " + mp.Amount.ToString("F2"),
                date = mp.Date.ToString("yyyy-MM-dd"),
                action = actionHtml
            };
        }).ToList();
    }

    public static PurchaseFormModel MapToPurchaseForm(Purchase purchase, IEnumerable<SelectListItem> suppliers)
    {
        return new PurchaseFormModel
        {
            Id = purchase.Id,
            Supplier = purchase.SupplierId,
            Date = purchase.Date,
            Note = purchase.Note,
            Total = purchase.Total,
            Tax = purchase.Tax,
            Discount = purchase.DiscountValue,
            Amount = purchase.Amount,
            Suppliers = suppliers,
            Items = purchase.Batches.Select(b => new PurchaseItemFormModel
            {
                Id = b.Id,
                MedicineId = b.MedicineId,
                Name = b.Medicine.Name,
                Batch = b.BatchName,
                Expiry = b.Expiry,
                Pqty = b.Pqty,
                Qty = b.Qty,
                Purchaseprice = b.PurchasePrice,
                Saleprice = b.SalePrice,
                Gross = b.Gross,
                Discounttype = b.DiscountType,
                Discount = b.Discount,
                Discountvalue = b.DiscountValue,
                Tax = b.Tax,
                Taxprice = b.TaxPrice,
                Price = b.Price
            }).ToList()
        };
    }
}
