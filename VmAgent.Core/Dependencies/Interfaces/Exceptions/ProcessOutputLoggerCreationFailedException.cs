using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VmAgent.Core.Dependencies.Interfaces.Exceptions
{
    [Serializable]
    public class ProcessOuputLoggerCreationFailedException : Exception
    {
        public ProcessOuputLoggerCreationFailedException(string message) : base(message)
        {
          
        }

        protected ProcessOuputLoggerCreationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
           
        }

    }
}
