using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Concurrent;

/// <summary>
/// util to check file hash
/// </summary>
public static class HashChecker
{
    static ConcurrentQueue<HashItem> fileQueue = new ConcurrentQueue<HashItem>();
    static CancellationTokenSource cts = new CancellationTokenSource();
    private static Task task;

    /// <summary> The HashCheckFileAsync function checks the hash of a file and calls a callback function if it matches.</summary>
    /// <param name="path"> The path of the file to hash.</param>
    /// <param name="hash"> The string to hash.</param>
    /// <param name="successCallback">Called on success</param>
    /// <param name="failedCallback">Called on Fail</param>
    public static void HashCheckFileAsync(string path, string hash, Action successCallback, Action failedCallback)
    {
        var hashItem = new HashItem(path, hash, successCallback, failedCallback);
        fileQueue.Enqueue(hashItem);

        if (task != null)
        {
            if (!task.IsCompleted && !task.IsCanceled)
                return;
        }

        task = Task.Run(() => ProcessQueue(fileQueue, cts.Token));
    }

    private static void ProcessQueue(ConcurrentQueue<HashItem> fileQueue, CancellationToken token)
    {
        while (fileQueue.Count > 0)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            // Dequeue a file path from the queue
            if (fileQueue.TryDequeue(out HashItem hashItem))
            {
                if (!File.Exists(hashItem.Path))
                {
                    MainThreadDispatcher.Instance.Enqueue(hashItem.FailedCallback);
                    continue;
                }

                var hash = GetHash(hashItem.Path);

                if (hashItem.Hash == hash)
                {
                    MainThreadDispatcher.Instance.Enqueue(hashItem.SuccessCallback);
                }
                else
                {
                    MainThreadDispatcher.Instance.Enqueue(hashItem.FailedCallback);
                }
            }
        }
    }

    /// <summary> The GetHash function computes the MD5 hash of a file and returns it as a string.</summary>
    /// <param name="path"> The path to the file</param>
    /// <returns> A string of hexadecimal digits that represent the md5 hash value of a file.</returns>
    public static string GetHash(string path)
    {
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            using (FileStream stream = File.OpenRead(path))
            {
                // Compute the hash of the file
                byte[] hash = md5.ComputeHash(stream);

                var hashString = BitConverter.ToString(hash);

                // Print the hash
                Console.WriteLine(path + " : " + hashString);

                return hashString;
            }
        }

        return null;
    }

    /// <summary>
    /// HashItem data structure
    /// </summary>
    private class HashItem
    {
        // The path of the file to hash.
        public string Path { get; private set; }

        // The string to hash.
        public string Hash { get; private set; }

        // Called on success
        public Action SuccessCallback { get; private set; }

        // Called on Fail
        public Action FailedCallback { get; private set; }

        /// <summary> The HashItem function creates a new HashItem object with the given path, hash, success callback and failed callback.</summary>
        /// <param name="path"> The path to the file</param>
        /// <param name="hash"> The string to hash.</param>
        /// <param name="successCallback"> Called on success</param>
        /// <param name="failedCallback"> Called on Fail</param>
        /// <returns> A HashItem object.</returns>
        public HashItem(string path, string hash, Action successCallback, Action failedCallback)
        {
            Path = path;
            Hash = hash;
            SuccessCallback = successCallback;
            FailedCallback = failedCallback;
        }
    }
}