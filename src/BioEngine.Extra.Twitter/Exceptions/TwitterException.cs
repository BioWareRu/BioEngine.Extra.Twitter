using System;

namespace BioEngine.Extra.Twitter.Exceptions
{
    public class TwitterException : Exception
    {
        public TwitterException(string message) : base(message)
        {
        }
    }
}