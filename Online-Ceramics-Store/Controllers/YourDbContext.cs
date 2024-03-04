
using System;

namespace Online_Ceramics_Store.Controllers
{
    internal class YourDbContext : IDisposable
    {
        // Implement IDisposable pattern
        private bool disposed = false;

        // Dispose method to release unmanaged resources
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources (if any)
                }

                // Dispose unmanaged resources (if any)

                disposed = true;
            }
        }

        // Public Dispose method
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
