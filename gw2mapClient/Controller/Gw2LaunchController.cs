/*
 * gw2live - GuildWars 2 Map Client
 * 
 * Website: http://gw2map.com
 *
 * Copyright 2013   zyclonite    networx
 *                  http://zyclonite.net
 * Developer: Manuel Bauer
 */
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gw2mapClient.Controller
{
    public class Gw2LaunchController : AController
    {
        public string GetGw2GamePath()
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(settings.Gw2RegHive))
            {
                return (string)registryKey.GetValue(settings.gw2RegKey);
            }

        }

        public void LaunchGame()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(GetGw2GamePath());
                Process.Start(psi);
            }
            catch (Exception e)
            {
                NotifyMessage("Couldn't start Guild Wars 2: " + e.Message);
            }

        }
    }
}
