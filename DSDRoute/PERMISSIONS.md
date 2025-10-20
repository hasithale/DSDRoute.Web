# User Role Permissions System

## Overview

This ASP.NET Core application implements a role-based permission system that provides fine-grained control over user actions. The system replaces simple role-based authorization with a more flexible permission-based approach.

## User Role Permissions

The **User** role has been configured with the following permissions based on your requirements:

### Orders
- ✅ **Create**: Can create new orders
- ✅ **View Own**: Can view only their own orders (not other users' orders)
- ❌ **View All**: Cannot view all orders in the system
- ❌ **Edit**: Cannot edit orders after creation
- ❌ **Delete**: Cannot delete orders

### Shops
- ✅ **Add**: Can add new shops to the system
- ✅ **View**: Can view shop details and listings
- ❌ **Edit**: Cannot edit existing shop information
- ❌ **Delete**: Cannot delete or deactivate shops

## Implementation Details

### 1. Permission Constants
Located in `Models/Permissions.cs`, this defines all available permissions and maps them to roles:

```csharp
public static class Permissions
{
    // Order permissions
    public const string Orders_Create = "orders.create";
    public const string Orders_ViewOwn = "orders.view_own";
    public const string Orders_ViewAll = "orders.view_all";
    // ... more permissions

    public static Dictionary<string, List<string>> RolePermissions = new Dictionary<string, List<string>>
    {
        ["User"] = new List<string>
        {
            Orders_Create, Orders_ViewOwn,
            Shops_Add, Shops_View
        }
        // ... other roles
    };
}
```

### 2. Authorization Infrastructure

#### Custom Attribute
`RequirePermissionAttribute` provides a clean way to protect controller actions:

```csharp
[RequirePermission(Permissions.Orders_Create)]
public async Task<IActionResult> Create()
```

#### Authorization Handler
`PermissionAuthorizationHandler` evaluates whether the current user has the required permission based on their roles.

#### Extension Methods
`PermissionExtensions` provides helper methods for checking permissions:

```csharp
var canCreate = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_Create);
```

### 3. Controller Updates

Controllers have been updated to use permissions instead of roles:

**Before:**
```csharp
[Authorize(Roles = "Admin,SalesRep")]
public async Task<IActionResult> Create()
```

**After:**
```csharp
[RequirePermission(Permissions.Orders_Create)]
public async Task<IActionResult> Create()
```

### 4. View Updates

Views now use permission flags to conditionally show/hide UI elements:

```html
@if (canCreate)
{
    <a class="modern-btn modern-btn-success" asp-action="Create">
        <i class="fas fa-plus me-2"></i>Create New Order
    </a>
}
```

## Testing User Role Behavior

### What Users CAN do:
1. **Login** to the system
2. **Create new orders** - Full access to order creation form
3. **View their own orders** - See orders they created
4. **View order details** - For their own orders only
5. **Add new shops** - Full access to shop creation
6. **View shop listings** - See all shops in the system
7. **View shop details** - Access to shop detail pages

### What Users CANNOT do:
1. **View other users' orders** - Will only see their own orders in lists
2. **Edit orders** - No edit buttons/access for any orders
3. **Delete orders** - No delete functionality available
4. **Approve/reject orders** - Admin-only functions
5. **Edit shops** - No edit buttons for shops
6. **Delete/deactivate shops** - No delete functionality
7. **Access admin dashboard** - Completely restricted
8. **Manage users** - Admin-only functionality
9. **View system-wide analytics** - Limited to their own data

## User Experience

When a User logs in:

1. **Order Page**: Shows only their orders with "Create New Order" button visible
2. **Shop Page**: Shows all shops with "Add New Shop" button, but no edit/delete buttons
3. **Navigation**: Admin-specific menu items are hidden
4. **Forbidden Actions**: Attempting restricted actions results in proper 403 Forbidden responses

## Security Features

- **Authentication Required**: All features require login
- **Permission Validation**: Every action checks specific permissions
- **Data Isolation**: Users automatically see only their own data where appropriate  
- **UI Consistency**: Buttons and links only appear when user has permission
- **Graceful Degradation**: Missing permissions result in proper HTTP status codes

## Admin vs User Comparison

| Feature | Admin | User |
|---------|-------|------|
| Create Orders | ✅ | ✅ |
| View All Orders | ✅ | ❌ |
| Edit Orders | ✅ | ❌ |
| Delete Orders | ✅ | ❌ |
| Add Shops | ✅ | ✅ |
| Edit Shops | ✅ | ❌ |
| Delete Shops | ✅ | ❌ |
| User Management | ✅ | ❌ |
| System Dashboard | ✅ | ❌ |

This permission system provides a secure, scalable foundation for role-based access control while maintaining a clean user experience.