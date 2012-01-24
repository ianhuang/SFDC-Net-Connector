using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SFDCNetConnector.sforce;

namespace SFDCNetConnector
{
    internal class Helper
    {
        const int RETRIEVE_BATCH_SIZE = 2000;
        const int MODIFY_BATCH_SIZE   = 200;
        const int SOAP_TIMEOUT        = 600000;

        public const string DATE_FORMAT       = "yyyy-MM-dd";
        public const string DATE_FORMAT_UTC   = "yyyy-MM-ddTHH:mm:ssZ";

        public static List<sObject> QuerysObjects(SforceServiceWrapper sforceService, string soql)
        {
            sforceService.QueryOptionsValue = new sforce.QueryOptions();
            sforceService.Timeout = SOAP_TIMEOUT;
            sforceService.QueryOptionsValue.batchSize = RETRIEVE_BATCH_SIZE;
            sforceService.QueryOptionsValue.batchSizeSpecified = true;

            List<sObject> result = new List<sObject>();
            QueryResult qr = sforceService.query(soql);

            bool queryMore = true;
            while (queryMore && (qr.size > 0))
            {
                foreach (sObject sobject in qr.records)
                    result.Add(sobject);

                if (qr.done)
                    queryMore = false;
                else
                    qr = sforceService.queryMore(qr.queryLocator);
            }
            return result;
        }

        public static void InsertsObjects(SforceServiceWrapper sforceService, List<sObject> sObjects)
        {
            if (sObjects.Count == 0) return;

            List<sObject> insertBatch = new List<sObject>();
            foreach (sObject sobject in sObjects)
            {
                insertBatch.Add(sobject);
                //insert 200 records at a time
                if (insertBatch.Count == MODIFY_BATCH_SIZE)
                {
                    SaveResult[] results = sforceService.create(insertBatch.ToArray());
                    Helper.CheckResultErrors(results);

                    insertBatch.Clear();
                }
            }
            //insert remaining records
            if (insertBatch.Count > 0)
            {
                SaveResult[] results = sforceService.create(insertBatch.ToArray());
                CheckResultErrors(results);
            }
        }

        public static void UpdatesObjects(SforceServiceWrapper sforceService, List<sObject> sObjects)
        {
            if (sObjects.Count == 0) return;

            List<sObject> updateBatch = new List<sObject>();
            foreach (sObject sobject in sObjects)
            {
                updateBatch.Add(sobject);
                //insert 200 records at a time
                if (updateBatch.Count == MODIFY_BATCH_SIZE)
                {
                    SaveResult[] results = sforceService.update(updateBatch.ToArray());
                    Helper.CheckResultErrors(results);

                    updateBatch.Clear();
                }
            }
            //insert remaining records
            if (updateBatch.Count > 0)
            {
                SaveResult[] results = sforceService.update(updateBatch.ToArray());
                CheckResultErrors(results);
            }
        }

        public static void DeletesObjects(SforceServiceWrapper sforceService, List<sObject> sObjects)
        {
            if (sObjects.Count == 0) return;

            List<string> deleteBatch = new List<string>();
            foreach (sObject sobject in sObjects)
            {
                deleteBatch.Add(sobject.Id);
                //insert 200 records at a time
                if (deleteBatch.Count == MODIFY_BATCH_SIZE)
                {
                    DeleteResult[] results = sforceService.delete(deleteBatch.ToArray());
                    Helper.CheckResultErrors(results);

                    deleteBatch.Clear();
                }
            }
            //insert remaining records
            if (deleteBatch.Count > 0)
            {
                DeleteResult[] results = sforceService.delete(deleteBatch.ToArray());
                CheckResultErrors(results);
            }
        }

        public static string FormatDate(DateTime datetime)
        {
            return datetime.ToString(Helper.DATE_FORMAT);
        }

        public static string FormatDateTimeUTC(DateTime datetime)
        {
            return datetime.ToUniversalTime().ToString(Helper.DATE_FORMAT_UTC);
        }

        public static sforce.sObject XmlToSObject(XmlElement element)
        {
            sforce.sObject obj = new sforce.sObject();
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(sforce.sObject));
                string xml = "<sObject xmlns:sf=\"urn:sobject.partner.soap.sforce.com\">" + element.InnerXml + "</sObject>";
                System.IO.MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                obj = (sforce.sObject)xs.Deserialize(ms);
                ms.Close();
                
                return obj;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return null;
        }


        public static string ArrayToString(string[] ids)
        {
            StringBuilder result = new StringBuilder();

            foreach (string id in ids)
            {
                if (result.Length == 0)
                    result.AppendFormat("'{0}'", id);
                else
                    result.AppendFormat(",'{0}'", id);
            }

            return result.ToString();
        }

        public static void CheckResultErrors(SaveResult[] saveResults)
        {
            string errMessages = string.Empty;
            foreach (SaveResult sr in saveResults)
            {
                if (!sr.success)
                {
                    foreach (Error err in sr.errors)
                        errMessages += err.message + "\n";
                }
            }
            if (errMessages != string.Empty)
                throw new Exception(errMessages);
        }

        public static void CheckResultErrors(DeleteResult[] deleteResults)
        {
            string errMessages = string.Empty;
            foreach (DeleteResult dr in deleteResults)
            {
                if (!dr.success)
                {
                    foreach (Error err in dr.errors)
                        errMessages += err.message + "\n";
                }
            }
            if (errMessages != string.Empty)
                throw new Exception(errMessages);
        }
    }
}
