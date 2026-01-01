# CAST Execution UI - Login Implementation Summary

## Overview
A complete login system with Basic Authentication has been added to the CAST Execution UI application. The login process can be controlled via the `enable_login` boolean parameter in `appsettings.json`.

## Changes Made

### 1. **New Service: AuthenticationService**
- **File**: `Services/AuthenticationService.cs`
- **Purpose**: Handles all authentication logic including:
  - Loading user credentials from `Properties/user.properties`
  - Validating credentials against stored values
  - Managing authentication tokens in session
  - Clearing authentication data when users logout

**Key Methods**:
- `ValidateCredentials(username, password)` - Validates user credentials
- `ValidateBasicAuth(request)` - Validates Basic Authentication headers
- `SetAuthenticationToken(token)` - Stores auth token in session
- `IsAuthenticated()` - Checks if user has active session
- `ClearAuthentication()` - Clears all auth data and session

### 2. **Configuration Update**
- **File**: `appsettings.json`
- **Change**: Added `"enable_login": true` to AppSettings section
- **Purpose**: Toggle login feature on/off without code changes

### 3. **Credentials File**
- **File**: `Properties/user.properties`
- **Format**: `username=password` (one per line)
- **Sample Credentials**:
  - `admin=admin@123`
  - `user=user@123`

### 4. **Program Configuration**
- **File**: `Program.cs`
- **Changes**:
  - Added `AddHttpContextAccessor()` for HTTP context access
  - Added session support with `AddSession()`
  - Registered `AuthenticationService` as scoped dependency
  - Added `UseSession()` middleware

### 5. **Pages/cast.cshtml.cs Updates**
- Added `AuthenticationService` dependency injection
- Added authentication properties: `IsLoginRequired`, `IsAuthenticated`, `AuthenticationError`
- Updated `OnGet()` method to check authentication before displaying content
- Added `OnPostLogin()` handler for login form submission
- Added `OnPostLogout()` handler for logout functionality

### 6. **Pages/cast_base.cshtml.cs Updates**
- Same changes as cast.cshtml.cs
- Authentication check in `OnGet()`
- Login/logout handlers

### 7. **UI Updates - cast.cshtml**
- Added conditional login page display when `IsLoginRequired && !IsAuthenticated`
- Login form with username and password fields
- Error message display for invalid credentials
- Logout button in header when login is enabled
- Styled login modal with gradient background

### 8. **UI Updates - cast_base.cshtml**
- Added CSS styles for login interface
- Added conditional login page display
- Login form with username and password fields
- Logout button in navigation
- Responsive design matching the application theme

### 9. **Cache Clearing - JavaScript**
- **File**: `wwwroot/js/cache-clear.js`
- **Purpose**: Clears all browser cache when pages are closed
- **Clearing Targets**:
  - LocalStorage
  - SessionStorage
  - Service Workers
  - IndexedDB
  - Browser cache headers

**Events Monitored**:
- `beforeunload` - Before page unloads
- `unload` - Page unloading
- `pagehide` - Page hidden (tab closed)
- `visibilitychange` - Tab switching
- `pageshow` - Page shown (back/forward navigation)

**Included in**: Both `cast.cshtml` and `cast_base.cshtml`

## Feature Details

### Login Flow
1. User accesses `/cast` or `/cast_base`
2. If `enable_login` is `true` and user is not authenticated:
   - Login page is displayed
   - User enters credentials
3. Credentials are validated against `user.properties`
4. On successful login:
   - Authentication token is stored in session
   - User is redirected to main page
5. User can logout using the logout button
6. On logout or page close:
   - All cached credentials are cleared
   - Session is cleared

### Security Features
- Session-based authentication (30-minute timeout)
- Credentials stored in properties file (not in database)
- All cache cleared on logout/page close
- LocalStorage and SessionStorage cleared
- Service workers and IndexedDB cleaned

## Configuration Options

### Enable/Disable Login
In `appsettings.json`:
```json
"AppSettings": {
  "enable_login": true  // Set to false to disable login
}
```

### Add New Credentials
Edit `Properties/user.properties`:
```
username1=password1
username2=password2
```

## Testing

### Default Test Credentials
- **Username**: `admin` **Password**: `admin@123`
- **Username**: `user` **Password**: `user@123`

### Test Scenarios
1. **Enable Login**: Set `enable_login: true` in appsettings.json
2. **Access Pages**: Navigate to `/cast` or `/cast_base`
3. **Invalid Credentials**: Enter wrong username/password - should show error
4. **Valid Login**: Use credentials above - should redirect to main page
5. **Logout**: Click logout button - should clear session and return to login
6. **Cache Clearing**: Close browser/tab - cache should be cleared automatically

## Files Modified/Created

### Created:
- `Services/AuthenticationService.cs`
- `Properties/user.properties`
- `wwwroot/js/cache-clear.js`

### Modified:
- `Program.cs`
- `Pages/cast.cshtml.cs`
- `Pages/cast_base.cshtml.cs`
- `Pages/cast.cshtml`
- `Pages/cast_base.cshtml`
- `appsettings.json`

## Notes
- Both `cast` and `cast_base` pages use the same authentication system
- Login state is shared across both pages via session
- Cache clearing happens automatically without user intervention
- The implementation is non-invasive and can be disabled via configuration
