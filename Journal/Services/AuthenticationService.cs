using JournalApp.Data;
using JournalApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace JournalApp.Services
{
    public interface IAuthenticationService
    {
        Task<bool> RegisterAsync(string username, string password, string? pin = null);
        Task<User?> LoginAsync(string username, string password);
        Task<bool> LoginWithPinAsync(string pin);
        Task<bool> SetPinAsync(string pin);
        Task<bool> RemovePinAsync();
        Task<bool> ValidatePinAsync(string pin);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<bool> IsAuthenticatedAsync();
        Task<bool> HasPinAsync();
        Task LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        Task LockAppAsync();
        Task<bool> IsAppLockedAsync();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly JournalDbContext _context;

        private const string PASSWORD_HASH_KEY = "user_password_hash";
        private const string PIN_HASH_KEY = "user_pin_hash";
        private const string USERNAME_KEY = "current_username";
        private const string USER_ID_KEY = "current_user_id";
        private const string IS_AUTHENTICATED_KEY = "is_authenticated";
        private const string HAS_PIN_KEY = "has_pin";
        private const string IS_LOCKED_KEY = "is_app_locked";

        public AuthenticationService(JournalDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterAsync(string username, string password, string? pin = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return false;

                if (password.Length < 6)
                    return false;

                var existingUser = await _context.GetUserByUsernameAsync(username);
                if (existingUser != null)
                    return false;

                var user = new User
                {
                    Username = username,
                    PasswordHash = "stored_in_secure_storage",
                    HasPin = !string.IsNullOrEmpty(pin),
                    Theme = "Light",
                    CreatedAt = DateTime.Now
                };

                await _context.SaveUserAsync(user);

                var savedUser = await _context.GetUserByUsernameAsync(username);
                if (savedUser == null)
                    return false;

                var passwordHash = HashPassword(password);
                await SecureStorage.SetAsync($"{PASSWORD_HASH_KEY}_{savedUser.Id}", passwordHash);

                if (!string.IsNullOrEmpty(pin) && pin.Length == 4)
                {
                    var pinHash = HashPassword(pin);
                    await SecureStorage.SetAsync($"{PIN_HASH_KEY}_{savedUser.Id}", pinHash);
                    savedUser.HasPin = true;
                    await _context.SaveUserAsync(savedUser);
                }

                await SetAuthenticationState(savedUser);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _context.GetUserByUsernameAsync(username);
                if (user == null)
                    return null;

                var storedHash = await SecureStorage.GetAsync($"{PASSWORD_HASH_KEY}_{user.Id}");
                if (string.IsNullOrEmpty(storedHash))
                    return null;

                var passwordHash = HashPassword(password);
                if (passwordHash != storedHash)
                    return null;

                user.LastLoginAt = DateTime.Now;
                await _context.SaveUserAsync(user);

                await SetAuthenticationState(user);

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> LoginWithPinAsync(string pin)
        {
            try
            {
                var userId = Preferences.Get(USER_ID_KEY, 0);
                if (userId == 0)
                    return false;

                var user = await _context.GetUserByIdAsync(userId);
                if (user == null || !user.HasPin)
                    return false;

                var isValid = await ValidatePinAsync(pin);
                if (!isValid)
                    return false;

                Preferences.Set(IS_AUTHENTICATED_KEY, true);
                Preferences.Set(IS_LOCKED_KEY, false);

                user.LastLoginAt = DateTime.Now;
                await _context.SaveUserAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PIN login error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidatePinAsync(string pin)
        {
            try
            {
                var userId = Preferences.Get(USER_ID_KEY, 0);
                if (userId == 0)
                    return false;

                var storedPinHash = await SecureStorage.GetAsync($"{PIN_HASH_KEY}_{userId}");
                if (string.IsNullOrEmpty(storedPinHash))
                    return false;

                var pinHash = HashPassword(pin);
                return pinHash == storedPinHash;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PIN validation error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetPinAsync(string pin)
        {
            try
            {
                if (string.IsNullOrEmpty(pin) || pin.Length != 4)
                    return false;

                if (!int.TryParse(pin, out _))
                    return false;

                var userId = Preferences.Get(USER_ID_KEY, 0);
                if (userId == 0)
                    return false;

                var user = await _context.GetUserByIdAsync(userId);
                if (user == null)
                    return false;

                var pinHash = HashPassword(pin);
                await SecureStorage.SetAsync($"{PIN_HASH_KEY}_{userId}", pinHash);

                user.HasPin = true;
                await _context.SaveUserAsync(user);

                Preferences.Set(HAS_PIN_KEY, true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Set PIN error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemovePinAsync()
        {
            try
            {
                var userId = Preferences.Get(USER_ID_KEY, 0);
                if (userId == 0)
                    return false;

                var user = await _context.GetUserByIdAsync(userId);
                if (user == null)
                    return false;

                SecureStorage.Remove($"{PIN_HASH_KEY}_{userId}");

                user.HasPin = false;
                await _context.SaveUserAsync(user);

                Preferences.Set(HAS_PIN_KEY, false);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Remove PIN error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasPinAsync()
        {
            var userId = Preferences.Get(USER_ID_KEY, 0);
            if (userId == 0)
                return false;

            var user = await _context.GetUserByIdAsync(userId);
            return user?.HasPin ?? false;
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                    return false;

                if (newPassword.Length < 6)
                    return false;

                var userId = Preferences.Get(USER_ID_KEY, 0);
                if (userId == 0)
                    return false;

                var user = await _context.GetUserByIdAsync(userId);
                if (user == null)
                    return false;

                var storedHash = await SecureStorage.GetAsync($"{PASSWORD_HASH_KEY}_{userId}");
                var currentHash = HashPassword(currentPassword);

                if (currentHash != storedHash)
                    return false;

                var newHash = HashPassword(newPassword);
                await SecureStorage.SetAsync($"{PASSWORD_HASH_KEY}_{userId}", newHash);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Change password error: {ex.Message}");
                return false;
            }
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            return Task.FromResult(Preferences.Get(IS_AUTHENTICATED_KEY, false));
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var userId = Preferences.Get(USER_ID_KEY, 0);
            if (userId == 0)
                return null;

            return await _context.GetUserByIdAsync(userId);
        }

        public async Task LogoutAsync()
        {
            try
            {
                Preferences.Remove(USER_ID_KEY);
                Preferences.Remove(USERNAME_KEY);
                Preferences.Set(IS_AUTHENTICATED_KEY, false);
                Preferences.Set(IS_LOCKED_KEY, false);
                Preferences.Set(HAS_PIN_KEY, false);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
            }
        }

        public async Task LockAppAsync()
        {
            Preferences.Set(IS_LOCKED_KEY, true);
            await Task.CompletedTask;
        }

        public async Task<bool> IsAppLockedAsync()
        {
            var isLocked = Preferences.Get(IS_LOCKED_KEY, false);
            var isAuthenticated = Preferences.Get(IS_AUTHENTICATED_KEY, false);
            return await Task.FromResult(isLocked && isAuthenticated);
        }

        private async Task SetAuthenticationState(User user)
        {
            Preferences.Set(USER_ID_KEY, user.Id);
            Preferences.Set(USERNAME_KEY, user.Username);
            Preferences.Set(IS_AUTHENTICATED_KEY, true);
            Preferences.Set(HAS_PIN_KEY, user.HasPin);
            Preferences.Set(IS_LOCKED_KEY, false);
            await Task.CompletedTask;
        }

        private string HashPassword(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}