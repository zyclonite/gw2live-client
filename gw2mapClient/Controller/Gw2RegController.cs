using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gw2mapClient.Controller
{
    public class Gw2RegController : AController
    {
        public string GetGw2GamePath()
        {
            string result = null;
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey sk1 = rk.OpenSubKey(appSettings.Get("gw2RegHive"));
            
            if (sk1 != null)
            {
                try
                {
                    result = (string)sk1.GetValue(appSettings.Get("gw2RegHive"));
                }
                catch (Exception e)
                {
                    logger.Error("unable to read registry for Gw2 path:" + e.Message);
                }
            }

            return result;
        }
    }
}
