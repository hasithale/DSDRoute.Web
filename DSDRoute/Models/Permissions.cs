namespace DSDRoute.Models
{
    public static class Permissions
    {
        // Order permissions
        public const string Orders_Create = "orders.create";
        public const string Orders_ViewOwn = "orders.view_own";
        public const string Orders_ViewAll = "orders.view_all";
        public const string Orders_Edit = "orders.edit";
        public const string Orders_Delete = "orders.delete";

        // Shop permissions
        public const string Shops_Add = "shops.add";
        public const string Shops_View = "shops.view";
        public const string Shops_Edit = "shops.edit";
        public const string Shops_Delete = "shops.delete";

        // Product permissions
        public const string Products_Create = "products.create";
        public const string Products_View = "products.view";
        public const string Products_Edit = "products.edit";
        public const string Products_Delete = "products.delete";

        // Payment permissions
        public const string Payments_Create = "payments.create";
        public const string Payments_View = "payments.view";
        public const string Payments_Edit = "payments.edit";
        public const string Payments_Delete = "payments.delete";

        // Return permissions
        public const string Returns_Create = "returns.create";
        public const string Returns_View = "returns.view";
        public const string Returns_Edit = "returns.edit";
        public const string Returns_Delete = "returns.delete";

        // Admin permissions
        public const string Admin_FullAccess = "admin.full_access";
        public const string Admin_UserManagement = "admin.user_management";
        public const string Admin_Dashboard = "admin.dashboard";

        public static Dictionary<string, List<string>> RolePermissions = new Dictionary<string, List<string>>
        {
            ["Admin"] = new List<string>
            {
                // All permissions for Admin
                Orders_Create, Orders_ViewOwn, Orders_ViewAll, Orders_Edit, Orders_Delete,
                Shops_Add, Shops_View, Shops_Edit, Shops_Delete,
                Products_Create, Products_View, Products_Edit, Products_Delete,
                Payments_Create, Payments_View, Payments_Edit, Payments_Delete,
                Returns_Create, Returns_View, Returns_Edit, Returns_Delete,
                Admin_FullAccess, Admin_UserManagement, Admin_Dashboard
            },
            ["User"] = new List<string>
            {
                // Limited permissions for User role
                Orders_Create, Orders_ViewOwn,
                Shops_Add, Shops_View
            }
        };

        public static List<string> GetPermissionsForRole(string role)
        {
            return RolePermissions.ContainsKey(role) ? RolePermissions[role] : new List<string>();
        }
    }
}