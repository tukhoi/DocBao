using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices.Persistence
{
    public interface IPersistentManager
    {
        Task<bool> UpdateSerializedCopyAsync(object obj, string fileName);
        Task<T> ReadSerializedCopyAsync<T>(string fileName) where T:class;

        bool UpdateSerializedCopy(object obj, string fileName, bool createBackup = true);
        T ReadSerializedCopy<T>(string fileName, bool tryBackup = true) where T:class;
    }
}
