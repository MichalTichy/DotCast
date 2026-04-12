using System.ComponentModel;

namespace DotCast.Infrastructure.CurrentUserProvider;

public class NoLoggedUserFoundException : Exception
{
    public NoLoggedUserFoundException([Localizable(true)] string message) : base(message)
    {
    }

    public NoLoggedUserFoundException() : this("No user logged in!")
    {
    }
}
