using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VmAgent.Core.Dependencies.Interfaces.Exceptions
{
    [Serializable]
    public class AssetExtractionFailedException : Exception
    {
        public AssetExtractionFailedException(string message) : base(message)
        {
          
        }

        protected AssetExtractionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
           
        }

    }
}
