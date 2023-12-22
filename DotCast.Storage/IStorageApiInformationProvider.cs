using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotCast.Storage
{
    public interface IStorageApiInformationProvider
    {
        string GetFileUrl(string audioBookId, string fileName, bool isArchive);
    }
}