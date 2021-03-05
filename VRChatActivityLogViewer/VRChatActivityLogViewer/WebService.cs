using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// Webへのアクセスを提供するサービス
    /// </summary>
    public class WebService
    {
        /// <summary>
        /// HttpClient
        /// </summary>
        private static HttpClient client = new HttpClient();

        /// <summary>
        /// URIを指定して画像をダウンロードする
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<WrappingStream> GetStreamAsync(string uri)
        {
            var byteArray = await client.GetByteArrayAsync(uri);
            return new WrappingStream(new MemoryStream(byteArray));
        }
    }

    /// <summary>
    /// Dispose後にキャッシュを破棄するStreamのラッパー
    /// </summary>
    public class WrappingStream : Stream
    {
        Stream innerStream;

        public WrappingStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            innerStream = stream;
        }

        public override bool CanRead
        {
            get
            {
                ThrowIfDisposed();
                return innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                ThrowIfDisposed();
                return innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                ThrowIfDisposed();
                return innerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                ThrowIfDisposed();
                return innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return innerStream.Position;
            }
            set
            {
                ThrowIfDisposed();
                innerStream.Position = value;
            }
        }

        public override void Flush()
        {
            ThrowIfDisposed();
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            innerStream.Write(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public new Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return innerStream.ReadAsync(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                innerStream.Dispose();
                innerStream = null;
            }

            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (innerStream == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
