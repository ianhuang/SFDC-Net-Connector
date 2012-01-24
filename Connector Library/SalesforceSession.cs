using System;
using System.Configuration;
using SFDCNetConnector.sforce;

namespace SFDCNetConnector
{
	/// <summary>
	/// SalesforceSession is the singleton-class for creating a connection to the physical data-store.
    /// SalesforceSession encapsulates the implementation details how to connect to the web service
	/// and delivers instances of an overridden SforceService which are setup to connect to the correct
	/// session and has 2 extra properties to control compression of the request and response.
	/// </summary>//internal
	public class SalesforceSession
	{
		#region Constructors

		/// <summary>
		/// Prevent creating instances of this class, it's methods are shared
		/// </summary>
		private SalesforceSession()
		{
		}

		#endregion

		#region Public Methods

        public static SforceServiceWrapper StartSession(string username, string password)
        {
            return StartSession(username, password, string.Empty);
        }

        public static SforceServiceWrapper StartSession(string username, string password, string securityToken)
		{
			SforceServiceWrapper service = new SforceServiceWrapper();
            LoginResult lr = service.login(username, password + securityToken);
            service.Url = lr.serverUrl;
			return service;
		}

		public static SforceServiceWrapper GetSession(string sessionId)
		{
			SforceServiceWrapper service = new SforceServiceWrapper();
            service.SessionHeaderValue = new SessionHeader();
            service.SessionHeaderValue.sessionId = sessionId;
            return service;
		}

		#endregion
	}
}
