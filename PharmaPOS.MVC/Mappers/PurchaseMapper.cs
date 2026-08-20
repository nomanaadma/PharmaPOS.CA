using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS.Domain.Entities;
using PharmaPOS.Domain.Enums;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Mappers;

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
                total = mp.Subtotal.ToCrF2(),
                tax = mp.Tax.ToCrF2(),
                discount_value = mp.DiscountValue.ToCrF2(),
                amount = mp.Amount.ToCrF2(),
                date = mp.Date.Format(),
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
            Total = purchase.Subtotal,
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
                Qty = b.Pricing.Qty,
                Purchaseprice = b.PurchasePrice,
                Saleprice = b.Pricing.SalePrice,
                Gross = b.Pricing.Gross,
                Discounttype = b.Pricing.DiscountType,
                Discount = b.Pricing.Discount,
                Discountvalue = b.Pricing.DiscountValue,
                Tax = b.Pricing.Tax,
                Taxprice = b.Pricing.TaxPrice,
                Price = b.Pricing.Price
            }).ToList()
        };
    }

    public static PurchaseViewModel MapToPurchaseView(Purchase purchase)
    {
        return new PurchaseViewModel
        {
            Id = purchase.Id,
            SupplierName = purchase.Supplier?.Name ?? "",
            SupplierEmail = purchase.Supplier?.Email,
            SupplierPhone = purchase.Supplier?.Phone,
            Date = purchase.Date,
            Total = purchase.Subtotal,
            Tax = purchase.Tax,
            DiscountValue = purchase.DiscountValue,
            Amount = purchase.Amount,
            Note = purchase.Note,
            Batches = purchase.Batches.Select(b => new PurchaseBatchViewModel
            {
                MedicineName = b.Medicine?.Name ?? "",
                BatchName = b.BatchName,
                Expiry = b.Expiry,
                Pqty = b.Pqty,
                Qty = b.Pricing.Qty,
                PurchasePrice = b.PurchasePrice,
                SalePrice = b.Pricing.SalePrice,
                Gross = b.Pricing.Gross,
                DiscountValue = b.Pricing.DiscountValue,
                TaxPrice = b.Pricing.TaxPrice,
                Price = b.Pricing.Price
            }).ToList()
        };
    }
}
