namespace DotCast.Library.Mcp.Tests;
public sealed class UnknownLengthContent(byte[] bytes) : ByteArrayContent(bytes)
{
    protected override bool TryComputeLength(out long length) { length = 0; return false; }
}
