# User Profile Management & Admin Role Management Guide

## Overview
Đã triển khai thành công hệ thống quản lý hồ sơ người dùng và quản lý vai trò admin, cho phép:
- Người dùng cập nhật thông tin cá nhân
- Admin quản lý vai trò của khách hàng

## Features Implemented

### 1. User Profile Update
**Endpoint:** `PUT /api/auth/profile`
**Authorization:** Required (Authenticated users)

**Request Body:**
```json
{
    "firstName": "string (optional, 1-100 chars)",
    "lastName": "string (optional, 1-100 chars)", 
    "phone": "string (optional, valid phone format, max 20 chars)",
    "email": "string (optional, valid email format, max 200 chars)"
}
```

**Response Success:**
```json
{
    "message": "Profile updated successfully",
    "data": {
        "id": 1,
        "email": "user@example.com",
        "firstName": "John",
        "lastName": "Doe", 
        "phone": "0123456789",
        "userType": "Customer",
        "createdAt": "2024-01-01T00:00:00Z",
        "roles": ["Customer"]
    },
    "success": true
}
```

**Features:**
- Email uniqueness validation
- Email format validation
- Automatic email verification reset when email changed
- Phone number format validation
- Field length validation
- JWT token-based user identification

### 2. Admin Role Management
**Endpoint:** `PUT /api/auth/customers/{customerId}/role`
**Authorization:** Required (Admin role only)

**Request Body:**
```json
{
    "roleId": 1,
    "reason": "string (optional, max 500 chars)"
}
```

**Response Success:**
```json
{
    "message": "Customer role updated successfully",
    "success": true
}
```

**Features:**
- Admin-only access control
- Role ID validation
- Automatic deactivation of current roles
- Comprehensive audit logging with reason
- Cache invalidation for immediate effect

## Technical Implementation

### New DTOs Created
1. **UpdateProfileDto** - For user profile updates
   - Validation attributes for all fields
   - Optional fields for partial updates
   
2. **UpdateCustomerRoleDto** - For admin role management
   - Required role ID validation
   - Optional reason field for audit trail

### Services Updated

#### AuthService.cs
- **UpdateProfileAsync()**: Handles user profile updates with comprehensive validation
- **UpdateCustomerRoleAsync()**: Handles admin role changes with proper authorization

#### RbacService.cs  
- **UpdateUserRoleAsync()**: Core role update logic with cache management
- Deactivates old roles and assigns new role
- Proper transaction handling

### Controllers Updated

#### AuthController.cs
- Added profile update endpoint with user authentication
- Added customer role management endpoint with admin authorization
- Proper error handling and response formatting

### Authorization & Security
- **Profile Update**: Requires authentication (`[Authorize]`)
- **Role Management**: Requires admin role (`[Authorize(Roles = "Admin")]`)
- JWT token parsing for user identification
- Email uniqueness validation
- Input validation with data annotations

### Validation Features
- **Profile Update Validation:**
  - Email format and uniqueness checking
  - Phone number format validation
  - Field length constraints
  - Optional field handling

- **Role Update Validation:**
  - User existence verification
  - Role ID validation
  - Admin permission verification
  - Audit reason tracking

### Logging & Monitoring
- Comprehensive logging for both profile and role updates
- Security logging for admin actions
- Error logging with detailed context
- Audit trail with reason tracking for role changes

## API Usage Examples

### Update User Profile
```bash
curl -X PUT "https://api.lightstore.com/api/auth/profile" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Updated", 
    "phone": "0987654321",
    "email": "newemail@example.com"
  }'
```

### Admin Update Customer Role
```bash
curl -X PUT "https://api.lightstore.com/api/auth/customers/123/role" \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleId": 2,
    "reason": "Promote to VIP customer due to high purchase volume"
  }'
```

## Database Impact
- Updates existing user records via UserRepo.UpdateUserAsync()
- RBAC role management through UserRoles table
- Maintains audit trail and timestamps
- Cache invalidation ensures immediate role changes

## Error Handling
- Comprehensive error responses with specific messages
- Validation error details in response
- Proper HTTP status codes
- Exception logging for debugging

## Security Considerations
- JWT token validation for all operations
- Role-based access control for admin functions
- Email verification reset on email changes
- Input sanitization and validation
- Audit logging for admin actions

## Dependencies
- Entity Framework Core for data operations
- JWT authentication for user identification
- RBAC service for role management
- Memory caching for performance
- Data annotations for validation

This implementation provides a complete user profile management system with proper security, validation, and admin controls while maintaining audit trails and performance through caching.