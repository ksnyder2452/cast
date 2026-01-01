using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Execution_UI.Services
{
    public class AuthenticationService
    {
        private readonly string _userPropertiesPath;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Dictionary<string, string> _credentials = new();

        public AuthenticationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userPropertiesPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Properties",
                "user.properties"
            );
            LoadCredentials();
        }

        /// <summary>
        /// Loads user credentials from user.properties file
        /// </summary>
        private void LoadCredentials()
        {
            try
            {
                if (File.Exists(_userPropertiesPath))
                {
                    var lines = File.ReadAllLines(_userPropertiesPath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            continue;

                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            var username = parts[0].Trim();
                            var password = parts[1].Trim();
                            _credentials[username] = password;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading credentials: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates user credentials
        /// </summary>
        public bool ValidateCredentials(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            if (_credentials.TryGetValue(username, out var storedPassword))
            {
                return storedPassword == password;
            }

            return false;
        }

        /// <summary>
        /// Validates Basic Authentication header
        /// </summary>
        public bool ValidateBasicAuth(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
                return false;

            try
            {
                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                var credentials = decodedString.Split(':');

                if (credentials.Length == 2)
                {
                    return ValidateCredentials(credentials[0], credentials[1]);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Stores authentication token in session
        /// </summary>
        public void SetAuthenticationToken(string token)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.SetString("AuthToken", token);
            }
        }

        /// <summary>
        /// Retrieves authentication token from session
        /// </summary>
        public string GetAuthenticationToken()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Session.GetString("AuthToken") ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var token = _httpContextAccessor.HttpContext.Session.GetString("AuthToken");
                return !string.IsNullOrEmpty(token);
            }
            return false;
        }

        /// <summary>
        /// Clears authentication token and session data
        /// </summary>
        public void ClearAuthentication()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.Clear();
                _httpContextAccessor.HttpContext.Session.Remove("AuthToken");
                _httpContextAccessor.HttpContext.Session.Remove("Username");
            }
        }

        /// <summary>
        /// Sets username in session
        /// </summary>
        public void SetUsername(string username)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.SetString("Username", username);
            }
        }

        /// <summary>
        /// Gets username from session
        /// </summary>
        public string GetUsername()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Session.GetString("Username") ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
