using System;
using System.IO.Compression;
using System.Collections;
using SFDCNetConnector.sforce;

namespace SFDCNetConnector
{
    public delegate void RequestTiming(System.TimeSpan elapsedTime);

    public class SforceServiceWrapper: SforceService
    {
        #region Public enumerations

        public enum CompressionMode
        {
            Automatic,
            Plain,
            Compressed
        }

        #endregion

        #region Private Properties

        private int maxRowCount = 200;
        private int soapTimeout = 600000;
        private CompressionMode responseCompression;
        private CompressionMode requestCompression;
		private LoginResult loginInfo = new LoginResult();

        private bool acceptCompressedResponse;
        private bool sendCompressedRequest;

        private const bool DefaultSendCompressedRequest    = false;
        private const bool DefaultAcceptCompressedResponse = true;

        #endregion

        #region Public Properties

		/// <summary>
		/// Property to control batch-sizes, default set as 200.
		/// The Wrapper will break up operations into batches of this size when arrays larger than this are requested.
		/// </summary>
		public int MaxRowCount
		{
			get
			{
				return maxRowCount;
			}
			set
			{
				maxRowCount = value;
			}
		}

		/// <summary>
		/// Property to toggle compression on and off, set to true to enable sending compressed requests, false to disable.
		/// </summary>
		public CompressionMode RequestCompression
		{
			get
			{
				return requestCompression;
			}
			set
			{
				requestCompression = value;
				switch(value)
				{
					case CompressionMode.Automatic:  sendCompressedRequest = DefaultSendCompressedRequest; break;
					case CompressionMode.Plain:      sendCompressedRequest = false;                        break;
					case CompressionMode.Compressed: sendCompressedRequest = true;                         break;
				}
			}
		}

		/// <summary>
        /// Property to toggle compression on and off, set to true to enable receiving compressed responses, false to disable.
        /// </summary>
        public CompressionMode ResponseCompression
        {
            get
            {
                return responseCompression;
            }
            set
            {
                responseCompression = value;
                switch(value)
                {
                    case CompressionMode.Automatic:  acceptCompressedResponse = DefaultAcceptCompressedResponse; break;
                    case CompressionMode.Plain:      acceptCompressedResponse = false;                           break;
                    case CompressionMode.Compressed: acceptCompressedResponse = true;                            break;
                }
            }
        }

		/// <summary>
		/// Property containing the LoginResult returned by the login-request.
		/// </summary>
		public LoginResult LoginInfo
		{
			get
			{
				return loginInfo;
			}
		}

        #endregion

        #region Constructors

        /// <summary>
        /// This constructor will create a request that has has it's initial compression
        /// attributes set to the parameter values.
        /// </summary>
        /// <param name="responseCompression">
        /// Set to true to send a zipped request, false to send plain response.
        /// </param>
        /// <param name="requestCompression">
        /// Set to Automatic to use automatic acceptance of zipped/uncompressed response (accept zipped in most cases), Plain to accept plain responses only.
        /// </param>
        public SforceServiceWrapper(CompressionMode requestCompression, CompressionMode responseCompression)
        {
            RequestCompression = requestCompression;
            ResponseCompression = responseCompression;

            this.EnableDecompression = true;
            this.Timeout = soapTimeout;
        }

        /// <summary>This constructor will create a request that has compression automatic by default (can be enabled later).</summary>
        /// <remarks>
        /// You can later set compression instructions using the SendCompressedRequest
        /// property and the <para>AcceptCompressedResponse property to turn compression on and off.</para></remarks>
        public SforceServiceWrapper() : this(CompressionMode.Automatic, CompressionMode.Automatic)
        {
            this.Timeout = soapTimeout;
        }

        #endregion

		#region Public Methods
		public void CreateLeadAssignmentRuleHeader()
		{
			//create the autoassignment rule for leads
			AssignmentRuleHeaderValue = new AssignmentRuleHeader();
			
			QueryResult qr = query("Select Id From AssignmentRule Where SobjectType='Lead' And Active=true");
			
			if (qr.size == 0)
				AssignmentRuleHeaderValue.useDefaultRule = true;
			else
				AssignmentRuleHeaderValue.assignmentRuleId = qr.records[0].Id;
		}

