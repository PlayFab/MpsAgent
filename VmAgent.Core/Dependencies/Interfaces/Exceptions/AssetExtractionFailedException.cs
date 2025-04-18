using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VmAgent.Core.Dependencies.Interfaces.Exceptions
{
    public class AssetExtractionFailedException : Exception
    {
        public AssetExtractionFailedException(string message) : base(message)
        {
          
        }
    }
}
