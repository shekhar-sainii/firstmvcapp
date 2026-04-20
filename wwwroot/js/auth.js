// JWT Token Management Service
const AuthService = {
    // Store token in localStorage
    setToken: function(token) {
        if (!token || typeof token !== 'string') {
            console.warn('Invalid token provided - token must be a string', token);
            return false;
        }
        
        try {
            // Simple check: JWT should have 3 parts separated by dots
            if (!token.includes('.')) {
                console.warn('Invalid JWT format - missing dots', token);
                return false;
            }
            
            localStorage.setItem('jwtToken', token);
            console.log('Token stored successfully in localStorage');
            return true;
        } catch (error) {
            console.error('Error storing token:', error);
            return false;
        }
    },

    // Get token from localStorage
    getToken: function() {
        const token = localStorage.getItem('jwtToken');
        console.log('Retrieved token:', token ? 'Token exists' : 'No token');
        return token;
    },

    // Store user info in localStorage
    setUser: function(user) {
        if (!user) {
            console.warn('Invalid user data');
            return false;
        }
        try {
            localStorage.setItem('user', JSON.stringify(user));
            console.log('User stored successfully:', user.email);
            return true;
        } catch (error) {
            console.error('Error storing user:', error);
            return false;
        }
    },

    // Get user info from localStorage
    getUser: function() {
        const user = localStorage.getItem('user');
        return user ? JSON.parse(user) : null;
    },

    // Check if user is authenticated
    isAuthenticated: function() {
        const token = this.getToken();
        if (!token) return false;
        
        // Additional validation
        if (isTokenExpired(token)) {
            this.logout();
            return false;
        }
        
        return true;
    },

    // Validate token format (JWT should have 3 parts separated by dots)
    isValidTokenFormat: function(token) {
        if (!token || typeof token !== 'string') return false;
        const parts = token.split('.');
        return parts.length === 3 && parts.every(part => part.length > 0);
    },

    // Clear all auth data
    logout: function() {
        try {
            localStorage.removeItem('jwtToken');
            localStorage.removeItem('user');
            sessionStorage.clear();
        } catch (error) {
            console.error('Error during logout:', error);
        }
    },

    // Force logout and redirect to login page
    forceLogout: function(reason = 'Session expired') {
        console.warn('Force logout:', reason);
        this.logout();
        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
    },

    // Get authorization header for API calls
    getAuthHeader: function() {
        const token = this.getToken();
        return token ? `Bearer ${token}` : null;
    }
};

// Helper function to validate token structure
function isValidTokenStructure(token) {
    try {
        if (!token || typeof token !== 'string') return false;
        const parts = token.split('.');
        if (parts.length !== 3) return false;
        
        // Try to decode and parse payload
        const base64Url = parts[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        const payload = JSON.parse(jsonPayload);
        return payload && payload.exp;
    } catch (error) {
        return false;
    }
}

// Helper function to check token expiration
function isTokenExpired(token) {
    try {
        if (!isValidTokenStructure(token)) {
            console.warn('Invalid token structure detected');
            return true;
        }
        
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        const payload = JSON.parse(jsonPayload);
        const exp = payload.exp * 1000; // Convert to milliseconds
        
        if (Date.now() >= exp) {
            console.warn('Token has expired');
            return true;
        }
        return false;
    } catch (error) {
        console.warn('Error checking token expiration:', error);
        return true;
    }
}

// Auto-logout if token is expired or invalid
function checkTokenExpiration() {
    const token = AuthService.getToken();
    
    if (!token) {
        // No token, user should not be authenticated
        return;
    }
    
    if (!isValidTokenStructure(token)) {
        console.warn('Invalid token detected - forcing logout');
        AuthService.forceLogout('Invalid token detected. Please login again.');
        return;
    }
    
    if (isTokenExpired(token)) {
        AuthService.forceLogout('Your session has expired. Please login again.');
        return;
    }
}

// Monitor localStorage changes for token removal/modification
window.addEventListener('storage', function(event) {
    if (event.key === 'jwtToken') {
        if (event.newValue === null) {
            // Token was removed
            console.log('Token removed from another tab/window');
            AuthService.logout();
        } else if (event.newValue !== event.oldValue) {
            // Token was modified
            console.warn('Token modified detected');
            AuthService.forceLogout('Your session was modified. Please login again.');
        }
    }
});

// Check token expiration on page load
document.addEventListener('DOMContentLoaded', function() {
    checkTokenExpiration();
});

// Periodically check token expiration (every 30 seconds)
setInterval(checkTokenExpiration, 30000);
