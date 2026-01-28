using System;
using System.Threading.Tasks;
using Daily_Journal_App.Models;

namespace Daily_Journal_App.Services;

public class AuthService
{
    private readonly UserService _userService;
    private bool _isAuthenticated = false;
    private User? _currentUser = null;
    private const int OtpLength = 4;

    public AuthService(UserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public bool IsAuthenticated => _isAuthenticated;
    public User? CurrentUser => _currentUser;

    public Task<bool> UserExistsAsync() => _userService.UserExistsAsync();

    public async Task<bool> RegisterUserAsync(string name, string otp)
    {
        if (string.IsNullOrWhiteSpace(name) || !IsOtpFormatValid(otp))
            return false;

        try
        {
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
        if (!IsOtpFormatValid(otp))
            return false;

        var isValid = await _userService.ValidateOTPAsync(otp);
        if (isValid)
        {
            _isAuthenticated = true;
            _currentUser = await _userService.GetUserAsync();
        }

        return isValid;
    }

    public void Logout()
    {
        _isAuthenticated = false;
        _currentUser = null;
    }

    public string GetUserName() => _currentUser?.Name ?? "User";

    public async Task<bool> UpdateOTPAsync(string newOtp)
    {
        if (!IsOtpFormatValid(newOtp))
            return false;

        try
        {
            var result = await _userService.UpdateOTPAsync(newOtp);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsOtpFormatValid(string? otp) =>
        !string.IsNullOrWhiteSpace(otp) && otp.Length == OtpLength && otp.All(char.IsDigit);
}
