// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Retry policy option.
    /// </summary>
    public class RetryPolicyOption
    {
        /// <summary>
        /// Gets or Sets the maximum retries times.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Gets or Sets the exceptions concerned by the retry policy.
        /// </summary>
        public IList<Type>? Exceptions { get; set; }

        /// <summary>
        /// Create the default <see cref="RetryPolicyOption"/>.
        /// </summary>
        /// <returns>A <see cref="RetryPolicyOption"/>.</returns>
        public static RetryPolicyOption DefaultRetryPolicy()
        {
            var retry = new RetryPolicyOption();
            retry.MaxRetries = 1;
            retry.ShouldRetryOn<System.Exception>();

            return retry;
        }

        /// <summary>
        /// Specify which exception should be concerned by the retry policy.
        /// </summary>
        /// <typeparam name="T">Exception.</typeparam>
        public RetryPolicyOption ShouldRetryOn<T>()
            where T : System.Exception
        {
            if (this.Exceptions == null)
            {
                this.Exceptions = new List<Type>();
            }

            this.Exceptions.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Identify if the <typeparamref name="T"/> is concerned by the retry policy.
        /// </summary>
        /// <param name="exception">Exception that occured.</param>
        /// <returns>True or false.</returns>
        public bool ShouldThrowExceptionForRetry<T>(T exception)
            where T : System.Exception => this.Exceptions!.Contains(typeof(T)) && exception != null;
    }
}
