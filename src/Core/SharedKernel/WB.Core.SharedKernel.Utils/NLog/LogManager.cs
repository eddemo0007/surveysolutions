﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogManager.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.Core.SharedKernel.Utils.NLog
{
    using Microsoft.Practices.ServiceLocation;

    using WB.Core.SharedKernel.Logger;

    /// <summary>
    ///     The log manager.
    /// </summary>
    public class LogManager
    {
        #region Public Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public static ILog Logger
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ILog>();
            }
        }

        #endregion
    }
}