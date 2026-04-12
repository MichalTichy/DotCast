using DotCast.Infrastructure.Initializer.Texts;

namespace DotCast.Infrastructure.Initializer;

public class InitializerWasAlreadyExecutedException : InitializerException
{
    public InitializerWasAlreadyExecutedException() : base(ExceptionTexts.InitializerAlreadyExecuted)
    {
    }
}
