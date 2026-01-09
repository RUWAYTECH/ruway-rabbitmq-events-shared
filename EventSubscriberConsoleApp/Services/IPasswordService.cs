using System;

namespace EventSubscriberConsoleApp.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateRandomToken();
}
