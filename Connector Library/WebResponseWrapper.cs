using System;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace SFDCNetConnector
{
	/// <summary>
	/// Summary description for WebResponseWrapper.
	/// </summary>
	public class WebResponseWrapper : WebResponse	
	{
		private WebResponse wr;
		private Stream response_stream = null;

		internal WebResponseWrapper(WebResponse wrapped)
		{
			this.wr = wrapped;
		}

		/// <summary>
		/// Wrap the returned stream in a gzip uncompressor if needed
		/// </summary>
		/// <returns></returns>
		public override Stream GetResponseStream()
		{
			if ( response_stream == null )
			{
				response_stream = wr.GetResponseStream();
				if (string.Compare(Headers["Content-Encoding"], "gzip", true) == 0)
					//response_stream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(response_stream);
                    response_stream = new GZipStream(response_stream, CompressionMode.Decompress);
			}
			return response_stream;
		}

		// these all delegate to the contained WebResponse
		public override long ContentLength
		{
			get { return wr.ContentLength; }
			set { wr.ContentLength = value; }
		}
	
		public override string ContentType
		{
			get { return wr.ContentType; }
			set { wr.ContentType = value; }
		}
	
		public override Uri ResponseUri
		{
			get { return wr.ResponseUri; }
		}
	
		public override WebHeaderCollection Headers
		{
			get { return wr.Headers; }
		}
	}
}
