namespace Backend.Application.Abstractions;

/// <summary>Password hashing. Real implementation arrives with Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}
