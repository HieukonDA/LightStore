# Customer Management API - Roles Enhancement

## Updated Features

### GetCustomerById API
**Endpoint:** `GET /api/v1/auth/customers/{customerId}`
**Authorization:** Admin role required

**Enhanced Response (now includes roles):**
```json
{
    "success": true,
    "message": "Customer retrieved successfully.",
    "data": {
        "id": 1,
        "email": "customer@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "phone": "0123456789",
        "userType": "Customer",
        "createdAt": "2024-01-01T00:00:00Z",
        "roles": ["Customer", "VIP"]  // ← NEW: Danh sách roles hiện tại của user
    }
}
```

### GetCustomers API (List)
**Endpoint:** `GET /api/v1/auth/customers?page=1&size=10&search=john&sort=createdat_desc`
**Authorization:** Admin role required

**Enhanced Response (now includes roles for each customer):**
```json
{
    "success": true,
    "message": "Customers retrieved successfully.",
    "data": {
        "items": [
            {
                "id": 1,
                "email": "customer1@example.com", 
                "firstName": "John",
                "lastName": "Doe",
                "phone": "0123456789",
                "userType": "Customer",
                "createdAt": "2024-01-01T00:00:00Z",
                "roles": ["Customer", "VIP"]  // ← NEW: Roles cho từng customer
            },
            {
                "id": 2,
                "email": "customer2@example.com",
                "firstName": "Jane", 
                "lastName": "Smith",
                "phone": "0987654321",
                "userType": "Customer",
                "createdAt": "2024-01-02T00:00:00Z",
                "roles": ["Customer"]  // ← NEW: Roles cho từng customer
            }
        ],
        "totalCount": 25,
        "page": 1,
        "pageSize": 10
    }
}
```

## Technical Changes Made

### 1. GetCustomerByIdAsync Method
- **Before:** Tried to access `user.UserRoles` directly (would cause null reference)
- **After:** Uses `_rbacService.GetUserRolesAsync()` for accurate role retrieval
- **Benefit:** Ensures roles are properly loaded and includes cache benefits

### 2. GetCustomersAsync Method  
- **Before:** Attempted direct access to `user.UserRoles` without Include
- **After:** Uses `.Include()` with proper filtering for active roles only
- **Performance:** Single database query with eager loading instead of N+1 queries
- **Filter:** Only includes active roles (`ur.IsActive == true`)

### 3. Response Structure
- **Maintained:** All existing fields (`userType`, `email`, etc.) remain unchanged
- **Enhanced:** Added `roles` array containing current active role names
- **Backward Compatible:** Existing clients won't break, new clients get enhanced data

## Usage Examples

### Get Single Customer with Roles
```bash
curl -X GET "https://api.lightstore.com/api/v1/auth/customers/123" \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN"
```

### Get Customers List with Search and Roles
```bash
curl -X GET "https://api.lightstore.com/api/v1/auth/customers?page=1&size=5&search=john" \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN"  
```

## Benefits

1. **Complete User Information:** Admins now see both `userType` and active `roles`
2. **Role Management Context:** When updating roles, admins can see current state
3. **Audit Capability:** Clear visibility of user permissions and roles
4. **Performance Optimized:** Efficient database queries with proper eager loading
5. **Cache Integrated:** GetCustomerById leverages RBAC service caching

## Role Management Workflow

1. **View Customer:** `GET /customers/{id}` - See current roles
2. **Update Role:** `PUT /customers/{id}/role` - Change customer role  
3. **Verify Change:** `GET /customers/{id}` - Confirm new role applied

The roles array will automatically reflect changes made through the role update API, providing real-time feedback for admin operations.