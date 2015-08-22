﻿using System;
using System.Runtime.Serialization;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher.Data.Submit;

namespace WB.UI.Interviewer.Infrastructure.Internals.Crasher
{
    /// <summary>
    /// This exception is thrown when an error ocurred while sending crash data in a <see cref="IReportSender"/> implementation.
    /// </summary>
    public class ReportSenderException : Exception
    {
        public ReportSenderException()
        {
        }

        public ReportSenderException(string message) : base(message)
        {
        }

        protected ReportSenderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ReportSenderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}