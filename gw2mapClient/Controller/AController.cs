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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gw2mapClient.Controller
{
    public abstract class AController
    {
        protected NameValueCollection appSettings = ConfigurationManager.AppSettings;
        protected Settings settings = new Settings();
        protected List<IMessenger> messageSubsribers = new List<IMessenger>();

        public virtual void SubscribeToServerMessages(IMessenger m)
        {
            messageSubsribers.Add(m);
        }

        protected virtual void NotifyMessage(string message)
        {
            messageSubsribers.ForEach(x => x.DispatchMessage(message));
        }

    }
}
