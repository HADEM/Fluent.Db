// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Read-only collection of <see cref="DbCommandResult"/>.
    /// </summary>
    public class DbCommandResultCollection : IReadOnlyList<DbCommandResult>
    {
        private IList<DbCommandResult> results;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommandResultCollection"/> class.
        /// </summary>
        public DbCommandResultCollection()
        {
            this.results = new List<DbCommandResult>();
        }

        /// <inheritdoc />
        public int Count => this.results.Count;

        /// <summary>
        /// Gets a value indicating whether gets if the collection is readonly.
        /// </summary>
        public bool IsReadOnly => true;

        /// <inheritdoc />
        public DbCommandResult this[int index]
        {
            get => this.results[index];
            set
            {
                if (value != null)
                {
                    this.results[index] = value;
                }
            }
        }

        /// <summary>
        /// Add a <see cref="DbCommandResult"/> item in the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(DbCommandResult item)
        {
            this.results.Add(item);
        }

        /// <summary>
        /// Removes all items in the collection.
        /// </summary>
        public void Clear()
        {
            this.results.Clear();
        }

        /// <inheritdoc />
        public IEnumerator<DbCommandResult> GetEnumerator() => this.results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.results.GetEnumerator();

        /// <summary>
        /// Merge all <see cref="DbCommandResult"/> items in a single result object.
        /// </summary>
        /// <returns>A <see cref="DbCommandResult"/>.</returns>
        public DbCommandResult MergeResults()
        {
            DbCommandResult merge = new DbCommandResult();

            merge.Result = 0;
            merge.IsSuccess = false;
            merge.Exception = null;

            if (this.results.Any())
            {
                merge.Result = this.results.Sum(o => o.Result);
                if (this.results.Any(o => !o.IsSuccess))
                {
                    merge.IsSuccess = false;
                    List<string> messages = Enumerable.Empty<string>().ToList();
                    foreach (var r in this.results.Where(o => !o.IsSuccess))
                    {
                        messages.AddRange(r.Exception?.GetMessages());
                    }

                    merge.Exception = new System.Exception(string.Join(";", messages));
                }
                else
                {
                    merge.IsSuccess = true;
                }
            }

            return merge;
        }
    }
}
