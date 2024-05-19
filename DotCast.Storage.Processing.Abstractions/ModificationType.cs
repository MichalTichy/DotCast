namespace DotCast.Storage.Processing.Abstractions
{
    [Flags]
    public enum ModificationType
    {
        Empty = 0, // 0000
        Extracted = 2, // 0010
        FileContentModified = 4, // 0100
        Deleted = 8, // 1000
        Renamed = 16 // 1000
    }
}