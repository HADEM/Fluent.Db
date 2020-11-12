// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Exception
{
    /// <summary>
    /// Exception occured when max retry count have been reached.
    /// <see cref="RetryPolicyOption"/>.
    /// </summary>
    public class RetryReachedException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReachedException"/> class.
        /// </summary>
        public RetryReachedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReachedException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public RetryReachedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReachedException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The inner exception.</param>
        public RetryReachedException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReachedException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        protected RetryReachedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
