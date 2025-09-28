# Debug Role Update Issue

## Issue Description
- API `PUT /api/v1/auth/customers/31/role` with `{"roleId":4}` returns 200 OK
- But database still shows role ID = 1 for user ID 31

## Debug Steps

### 1. Check Available Roles
**Request:**
```
GET /api/v1/auth/roles
Authorization: Bearer ADMIN_TOKEN
```

**Expected Response:**
```json
{
    "success": true,
    "message": "Roles retrieved successfully.",
    "data": [
        {"id": 1, "name": "Customer", "description": "Standard customer"},
        {"id": 2, "name": "Admin", "description": "Administrator"},
        {"id": 3, "name": "VIP", "description": "VIP customer"},
        {"id": 4, "name": "Premium", "description": "Premium customer"}
    ]
}
```

### 2. Check Current User Roles (Before Update)
**Request:**
```
GET /api/v1/auth/customers/31/roles  
Authorization: Bearer ADMIN_TOKEN
```

**Expected Response:**
```json
{
    "success": true,
    "message": "Customer roles retrieved successfully.",
    "data": {
        "userId": 31,
        "userEmail": "customer@example.com",
        "userName": "John Doe",
        "roleAssignments": [
            {
                "roleId": 1,
                "roleName": "Customer", 
                "isActive": true,
                "assignedAt": "2024-01-01T10:00:00Z"
            }
        ]
    }
}
```

### 3. Update Role
**Request:**
```
PUT /api/v1/auth/customers/31/role
Authorization: Bearer ADMIN_TOKEN
Content-Type: application/json

{"roleId": 4, "reason": "Debug test"}
```

### 4. Verify Update (After Update)
**Request:**
```
GET /api/v1/auth/customers/31/roles
Authorization: Bearer ADMIN_TOKEN  
```

**Expected Response:**
```json
{
    "success": true,
    "message": "Customer roles retrieved successfully.",
    "data": {
        "userId": 31,
        "userEmail": "customer@example.com", 
        "userName": "John Doe",
        "roleAssignments": [
            {
                "roleId": 4,
                "roleName": "Premium",
                "isActive": true,
                "assignedAt": "2024-01-01T11:00:00Z"  
            },
            {
                "roleId": 1,
                "roleName": "Customer",
                "isActive": false,
                "assignedAt": "2024-01-01T10:00:00Z"
            }
        ]
    }
}
```

## Potential Issues to Check

1. **Role ID 4 doesn't exist** - Check GET /roles response
2. **Database transaction failed** - Check logs for SaveChanges errors  
3. **User ID 31 doesn't exist** - Check if user exists
4. **Exception caught silently** - Updated RbacService now logs all errors
5. **Database connection issue** - Check connection string
6. **Authorization issue** - Verify admin token is valid

## Enhanced Logging

The RbacService.UpdateUserRoleAsync now includes comprehensive logging:
- Role existence validation  
- User existence validation
- Current roles deactivation
- New role assignment
- SaveChanges result
- Exception details

Check application logs for detailed information about the role update process.