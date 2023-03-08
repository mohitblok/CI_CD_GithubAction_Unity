using System;

public struct RequestData
{
    public string path;
    public int retryCount;
    public Action<string> onSuccess;
    public Action<string> onError;
    public string token; // If needed
}
