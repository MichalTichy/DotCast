namespace DotCast.Storage.Processing.Abstractions
{
    [Flags]
    public enum ModificationType
    {
        Empty = 0, // 0000
        Extracted = 2, // 0010
        Modified = 4, // 0100
        Deleted = 8 // 1000
    }
}