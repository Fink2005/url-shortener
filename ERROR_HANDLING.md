# Error Handling Implementation

## Overview

Implemented comprehensive error handling across all services với proper HTTP status codes.

## Error Response Format

```json
{
  "statusCode": 400,
  "message": "Error message",
  "details": "ERROR_CODE",
  "errors": {
    "field": ["error message"]
  }
}
```

## HTTP Status Codes

### Success (2xx)

- **200 OK**: Request succeeded
- **201 Created**: Resource created successfully

### Client Errors (4xx)

- **400 Bad Request**: Validation errors, invalid input
- **401 Unauthorized**: Invalid credentials, missing/invalid token
- **403 Forbidden**: User doesn't have permission
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource already exists, duplicate

### Server Errors (5xx)

- **500 Internal Server Error**: Unexpected server errors
- **504 Gateway Timeout**: Request timeout

## Error Codes

### Auth Errors

- `INVALID_CREDENTIALS`: Wrong username/password
- `USER_ALREADY_EXISTS`: Duplicate username/email
- `USER_NOT_FOUND`: User doesn't exist
- `EMAIL_NOT_VERIFIED`: Email verification required
- `INVALID_TOKEN`: Invalid JWT or refresh token
- `TOKEN_EXPIRED`: Token has expired

### Validation Errors

- `VALIDATION_ERROR`: General validation failure
- `INVALID_EMAIL`: Email format invalid
- `WEAK_PASSWORD`: Password doesn't meet requirements

### Resource Errors

- `RESOURCE_NOT_FOUND`: Generic resource not found
- `URL_NOT_FOUND`: Short URL not found

### Business Logic Errors

- `DUPLICATE_RESOURCE`: Resource already exists
- `EMAIL_ALREADY_VERIFIED`: Email already verified

### Server Errors

- `INTERNAL_ERROR`: Unexpected error
- `DATABASE_ERROR`: Database operation failed
- `EXTERNAL_SERVICE_ERROR`: External API failed

## Implementation

### ApiGateway

1. **GlobalExceptionMiddleware**: Catches all unhandled exceptions
2. **ErrorHandlingExtensions**: Helper methods for consistent error handling
3. **Controller error handling**: Try-catch in controllers với proper status codes

### Services (AuthService, UserService, etc.)

1. **Consumers catch exceptions**: Prevent RabbitMQ message failures
2. **Throw specific exceptions**: UnauthorizedAccessException, ArgumentException, etc.
3. **Log errors**: Console.WriteLine với emoji indicators

## Testing Error Scenarios

### 401 Unauthorized

```bash
curl -X POST http://localhost:5050/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "wrong", "password": "wrong"}'
# Response: 401 {"message": "Invalid username or password"}
```

### 409 Conflict

```bash
curl -X POST http://localhost:5050/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username": "existing", "email": "existing@example.com", "password": "Pass123!"}'
# Response: 409 {"message": "Username or email already exists"}
```

### 400 Bad Request

```bash
curl -X POST http://localhost:5050/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username": "", "email": "invalid", "password": "123"}'
# Response: 400 {"message": "Validation failed: ..."}
```

## Benefits

1. **Consistent error format**: All services return same JSON structure
2. **Proper HTTP status codes**: Clients can handle errors correctly
3. **Detailed error messages**: Helpful for debugging
4. **Security**: Don't expose internal error details in production
5. **Logging**: All errors logged với context