		public void CreateCaseAssignmentRuleHeader()
		{
			//create the autoassignment rule for leads
			AssignmentRuleHeaderValue = new AssignmentRuleHeader();
			
			QueryResult qr = query("Select Id From AssignmentRule Where SobjectType='Case' And Active=true");
			
			if (qr.size == 0)
				AssignmentRuleHeaderValue.useDefaultRule = true;
			else
				AssignmentRuleHeaderValue.assignmentRuleId = qr.records[0].Id;
		}
		#endregion

        #region Private Methods

        private void SetAutoRequestCompressed()
        {
            if (requestCompression == CompressionMode.Automatic)
                sendCompressedRequest = true;
        }

        private void SetAutoRequestPlain()
        {
            if (requestCompression == CompressionMode.Automatic)
                sendCompressedRequest = false;
        }

        private void SetAutoResponseCompressed()
        {
            if (responseCompression == CompressionMode.Automatic)
                acceptCompressedResponse = true;
        }

        private void SetAutoResponsePlain()
        {
            if (responseCompression == CompressionMode.Automatic)
                acceptCompressedResponse = false;
        }

        private void ResetCompressionModes()
        {
            if (requestCompression == CompressionMode.Automatic)
                sendCompressedRequest = DefaultSendCompressedRequest;
            if (responseCompression == CompressionMode.Automatic)
                acceptCompressedResponse = DefaultAcceptCompressedResponse;
        }

        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            //return new WebRequestWrapper(base.GetWebRequest(uri), sendCompressedRequest, acceptCompressedResponse);
            return new WebRequestWrapper(base.GetWebRequest(uri));
        }

        #endregion

        #region Events

        public event RequestTiming OnConvertLeadComplete;
        public event RequestTiming OnCreateComplete;
        public event RequestTiming OnDeleteComplete;
        public event RequestTiming OnDescribeSObjectsComplete;
        public event RequestTiming OnGetDeletedComplete;
        public event RequestTiming OnGetUpdatedComplete;
        public event RequestTiming OnQueryComplete;
        public event RequestTiming OnQueryMoreComplete;
        public event RequestTiming OnRetrieveComplete;
        public event RequestTiming OnSearchComplete;
        public event RequestTiming OnUpdateComplete;
        public event RequestTiming OnUpsertComplete;

        #endregion

        #region Overridden Public Methods

		public new LoginResult login(string username, string password)
		{
			loginInfo = base.login(username, password);
			SessionHeaderValue = new SessionHeader();
			SessionHeaderValue.sessionId = loginInfo.sessionId;
			return loginInfo;
		}

