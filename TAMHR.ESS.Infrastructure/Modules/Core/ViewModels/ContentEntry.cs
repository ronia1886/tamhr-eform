using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ContentEntry
    {
        public byte[] Contents { get; set; }
        public string FileName { get; set; }

        public ContentEntry(string fileName, byte[] contents)
        {
            Contents = contents;
            FileName = fileName;
        }
    }
}
