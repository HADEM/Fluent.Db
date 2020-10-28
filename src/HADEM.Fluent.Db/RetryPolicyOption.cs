// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
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
        /// Gets or sets a value indicating whether we should retry
        /// the command execution after an exception.
        /// </summary>
        public bool RetryOnException { get; set; }
    }
}
