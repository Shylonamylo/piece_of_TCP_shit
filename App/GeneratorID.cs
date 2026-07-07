using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public static class GeneratorID
    {
        public static int ClientID = 0;
        public static int MessageID = 0;
        public static int GetClientID()
        {
            return ++ClientID;
        }
        public static int GetMessageID()
        {
            return ++MessageID;
        }
    }
}
