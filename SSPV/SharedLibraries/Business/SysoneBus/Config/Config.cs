using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;

namespace sysoneBus
{
    public static class Config
    {

        public static string GetResourceURL()
        {
            return "BUS_ResourceURL".FromAppSettings<string>(notFoundException: true);
        }
        public static string GetApplicationID()
        {
            return "BUS_ApplicationID".FromAppSettings<string>(notFoundException:true);
        }
        public static string GetChannelID()
        {
            return "BUS_ChannelID".FromAppSettings<string>(notFoundException: true);
        }
        public static string GetUser()
        {
            return "BUS_User".FromAppSettings<string>(notFoundException: true);
        }
        public static string GetPassword()
        {
            return "BUS_Password".FromAppSettings<string>(notFoundException: true);
        }
        public static string GetAuthAgent()
        {
            return "BUS_AuthAgent".FromAppSettings<string>(notFoundException: true);
        }
        public static int GetTimeOut()
        {
            return "BUS_TimeOut".FromAppSettings<int>(notFoundException: true);
        }
    

    }
}
