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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gw2map;

namespace gw2mapClient.Controller
{
    public class Gw2Controller : AController, IDisposable
    {
        public enum RecordingState
        {
            Stopped, Started, Recording
        }

        private Gw2Location location;
        private List<IStateListener> stateSubscribers;
        private RecordingState state;

        public RecordingState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                stateSubscribers.ForEach(x => x.NotifyState(value));

            }
        }
        public long Channel { get; set; }
        public string BaseUrl { get; set; }

        public Gw2Controller()
        {
            this.BaseUrl = settings.BaseUrl;
            this.stateSubscribers = new List<IStateListener>();
            location = new Gw2Location();
             this.State = RecordingState.Stopped;
        }


        public void Start(string channel)
        {
            this.Channel = long.Parse(channel);
            this.Start();
        }

        public void Start()
        {
            if (this.Channel == 0 || this.BaseUrl == null)
                return;

            try
            {
                location.Channel = this.Channel;
                location.BaseUrl = this.BaseUrl;
                location.MapStateChanged += Gw2StateChanged;
                location.Start();
                this.State = RecordingState.Started;
                var msg = "Gw2Map Client Started";
                NotifyMessage(msg);
                
            }
            catch (Exception e)
            {
                var msg = "Couldn't Start Gw2Client" + e.Message;
                NotifyMessage(msg);
            }

        }

        private void Gw2StateChanged(object sender, Gw2MapStateChangedEventArgs e)
        {
            if (e.MapState == Gw2MapState.Attached)
            {
                State = RecordingState.Recording;
                NotifyMessage("Started capturing data");
            }
            else
            {
                State = RecordingState.Started;
                NotifyMessage("Stopped capturing data");
            }
        }

        public void Stop()
        {
            if (this.State != RecordingState.Stopped)
            {
                location.Stop();
                this.State = RecordingState.Stopped;
                var msg = "Gw2Map Client Stopped";
                NotifyMessage(msg);
            }
        }

        public void SubscribeStates(IStateListener listener)
        {
            this.stateSubscribers.Add(listener);
        }

        private void Gw2MessageArrived(object sender, Gw2ServerMessageEventArgs e)
        {
            if(Boolean.Parse(appSettings.Get("debugModeEnabled")))
                NotifyMessage(e.Message);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
