// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Abstract class for any <see cref="IDisposable"/> used in acwaba solution.
    /// The purpose of this abstract class is to properly dispose resources following
    /// the <see cref="IDisposable"/> pattern.
    /// <see href="https://docs.microsoft.com/fr-fr/visualstudio/code-quality/ca1063?view=vs-2019">See the documentation.</see>.
    /// </summary>
    public abstract class DisposableObject : IDisposable
    {
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.

        /// <summary>
        /// Finalizes an instance of the <see cref="DisposableObject"/> class.
        /// </summary>
        ~DisposableObject()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        /// <summary>
        /// Execute the dispose method for the current object.
        /// </summary>
        public abstract void DoDispose();

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)

        /// <summary>
        /// Dispose the current object.
        /// </summary>
        /// <param name="disposing">Indicate if the current object is already disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Call object dispose function
                this.DoDispose();
            }

            // free native resources if there are any.
            if (this.nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.nativeResource);
                this.nativeResource = IntPtr.Zero;
            }

            this.isDisposed = true;
        }
    }
}
