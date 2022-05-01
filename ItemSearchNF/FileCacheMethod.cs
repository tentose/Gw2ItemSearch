using Gw2Sharp.WebApi.Caching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ItemSearch
{
    internal class FileCacheMethod : BaseCacheMethod
    {
        // No easy way to store expiry in the file system. Just pick a constant since we're storing known data.
        private static readonly TimeSpan DEFAULT_EXPIRY = TimeSpan.FromDays(7);
        private const int MAX_CONCURRENT_READERS = 8;

        private string m_cacheRootPath;
        private ConcurrentDictionary<string, ReaderWriterLockSlim> m_accessedResources = new ConcurrentDictionary<string, ReaderWriterLockSlim>();
        private SemaphoreSlim m_readerSemaphore;

        public FileCacheMethod(string cacheRootPath)
        {
            m_cacheRootPath = cacheRootPath;
            m_readerSemaphore = new SemaphoreSlim(MAX_CONCURRENT_READERS);
        }

        public override Task ClearAsync()
        {
            Directory.Delete(m_cacheRootPath, true);
            Directory.CreateDirectory(m_cacheRootPath);
            return Task.CompletedTask;
        }

        public override Task SetAsync(CacheItem item)
        {
            if (item.Type != Gw2Sharp.CacheItemType.Raw)
            {
                throw new Exception("Can only cache raw");
            }

            var rwlock = m_accessedResources.GetOrAdd(item.Id, (string s) => new ReaderWriterLockSlim());
            rwlock.EnterWriteLock();
            try
            {
                string hash = ComputeHashForString(item.Id);
                string path = Path.Combine(m_cacheRootPath, hash);
                File.WriteAllBytes(path, item.RawItem);
            }
            finally
            {
                rwlock.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        public override async Task<CacheItem> TryGetAsync(string category, string id)
        {
            CacheItem result = null;
            await m_readerSemaphore.WaitAsync();

            try
            {
                var rwlock = m_accessedResources.GetOrAdd(id, (string s) => new ReaderWriterLockSlim());
                rwlock.EnterUpgradeableReadLock();
                try
                {
                    string hash = ComputeHashForString(id);
                    string path = Path.Combine(m_cacheRootPath, hash);

                    if (File.Exists(path))
                    {
                        var expiryTime = File.GetCreationTime(path) + DEFAULT_EXPIRY;
                        if (DateTime.Now > expiryTime)
                        {
                            rwlock.EnterWriteLock();
                            try
                            {
                                File.Delete(path);
                            }
                            finally
                            {
                                rwlock.ExitWriteLock();
                            }
                        }
                        else
                        {
                            var bytes = File.ReadAllBytes(path);
                            result = new CacheItem(category, id, bytes, System.Net.HttpStatusCode.OK, expiryTime, Gw2Sharp.CacheItemStatus.Cached);
                        }
                    }
                }
                finally
                {
                    rwlock.ExitUpgradeableReadLock();
                }
            }
            finally
            {
                m_readerSemaphore.Release();
            }
            return result;
        }

        private string ComputeHashForString(string s)
        {
            using (var algorithm = SHA1Managed.Create())
            {
                var bytes = algorithm.ComputeHash(Encoding.ASCII.GetBytes(s));
                return BitConverter.ToString(bytes);
            }
        }
    }
}
