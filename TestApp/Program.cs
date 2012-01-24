using System;
using System.Collections.Generic;
using System.Configuration;
using SFDCNetConnector;
using SFDCNetConnector.sforce;

namespace SFDCNetConnector
{
    class Program
    {
        private static SforceServiceWrapper sforceService;

        static void Main(string[] args)
        {
            //Create Salesforce binging with username and password
            //sforceService = SalesforceSession.StartSession("user name", "password", "security token");
            sforceService = SalesforceSession.StartSession("ian.huang.syd@gmail.com", "scmy_209_XX_!", "KwgCnbIa0QRHEEpPvXV6DaZb");

            List<sObject> accounts = AccountProvider.retrieve10Accounts(sforceService);
            foreach (sObject account in accounts)
                Console.WriteLine(string.Format("{0} {1}", account.Id, account.Any[1].InnerText));

            Console.WriteLine("");
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}
