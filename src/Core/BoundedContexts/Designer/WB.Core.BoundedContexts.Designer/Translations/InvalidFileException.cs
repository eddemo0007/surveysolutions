using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    [Serializable]
    public class InvalidFileException : Exception
    {
        public InvalidFileException()
        {
        }

        public InvalidFileException(string message)
            : base(message)
        {
        }

        public InvalidFileException(string message, Exception inner)
            : base(message, inner)
        {
        }        

        public List<ImportValidationError>? FoundErrors { get; set; }
    }
}
