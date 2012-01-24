using System;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace SFDCNetConnector
{
	/// <summary>
	/// Summary description for WebRequestWrapper.
	/// </summary>
	public class WebRequestWrapper : WebRequest
	{
		internal const string GZIP = "gzip";
		private bool gzipRequest;
		private WebRequest wr;

		/// <summary>
		/// This constructor will send an uncompressed request, and indicate that we can accept a compressed response.
		/// You should be able to use this anywhere to get automatic support for handling compressed responses
		/// </summary>
		/// <param name="wrappedRequest"></param>
		public WebRequestWrapper(WebRequest wrappedRequest) : this(wrappedRequest, false, true) 
		{
		}

		/// <summary>
		/// This constructor allows to indicate if you want to compress the request, and if you want to indicate that you can handled a compressed response
		/// </summary>
		/// <param name="wrappedRequest">The WebRequest we're wrapping.</param>
		/// <param name="compressRequest">if true, we will gzip the request message.</param>
		/// <param name="acceptCompressedResponse">if true, we will indicate that we can handle a gzip'd response, and decode it if we get a gziped response.</param>
		public WebRequestWrapper(WebRequest wrappedRequest, bool compressRequest, bool acceptCompressedResponse) 
		{
			this.wr = wrappedRequest;
            this.gzipRequest = compressRequest;
			if(this.gzipRequest)
				wr.Headers["Content-Encoding"] = GZIP;
			if(acceptCompressedResponse)
				wr.Headers["Accept-Encoding"] = GZIP;
		}

		#region These are straight pass-through delegates to the contained webrequest
		// most of these just delegate to the contained WebRequest
		public override string Method
		{
			get { return wr.Method; }
			set { wr.Method = value; }
		}
	
		public override Uri RequestUri
		{
			get { return wr.RequestUri; }
		}
	
		public override WebHeaderCollection Headers
		{
			get { return wr.Headers; }
			set { wr.Headers = value; }
		}
	
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
	
		public override ICredentials Credentials
		{
			get { return wr.Credentials; }
			set { wr.Credentials = value; }
		}
	
		public override bool PreAuthenticate
		{
			get { return wr.PreAuthenticate; }
			set { wr.PreAuthenticate = value; }
		}
	
		#endregion

		private Stream request_stream = null;

		public override System.IO.Stream GetRequestStream()
		{
			return WrappedRequestStream(wr.GetRequestStream());
		}

		public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			return wr.BeginGetRequestStream (callback, state);
		}

		public override System.IO.Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			return WrappedRequestStream(wr.EndGetRequestStream (asyncResult));
		}

		/// <summary>
		/// helper function that wraps the request stream in a GzipOutputStream, 
		/// if we're going to be compressing the request
		/// </summary>
		/// <param name="requestStream"></param>
		/// <returns></returns>
		private Stream WrappedRequestStream(Stream requestStream)
		{
			if (request_stream == null)
			{
				request_stream = requestStream;
				if(this.gzipRequest)
					//request_stream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(request_stream);
                    request_stream = new GZipStream(request_stream, CompressionMode.Compress);
			}
			return request_stream;
		}

		public override WebResponse GetResponse()
		{
			return new WebResponseWrapper(wr.GetResponse()); 
		}

		public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			return wr.BeginGetResponse(callback, state);
		}

		public override WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			return new WebResponseWrapper(wr.EndGetResponse(asyncResult)); 
		}
	}
}
