using System;

namespace HpkgReader.Compat
{
    public abstract class Closable: IDisposable
    {
        private bool _disposedValue;

        public abstract void Close();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                Close();
                _disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Closable()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
