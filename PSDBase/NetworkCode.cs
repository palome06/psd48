using System.IO;

namespace PSD.Base
{
    public static class NetworkCode
    {
        public static int DIR_PORT;
        public static int HALL_PORT;

        static NetworkCode()
        {
            if (File.Exists("PSDDorm.AKB48Show!"))
            {
                DIR_PORT = 41201; HALL_PORT = 41421;
            }
            else
            {
                DIR_PORT = 40201; HALL_PORT = 40421;
            }
        }
    }
}
