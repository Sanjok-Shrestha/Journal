using JournalApp.Models;

namespace JournalApp.Services;

public class AuthService
{
    public readonly UserService _userService;
    public bool isAuthenticated = false;
    public User? currentUser = null;

    public AuthService(UserService userService)
    {
        _userService = userService;
    }

    public bool IsAuthenticated => isAuthenticated;
    public User? CurrentUser => currentUser;

    public async Task<bool> UserExistsAsync()
    {
        return await _userService.UserExistsAsync();
    }

    public async Task<bool> RegisterUserAsync(string name, string otp)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(otp))
                return false;

            if (otp.Length != 4 || !otp.All(char.IsDigit))
                return false;

            var result = await _userService.CreateUserAsync(name, otp);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateOTPAsync(string otp)
    {
        if (string.IsNullOrWhiteSpace(otp))
            return false;

        var isValid = await _userService.ValidateOTPAsync(otp);
        if (isValid)
        {
            isAuthenticated = true;
            currentUser = await _userService.GetUserAsync();
        }
        return isValid;
    }

    public void Logout()
    {
        isAuthenticated = false;
        currentUser = null;
    }

    public string GetUserName()
    {
        return currentUser?.Name ?? "User";
    }

    public async Task<bool> UpdateOTPAsync(string newOtp)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newOtp) || newOtp.Length != 4 || !newOtp.All(char.IsDigit))
                return false;

            var result = await _userService.UpdateOTPAsync(newOtp);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    // New helpers used by Razor pages
    public Task<User?> GetCurrentUserAsync()
    {
        // If currentUser already loaded, return it; otherwise fetch from UserService
        return Task.FromResult(currentUser ?? _userService.GetUserAsync().Result);
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(isAuthenticated);
    }
}