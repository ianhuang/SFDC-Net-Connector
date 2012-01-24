using System;
using System.Collections.Generic;
using System.Text;
using SFDCNetConnector.sforce;

namespace SFDCNetConnector
{
    public class AccountProvider
    {
        public static List<sObject> retrieve10Accounts(SforceServiceWrapper sforceService)
        {
            return Helper.QuerysObjects(sforceService, "SELECT Id, Name FROM Account LIMIT 10");
        }
    }
}
