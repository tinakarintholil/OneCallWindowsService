using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;




namespace OneCallWindowsService
{
    public static class VGlobal
    {
       
        public static string Mode = ConfigurationManager.AppSettings["Mode"].ToString().ToUpper();
        public static string devResponseFolderPath= ConfigurationManager.AppSettings["responseDevFolderPath"].ToString();
        public static string ProdResponseFolderPath = ConfigurationManager.AppSettings["responseProdFolderPath"].ToString();


        public static string GetResponseFolderPath()
        {
            if (Mode=="PROD")
            {

                return ProdResponseFolderPath;

            }
            else
            {

                return devResponseFolderPath;
            }

        }

        public static string Before(this string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        }


    }
}