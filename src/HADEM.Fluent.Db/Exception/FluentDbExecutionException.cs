// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Exception
{
    using System;
    using HADEM.Fluent.Db.Interfaces;

    /// <summary>
    /// Execution of an FluentDb execution. Occured during an <see cref="IFluentDbCommand"/> ExecuteAsync method.
    /// </summary>
    [Serializable]
    public class FluentDbExecutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentDbExecutionException"/> class.
        /// </summary>
        public FluentDbExecutionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentDbExecutionException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public FluentDbExecutionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentDbExecutionException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The inner exception.</param>
        public FluentDbExecutionException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentDbExecutionException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        protected FluentDbExecutionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
