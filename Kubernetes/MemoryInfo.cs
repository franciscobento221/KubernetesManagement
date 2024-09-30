using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Kubernetes
{
    internal class MemoryInfo
    {

        public ulong TotalPhysicalMemory { get; }

        public MemoryInfo()
        {
            TotalPhysicalMemory = GetTotalPhysicalMemory();
        }

        private ulong GetTotalPhysicalMemory()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    return Convert.ToUInt64(queryObj["TotalPhysicalMemory"]);
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying WMI: " + e.Message);
            }
            return 0;
        }



    }
}
