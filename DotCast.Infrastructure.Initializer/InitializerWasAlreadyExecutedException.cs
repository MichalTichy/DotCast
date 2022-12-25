namespace DotCast.Infrastructure.Initializer
{
    public class InitializerWasAlreadyExecutedException : InitializerException
    {
        public InitializerWasAlreadyExecutedException() : base("Initializer already executed.")
        {
        }
    }
}