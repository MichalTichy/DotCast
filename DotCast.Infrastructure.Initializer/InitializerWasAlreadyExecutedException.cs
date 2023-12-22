namespace DotCast.Infrastructure.Initializer
{
    public class InitializerWasAlreadyExecutedException() : InitializerException("Initializer already executed.");
}
