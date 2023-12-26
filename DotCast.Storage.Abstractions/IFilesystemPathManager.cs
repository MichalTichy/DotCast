using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotCast.Storage.Abstractions
{
    public interface IFilesystemPathManager
    {
        string GetTargetFilePath(string audioBookId, string fileName);
        bool IsArchive(string filePath);
        string GetAudioBooksLocation();
        string GetAudioBooksZipDirectoryLocation();
        string GetAudioBookLocation(string audiobookId);
    }
}
