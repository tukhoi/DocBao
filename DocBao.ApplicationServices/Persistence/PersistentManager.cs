using Davang.Utilities.Helpers;
using Davang.Utilities.Helpers.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DocBao.ApplicationServices.Persistence
{
    public sealed class PersistentManager : IPersistentManager
    {
        private SerializationHelperManager _serializationManager;
        private static string _mutextName = "SubscribedFeedData";

        public PersistentManager(SerializationHelperManager serializationManager = null)
        {
            _serializationManager = serializationManager ?? new SerializationHelperManager();
        }

        public async Task<bool> UpdateSerializedCopyAsync(object obj, string fileName)
        {
            var mutex = new Mutex(false, _mutextName);
            try
            {
                mutex.WaitOne();
                var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
                var localStream = await StorageHelper.OpenStreamForWriteAsync(fileName, false);
                if (!StorageHelper.EnoughSpace()) return false;
                return await serializationHelper.SerializeAsync(localStream, obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try { mutex.ReleaseMutex(); }
                catch (Exception) { }
                mutex.Dispose();
            }
        }

        public async Task<T> ReadSerializedCopyAsync<T>(string fileName)
            where T : class
        {
            try
            {
                var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
                var localStream = await StorageHelper.OpenStreamForReadAsync(fileName, AppConfig.Backup);

                //There's no map file. User hasn't registered any pod
                if (localStream == null) return default(T);

                T graph = default(T);

                graph = await serializationHelper.DeserializeAsync<T>(localStream);

                if (graph == default(T))
                    await StorageHelper.DeleteAsync(fileName, AppConfig.Backup);

                return graph;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateSerializedCopy(object obj, string fileName, bool createBackup = true)
        {
            var mutex = new Mutex(false, _mutextName);
            try
            {
                mutex.WaitOne();
                var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
                var localStream = StorageHelper.GetFileStream(fileName);

                if (!StorageHelper.EnoughSpace()) return false;
                var serialized = serializationHelper.Serialize(localStream, obj);
                if (serialized && createBackup)
                    StorageHelper.CopyFile(fileName, fileName + ".bak");

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally 
            {
                try { mutex.ReleaseMutex(); }
                catch (Exception) { }
                mutex.Dispose();
            }
        }

        public T ReadSerializedCopy<T>(string fileName, bool tryBackup = true)
            where T : class
        {
            try
            {
                var stream = StorageHelper.GetFileStream(fileName);
                if (stream == null && tryBackup)
                    stream = StorageHelper.GetFileStream(fileName + ".bak");

                if (stream == null) return default(T);

                var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
                T graph = default(T);

                graph = serializationHelper.Deserialize<T>(stream);
                return graph;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
