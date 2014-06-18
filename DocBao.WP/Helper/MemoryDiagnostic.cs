using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DocBao.WP.Helper
{
    public class MemoryDiagnostic
    {
        private static Timer _timer = null;
        private static double _last = 0;
        private static object lockObj = new object();

        public static void BeginRecording()
        {

            // start a timer to report memory conditions every 3 seconds 
            // 
            _timer = new Timer(state =>
            {
                string c = "unassigned";
                try
                {
                    //  
                }
                catch (ArgumentOutOfRangeException ar)
                {
                    var c1 = ar.Message;
                }
                catch
                {
                    c = "unassigned";
                }


                string report = "";
                //report += Environment.NewLine +
                //    "Current: " + (DeviceStatus.ApplicationCurrentMemoryUsage / 1000000).ToString() + "MB\n" +
                //    "Peak: " + (DeviceStatus.ApplicationPeakMemoryUsage / 1000000).ToString() + "MB\n" +
                //    "Memory Limit: " + (DeviceStatus.ApplicationMemoryUsageLimit / 1000000).ToString() + "MB\n\n" +
                //    "Device Total Memory: " + (DeviceStatus.DeviceTotalMemory / 1000000).ToString() + "MB\n" +
                //    "Working Limit: " + Convert.ToInt32((Convert.ToDouble(DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit")) / 1000000)).ToString() + "MB";

                double current = 0;
                double rate = 0;
                lock (lockObj)
                {
                    current = DeviceStatus.ApplicationCurrentMemoryUsage / 1000000;
                    rate = 0;
                    if (_last != 0)
                        rate = ((current - _last) / current) * 100;

                    _last = current;
                }


                report += Environment.NewLine +
                    "Current: " + current.ToString() + "MB\n" +
                    "Increased: " + rate.ToString() + "%\n" +
                    "Peak: " + (DeviceStatus.ApplicationPeakMemoryUsage / 1000000).ToString() + "MB\n" +
                    "Memory Limit: " + (DeviceStatus.ApplicationMemoryUsageLimit / 1000000).ToString() + "MB\n" +
                    "Working Limit: " + Convert.ToInt32((Convert.ToDouble(DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit")) / 1000000)).ToString() + "MB";

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Debug.WriteLine(report);
                });

            },
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(5));
        }
    
    }
}
