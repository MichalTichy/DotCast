namespace DotCast.Infrastructure.M3U
{
    public class M3uManager
    {
        public async Task<ICollection<string>> ReadM3uFile(string filePath)
        {
            List<string> fileLines = new List<string>();

            // Read all lines from the specified file
            var lines = await File.ReadAllLinesAsync(filePath);

            foreach (string line in lines)
            {
                // Assuming you want to skip empty lines and comments
                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                {
                    fileLines.Add(line);
                }
            }

            return fileLines;
        }

        public async Task GenerateM3uFile(IEnumerable<string> filePaths, string savePath)
        {
            await using var file = new StreamWriter(savePath);
            // Write the file header
            await file.WriteLineAsync("#EXTM3U");

            foreach (var path in filePaths)
            {
                // Write the file path to the M3U file
                await file.WriteLineAsync(path);
            }
        }
    }
}
