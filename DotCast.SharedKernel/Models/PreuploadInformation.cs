using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotCast.SharedKernel.Models
{
    public record PreuploadFileInformation(string FileName, string UploadUrl, bool AlreadyExists);
}
