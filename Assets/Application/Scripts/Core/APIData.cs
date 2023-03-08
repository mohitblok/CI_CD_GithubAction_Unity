public class APIData
{
    public abstract class Data{}
    
    public class Response<T> 
    {
        public string message;
        public T data;
    }

    public class AuthenticateDevice : Data
    {
        public string originalMessage;
        public string signedMessage;
    }
}
