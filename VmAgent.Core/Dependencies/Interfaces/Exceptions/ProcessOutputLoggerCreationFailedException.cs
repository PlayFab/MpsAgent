using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VmAgent.Core.Dependencies.Interfaces.Exceptions
{
    public class ProcessOuputLoggerCreationFailedException : Exception
    {
        public ProcessOuputLoggerCreationFailedException(string message) : base(message)
        {
          
        }

        #if NET8_0_OR_GREATER
        [Obsolete(DiagnosticId = "SYSLIB0051")] 
        #endif
        protected ProcessOuputLoggerCreationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
           
        }

    }
}
