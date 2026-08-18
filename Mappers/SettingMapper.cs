using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

public static class SettingMapper
{
    public static SystemInfoViewModel MapToSystemInfo(Setting? rawSetting, Setting siteInfo)
    {
        return new SystemInfoViewModel
        {
            Logo = rawSetting?.Logo,
            LogoIcon = rawSetting?.LogoIcon,
            LgBackground = rawSetting?.LgBackground,
            Favicon = rawSetting?.Favicon,
            Name = siteInfo.Name,
            LegalName = siteInfo.LegalName,
            Mail = siteInfo.Mail,
            Phone = siteInfo.Phone,
            AddressLine1 = siteInfo.Address?.Line1,
            AddressLine2 = siteInfo.Address?.Line2,
            City = siteInfo.Address?.City,
            Country = siteInfo.Address?.Country,
            Postal = siteInfo.Address?.Postal
        };
    }

    public static SupplierViewModel MapToSupplier(Supplier supplier)
    {
        return new SupplierViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address
        };
    }

    public static IEnumerable<SupplierViewModel> MapToSuppliers(IEnumerable<Supplier> suppliers)
    {
        return suppliers.Select(MapToSupplier);
    }

    public static MedicineCategoryViewModel MapToMedicineCategory(MedicineCategory category)
    {
        return new MedicineCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public static IEnumerable<MedicineCategoryViewModel> MapToMedicineCategories(IEnumerable<MedicineCategory> categories)
    {
        return categories.Select(MapToMedicineCategory).ToList();
    }

    public static PaymentMethodViewModel MapToPaymentMethod(PaymentMethod method)
    {
        return new PaymentMethodViewModel
        {
            Id = method.Id,
            Name = method.Name,
            Status = method.Status
        };
    }

    public static IEnumerable<PaymentMethodViewModel> MapToPaymentMethods(IEnumerable<PaymentMethod> methods)
    {
        return methods.Select(MapToPaymentMethod).ToList();
    }
}
