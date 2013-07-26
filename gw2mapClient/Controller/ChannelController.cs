/*
 * gw2live - GuildWars 2 Map Client
 * 
 * Website: http://gw2map.com
 *
 * Copyright 2013   zyclonite    networx
 *                  http://zyclonite.net
 * Developer: Manuel Bauer
 */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace gw2mapClient.Controller
{
    public class ChannelController : AController
    {
        private string JSONPATH = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +  "\\channels.json");

        public List<String> GetChannelHistory()
        {
            List<String> result = new List<String>();
            try
            {
                using (StreamReader r = new StreamReader(JSONPATH))
                {
                    string json = r.ReadToEnd();
                    result = JsonConvert.DeserializeObject<List<String>>(json);
                    
                }
            }
            catch (Exception e)
            {
                NotifyMessage(e.Message);
            }

            return result;
        }

        public void AppendChannel(String channel)
        {
            if (!CheckChannel(channel))
                return;
            try
            {
                var previous = GetChannelHistory();
                previous.Add(channel);
                using (StreamWriter r = File.CreateText(JSONPATH))
                {
                    string json = JsonConvert.SerializeObject(previous);
                    r.WriteAsync(json.ToCharArray()).ContinueWith(y => NotifyMessage("Updated Channel History"));

                }
            }
            catch (Exception e)
            {
                NotifyMessage(e.Message);
            }

        }

        public void ClearChannelHistory()
        {
            try
            {
                using (StreamWriter r = File.CreateText(JSONPATH))
                {
                   r.WriteAsync("[]").ContinueWith(y => NotifyMessage("History Cleared"));
                }
            }
            catch (Exception)
            {
                NotifyMessage("couldn't clear history!");
            }

        }

        public bool CheckChannel(String channel)
        {
            long i;
            return channel.Length == 13 && Int64.TryParse(channel,out i);
        }
    }
}
 