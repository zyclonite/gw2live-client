/*
 * gw2live - GuildWars 2 Location Library
 * 
 * Website: http://gw2map.com
 *
 * Copyright 2013   zyclonite    networx
 *                  http://zyclonite.net
 * Developer: Manuel Bauer
 */
using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;

namespace gw2map
{
    public delegate void ServerMessageEventHandler(object sender, Gw2ServerMessageEventArgs e);
    
    public class Gw2Location
    {
        public event ServerMessageEventHandler MessageArrived;
        public event StateChangedEventHandler MapStateChanged;

        public Int64 Channel { get; set; }
        public String BaseUrl { get; set; }

        private  Gw2Map map;
		private  Guid guid;
		private  int currentmap;
		private  dynamic mapcache;

        public Gw2Location()
        { 			
            guid = Guid.NewGuid ();
			map = new Gw2Map();
        }

        public Gw2Location(Int64 Channel, string BaseUrl)
            : this()
        {
            this.BaseUrl = BaseUrl;
            this.Channel = Channel;
        }
		
        public void Start ()
		{

            map.Changed += new ChangedEventHandler(OnMapChanged);
            map.StateChanged += new StateChangedEventHandler(OnMapStateChanged);
		}

        private void OnMapStateChanged(object sender, Gw2MapStateChangedEventArgs e)
        {
            if (MapStateChanged != null)
                MapStateChanged(this, e);
        }

        public void Stop()
        {
            map.Changed -= OnMapChanged;
            map.StateChanged -= OnMapStateChanged;
            map.Dispose();
        }

		private void OnMapChanged(object source, Gw2MapEventArgs e)
     	{
			var coords = e.Coordinates;
			if(coords.identity.Length > 0) {
				if(coords.mapId != currentmap){
					currentmap = coords.mapId;
					CacheMaps();
				}

				postData(MapToContinentCoords (coords));
			}
     	}

        protected virtual void OnServerMessage(Gw2ServerMessageEventArgs e)
        {
            if (MessageArrived != null)
                MessageArrived(this, e);
        }

		private Gw2Coordinates MapToContinentCoords(Gw2Coordinates coords){
            if (mapcache == null)
                return null;
            var tmp = (Gw2Coordinates)coords.Clone();
			tmp.x = (coords.x - (int)mapcache.map_rect [0] [0]) / ((int)mapcache.map_rect [1] [0] - (int)mapcache.map_rect [0] [0]) * ((int)mapcache.continent_rect [1] [0] - (int)mapcache.continent_rect [0] [0]) + (int)mapcache.continent_rect [0] [0];
			tmp.y = (-coords.y - (int)mapcache.map_rect [0] [1]) / ((int)mapcache.map_rect [1] [1] - (int)mapcache.map_rect [0] [1]) * ((int)mapcache.continent_rect [1] [1] - (int)mapcache.continent_rect [0] [1]) + (int)mapcache.continent_rect [0] [1];
			return tmp;
		}

		private void CacheMaps(){
			var httpWebRequest = (HttpWebRequest)WebRequest.Create (this.BaseUrl + "/rest/maps/map/" + currentmap);
			httpWebRequest.ContentType = "application/json; charset=utf-8";
			httpWebRequest.Accept = "application/json";
			httpWebRequest.Method = WebRequestMethods.Http.Get;
			var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var text = streamReader.ReadToEnd();
				var result = DynamicJson.Parse(text);
				foreach(dynamic map in result){
					if(int.Parse(map.map_id) == currentmap){
						mapcache = map;
						return;
					}
				}
			}
		}

		private void postData(Gw2Coordinates coords)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.BaseUrl + "/rest/playerlocation");
			httpWebRequest.ContentType = "application/json; charset=utf-8";
			httpWebRequest.Accept = "application/json";
			httpWebRequest.Method = WebRequestMethods.Http.Post;

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				string json = "{\"channel\":\"" + this.Channel.ToString() + 
                    "\",\"id\":\"" + guid.ToString() +
                    "\",\"identity\":\"" + coords.identity + 
                    "\",\"world_id\":" + coords.worldId + 
                    ",\"map_id\":" + coords.mapId + 
                    ",\"x\":" + coords.x.ToString(CultureInfo.InvariantCulture) + 
                    ",\"y\":"+coords.y.ToString(CultureInfo.InvariantCulture) +
                    ",\"z\":"+coords.z.ToString(CultureInfo.InvariantCulture) +
                    "}";
				streamWriter.Write(json);
				streamWriter.Flush();
				streamWriter.Close();
			}

			var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
			    var result = streamReader.ReadToEnd();
				OnServerMessage(new Gw2ServerMessageEventArgs(result));
			}
		}

        public void Dispose()
        {
            if (map != null)
                map.Dispose();
        }
	}

}