        public new LeadConvertResult[] convertLead(LeadConvert[] leadConverts)
        {
            long start = DateTime.Now.Ticks;
            if (leadConverts.Length > 1)
                SetAutoRequestCompressed();
            else
                SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.convertLead(leadConverts);
            }
            finally
            {
                if (OnConvertLeadComplete != null)
                    OnConvertLeadComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new SaveResult[] create(sObject[] sObjects)
        {
            long start = DateTime.Now.Ticks;
            if (sObjects.Length > 1)
                SetAutoRequestCompressed();
            else
                SetAutoRequestPlain();
            SetAutoResponseCompressed();

            try
            {
                if (sObjects.Length <= maxRowCount)
                    return base.create(sObjects);
                else
                {
                    SaveResult[] results = new SaveResult[sObjects.Length];
                    for (int startPos = 0; startPos < sObjects.Length; startPos += maxRowCount) 
                    {
                        sObject[] buffer = new sObject[Math.Min(maxRowCount, sObjects.Length - startPos)];
                        Array.Copy(sObjects, startPos, buffer, 0, buffer.Length);
                        (base.create(buffer)).CopyTo(results, startPos);
                    }
                    return results;
                }
            }
            finally
            {
                if (OnCreateComplete != null)
                    OnCreateComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new DeleteResult[] delete(string[] ids)
        {
            long start = DateTime.Now.Ticks;
            if (ids.Length > 1)
                SetAutoRequestCompressed();
            else
                SetAutoRequestPlain();
            SetAutoResponseCompressed();

            try
            {
                if (ids.Length <= maxRowCount)
                    return base.delete(ids);
                else
                {
                    DeleteResult[] results = new DeleteResult[ids.Length];
                    for (int startPos = 0; startPos < ids.Length; startPos += maxRowCount) 
                    {
                        string[] buffer = new string[Math.Min(maxRowCount, ids.Length - startPos)];
                        Array.Copy(ids, startPos, buffer, 0, buffer.Length);
                        (base.delete(buffer)).CopyTo(results, startPos);
                    }
                    return results;
                }
            }
            finally
            {
                if (OnDeleteComplete != null)
                    OnDeleteComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }            
        }

        public new DescribeSObjectResult[] describeSObjects(string[] sObjectType)
        {
            long start = DateTime.Now.Ticks;
            if (sObjectType.Length > 1)
                SetAutoRequestCompressed();
            else
                SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.describeSObjects(sObjectType);
            }
            finally
            {
                if (OnDescribeSObjectsComplete != null)
                    OnDescribeSObjectsComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new GetDeletedResult getDeleted(String sObjectType, DateTime startDate, DateTime endDate)
        {
            long start = DateTime.Now.Ticks;
            SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.getDeleted(sObjectType, startDate, endDate);
            }
            finally
            {
                if (OnGetDeletedComplete != null)
                    OnGetDeletedComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new GetUpdatedResult getUpdated(String sObjectType, DateTime startDate, DateTime endDate)
        {
            long start = DateTime.Now.Ticks;
            SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.getUpdated(sObjectType, startDate, endDate);
            }
            finally
            {
                if (OnGetUpdatedComplete != null)
                    OnGetUpdatedComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new QueryResult query(string queryString)
        {
            long start = DateTime.Now.Ticks;
            SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.query(queryString);
            }
            finally
            {
                if (OnQueryComplete != null)
                    OnQueryComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new QueryResult queryMore(String queryLocator)
        {
            long start = DateTime.Now.Ticks;
            SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.queryMore(queryLocator);
            }
            finally
            {
                if (OnQueryMoreComplete != null)
                    OnQueryMoreComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new sObject[] retrieve(String fieldList, String sObjectType, string[] ids)
        {
            long start = DateTime.Now.Ticks;
            if (ids.Length > 1)
                SetAutoRequestCompressed();
            else
                SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.retrieve(fieldList, sObjectType, ids);
            }
            finally
            {
                if (OnRetrieveComplete != null)
                    OnRetrieveComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new SearchResult search(String searchString)
        {
            long start = DateTime.Now.Ticks;
            SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                return base.search(searchString);
            }
            finally
            {
                if (OnSearchComplete != null)
                    OnSearchComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new SaveResult[] update(sObject[] sObjects)
        {
            long start = DateTime.Now.Ticks;
            if (sObjects.Length > 1)
                SetAutoRequestCompressed();
            else
                SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                if (sObjects.Length <= maxRowCount)
                    return base.update(sObjects);
                else
                {
                    SaveResult[] results = new SaveResult[sObjects.Length];
                    for (int startPos = 0; startPos < sObjects.Length; startPos += maxRowCount) 
                    {
                        sObject[] buffer = new sObject[Math.Min(maxRowCount, sObjects.Length - startPos)];
                        Array.Copy(sObjects, startPos, buffer, 0, buffer.Length);
                        (base.update(buffer)).CopyTo(results, startPos);
                    }
                    return results;
                }
            }
            finally
            {
                if (OnUpdateComplete != null)
                    OnUpdateComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        public new UpsertResult[] upsert(String externalIDFieldName, sObject[] sObjects)
        {            
            long start = DateTime.Now.Ticks;
            if (sObjects.Length > 1)
                SetAutoRequestCompressed();
            else
                SetAutoRequestPlain();
            SetAutoResponseCompressed();
            try
            {
                if (sObjects.Length <= maxRowCount)
                    return base.upsert(externalIDFieldName, sObjects);
                else
                {
                    UpsertResult[] results = new UpsertResult[sObjects.Length];
                    for (int startPos = 0; startPos < sObjects.Length; startPos += maxRowCount) 
                    {
                        sObject[] buffer = new sObject[Math.Min(maxRowCount, sObjects.Length - startPos)];
                        Array.Copy(sObjects, startPos, buffer, 0, buffer.Length);
                        (base.upsert(externalIDFieldName, buffer)).CopyTo(results, startPos);
                    }
                    return results;
                }
            }
            finally
            {
                if (OnUpsertComplete != null)
                    OnUpsertComplete(new TimeSpan(DateTime.Now.Ticks - start));
                ResetCompressionModes();
            }
        }

        #endregion
    }
}
