using System;
using System.Runtime.Serialization;

namespace OneCopy2017.DataObjects
{
    [Serializable]
    public class OneCopy2017Exception : Exception
    {
        public OneCopy2017Exception()
        {
        }

        public OneCopy2017Exception(string message) : base(message)
        {
        }

        public OneCopy2017Exception(string message, Exception inner) : base(message, inner)
        {
        }

        protected OneCopy2017Exception(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}