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

        public DbCommandResultCollection()
        {
            this.results = new List<DbCommandResult>();
        }

        public int Count => this.results.Count;

        public bool IsReadOnly => true;

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

        public void Add(DbCommandResult item)
        {
            this.results.Add(item);
        }

        public void Clear()
        {
            this.results.Clear();
        }

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
            merge.Exception = new System.Exception("No result found");

            if (this.results.Any())
            {
                merge.Result = this.results.Sum(o => o.Result);
                if (this.results.Any(o => !o.IsSuccess))
                {
                    merge.IsSuccess = false;
                    IEnumerable<string> messages = Enumerable.Empty<string>();
                    foreach (var r in this.results.Where(o => !o.IsSuccess))
                    {
                        messages.Concat(r.Exception.GetMessages());
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
