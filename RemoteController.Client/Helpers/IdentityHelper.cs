using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RemoteController.Client.Helpers
{
    public class IdentityHelper
    {
        /// <summary>
        /// 取本机机器码
        /// </summary>
        public static string GetMachineCode()
        {
            string cpuInfo = GetMD5Value(GetCpuID() + typeof(string).ToString());
            if (cpuInfo.Equals("UnknowCpuInfo")) return null;
            string diskInfo = GetMD5Value(GetDiskID() + typeof(int).ToString());
            if (diskInfo.Equals("UnknowDiskInfo")) return null;
            string macInfo = GetMD5Value(GetMacByNetworkInterface() + typeof(double).ToString());
            if (macInfo.Equals("UnknowMacInfo")) return null;

            return GetNum(cpuInfo, 3) + GetNum(diskInfo, 3) + GetNum(macInfo, 3);
        }

        /// <summary>
        /// 取MD5
        /// </summary>
        /// <param name="value">要加密的字符串</param>
        public static string GetMD5Value(string value)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] targetData = md5.ComputeHash(Encoding.Unicode.GetBytes(value));
            string resString = null;
            for (int i = 0; i < targetData.Length; i++)
            {
                resString += targetData[i].ToString("x");
            }
            return resString;
        }

        /// <summary>
        /// 取数字
        /// </summary>
        /// <param name="md5"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetNum(string md5, int len)
        {
            Regex regex = new Regex(@"\d");
            MatchCollection listMatch = regex.Matches(md5);
            string str = "";
            for (int i = 0; i < len; i++)
            {
                str += listMatch[i].Value;
            }
            while (str.Length < len)
            {
                //不足补0
                str += "0";
            }
            return str;
        }

        /// <summary>
        /// 取CPU序列号
        /// </summary>
        public static string GetCpuID()
        {
            try
            {
                string cpuInfo = "";
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc.Dispose();
                mc.Dispose();
                return cpuInfo;
            }
            catch
            {
                return "UnknowCpuInfo";
            }
        }

        /// <summary>
        /// 取硬盘序列号
        /// </summary>
        public static string GetDiskID()
        {
            try
            {
                string HDid = "";
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                }
                moc.Dispose();
                mc.Dispose();
                return HDid;
            }
            catch
            {
                return "UnknowDiskInfo";
            }
        }

        /// <summary>
        /// 获取本机MAC地址
        /// </summary>
        public static string GetMacByNetworkInterface()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                }
                return "UnknowMacInfo";
            }
            catch (Exception)
            {
                return "UnknowMacInfo";
            }
        }

        static string str = @"0123456789ABCDEFGHIGKLMNOPQRSTUVWXYZ";
        static Random random = new Random();
        public static string GetMix()
        {
            return str.Substring(0 + random.Next(str.Length - 1), 1);
        }
    }
}
