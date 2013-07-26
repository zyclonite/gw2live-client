/*
 * gw2live - GuildWars 2 Map Client
 * 
 * Website: http://gw2map.com
 *
 * Copyright 2013   zyclonite    networx
 *                  http://zyclonite.net
 * Developer: Manuel Bauer
 */
using gw2mapClient.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gw2mapClient
{
    public class Runtime : IDisposable
    {
        private static Runtime instance;
        private Settings settings = new Settings();

        public  static Runtime GetInstance()
        {
            if (instance == null)
                instance = new Runtime();
            return instance;
        }

        public Gw2Controller gw2Controller;
        public ChannelController channelController;
        public Gw2LaunchController regController;

        public Runtime()
        {
            gw2Controller = new Gw2Controller();
            channelController = new ChannelController();
            regController = new Gw2LaunchController();

            if (!CheckVersion())
            {
                System.Windows.Forms.MessageBox.Show("Your Client is outdated, check http://gw2map.com/download for updates!");
                Application.Current.Shutdown();
            }
        }

        public void Dispose()
        {
            gw2Controller.Dispose();
        }

        public bool CheckVersion()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(settings.BaseUrl + "/download.php?checkversion=1");
            httpWebRequest.Method = WebRequestMethods.Http.Get;

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result.Equals("1.0.0.0");
            }
        }
    }
}
