using SQLite;
using JournalApp.Models;

namespace JournalApp.Services;

public class UserService
{
    private readonly SQLiteAsyncConnection _database;

    public UserService(string dbPath)
    {
        try
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<User>().Wait();
            Console.WriteLine("User table initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService initialization error: {ex.Message}");
            throw;
        }
    }

    public async Task<User?> GetUserAsync()
    {
        try
        {
            return await _database.Table<User>().FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetUserAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UserExistsAsync()
    {
        try
        {
            var user = await GetUserAsync();
            return user != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserExistsAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<int> CreateUserAsync(string name, string otp)
    {
        try
        {
            // Check if user already exists
            if (await UserExistsAsync())
            {
                throw new InvalidOperationException("User already exists. Only one user is allowed.");
            }

            var user = new User
            {
                Name = name.Trim(),
                OTP = otp.Trim()
            };

            var result = await _database.InsertAsync(user);
            Console.WriteLine($"User created. ID: {user.Id}, Result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateUserAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> ValidateOTPAsync(string otp)
    {
        try
        {
            var user = await GetUserAsync();
            if (user == null)
            {
                return false;
            }

            return user.OTP == otp.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ValidateOTPAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<int> UpdateOTPAsync(string newOtp)
    {
        try
        {
            var user = await GetUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.OTP = newOtp.Trim();
            var result = await _database.UpdateAsync(user);
            Console.WriteLine($"User OTP updated. ID: {user.Id}, Result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateOTPAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<int> DeleteUserAsync()
    {
        try
        {
            var user = await GetUserAsync();
            if (user == null)
            {
                return 0;
            }

            var result = await _database.DeleteAsync(user);
            Console.WriteLine($"User deleted. ID: {user.Id}, Result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteUserAsync error: {ex.Message}");
            throw;
        }
    }
}