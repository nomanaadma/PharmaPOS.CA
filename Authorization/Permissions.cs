using System.Reflection;

namespace PharmaPOS_MVC.Authorization;

public static class Permissions
{
	
    public static class Dashboard
    {
        public static readonly Permission View = new("dashboard", "Dashboard");
    }

    public static class Customer
    {
        public static readonly Permission List = new("customers", "Customers List");
        public static readonly Permission Add = new("customer/add", "Customer Add");
        public static readonly Permission Edit = new("customer/edit", "Customer Edit");
        public static readonly Permission Delete = new("customer/delete", "Customer Delete");
        public static readonly Permission View = new("customer/view", "Customer View");
    }

    public static class Billing
    {
	    public const string Name = "POS/Bill";
	    
        public static readonly Permission List = new("medicine/billing", "POS/Bill List");
        public static readonly Permission Add = new("medicine/billing/add", "Bill Add");
        public static readonly Permission Edit = new("medicine/billing/edit", "Bill Edit");
        public static readonly Permission Delete = new("medicine/billing/delete", "Bill Delete");
        public static readonly Permission View = new("medicine/billing/view", "Bill View");
    }

    public static class Purchase
    {
        public static readonly Permission List = new("medicine/purchase", "Purchase List");
        public static readonly Permission Add = new("medicine/purchase/add", "Purchase Add");
        public static readonly Permission Edit = new("medicine/purchase/edit", "Purchase Edit");
        public static readonly Permission Delete = new("medicine/purchase/delete", "Purchase Delete");
        public static readonly Permission View = new("medicine/purchase/view", "Purchase View");
    }

    public static class Stock
    {
	    public const string Name = "Stock Adjustment";
	    
        public static readonly Permission View = new("medicine/stock", "Stock List");
        public static readonly Permission Delete = new("medicine/stock/delete", "Stock Delete");
    }

    public static class Medicine
    {
        public static readonly Permission List = new("medicines", "Medicine List");
        public static readonly Permission Add = new("medicine/add", "Medicine Add");
        public static readonly Permission Edit = new("medicine/edit", "Medicine Edit");
        public static readonly Permission Delete = new("medicine/delete", "Medicine Delete");
        public static readonly Permission View = new("medicine/view", "Medicine View");
    }

    public static class Category
    {
	    public const string Name = "Medicine Category";
	    
        public static readonly Permission List = new("medicine/category", "Medicine Category List");
        public static readonly Permission Add = new("medicine/category/add", "Medicine Category Add");
        public static readonly Permission Edit = new("medicine/category/edit", "Medicine Category Edit");
        public static readonly Permission Delete = new("medicine/category/delete", "Medicine Category Delete");
    }
    
    public static class Reports
    {
	    public static readonly Permission View = new("reports", "All Reports");
    }
    
    public static class User
    {
	    public static readonly Permission List = new("users", "User List");
	    public static readonly Permission Add = new("user/add", "User Add");
	    public static readonly Permission Edit = new("user/edit", "User Edit");
	    public static readonly Permission Delete = new("user/delete", "User Delete");
    }
    
    public static class Info
    {
	    public const string Name = "System Info";

	    public static readonly Permission View = new("info", "System Info");
    }
    

    public static class PaymentMethod
    {
	    public const string Name = "Payment Method";
	    
        public static readonly Permission List = new("paymentmethod", "Payment Methods");
        public static readonly Permission Add = new("paymentmethod/add", "Payment Method Add");
        public static readonly Permission Edit = new("paymentmethod/edit", "Payment Method Edit");
        public static readonly Permission Delete = new("paymentmethod/delete", "Payment Method Delete");
    }

    public static class Media
    {
        public static readonly Permission List = new("get/media", "Media on Modal");
        public static readonly Permission Upload = new("media/upload", "Media Upload");
        public static readonly Permission Delete = new("media/delete", "Media Delete");
    }
    
    public static class Suppliers
    {
        public static readonly Permission List = new("suppliers", "Suppliers List");
        public static readonly Permission Add = new("supplier/add", "Supplier Add");
        public static readonly Permission Edit = new("supplier/edit", "Supplier Edit");
        public static readonly Permission Delete = new("supplier/delete", "Supplier Delete");
    }
   
    
    public static string[] GetAll()
    {
	    return typeof(Permissions)
		    .GetNestedTypes(BindingFlags.Public)
		    .SelectMany(Get)
		    .ToArray();
    }
    
    public static string[] Get(Type group)
    {
	    return group.GetFields(BindingFlags.Public | BindingFlags.Static)
		    .Where(f => f.FieldType == typeof(Permission))
		    .Select(f => ((Permission)f.GetValue(null)!).Key)
		    .ToArray();
    }
    
    public static string[] Get(params Type[] groups)
    {
	    return groups
		    .SelectMany(Get)
		    .ToArray();
    }
    
    public static Dictionary<string, List<Permission>> GetPermissionsTemplate()
    {
	    var roles = new Dictionary<string, List<Permission>>();

	    // Loop through all the nested classes (Dashboard, Customer, etc.)
	    var nestedTypes = typeof(Permissions).GetNestedTypes(BindingFlags.Public);

	    foreach (var groupType in nestedTypes)
	    {
		    // 1. Determine the Group Name
		    var groupName = groupType.Name; // Default to class name
        
		    // Look for `public const string Name`
		    var nameConstant = groupType.GetField("Name", BindingFlags.Public | BindingFlags.Static);
		    if (nameConstant != null && nameConstant.IsLiteral && nameConstant.FieldType == typeof(string))
		    {
			    groupName = (string)nameConstant.GetRawConstantValue()!;
		    }

		    // 2. Get all permission records inside this class
		    var groupPermissions = new List<Permission>();
        
		    var permissionFields = groupType.GetFields(BindingFlags.Public | BindingFlags.Static)
			    .Where(f => f.FieldType == typeof(Permission));

		    foreach (var field in permissionFields)
		    {
			    var permission = (Permission)field.GetValue(null)!;
			    groupPermissions.Add(permission);
		    }

		    // Only add if the group actually has permissions
		    if (groupPermissions.Count > 0)
		    {
			    roles.Add(groupName, groupPermissions);
		    }
	    }

	    return roles;
    }
    
}