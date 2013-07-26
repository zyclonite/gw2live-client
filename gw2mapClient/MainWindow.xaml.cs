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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gw2mapClient.Controller;

namespace gw2mapClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessenger, IStateListener, IDisposable
    {
        private Runtime application;
        System.Windows.Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            application = Runtime.GetInstance();

            application.gw2Controller.SubscribeToServerMessages(this);
            application.gw2Controller.SubscribeStates(this);
            LoadChannels();
            GenerateNotify();

        }

        public void DispatchMessage(string message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                tb_messageBox.Text = (message + "\n" + tb_messageBox.Text);
            }));
 
        }

        private void LoadChannels()
        {

            var items = application.channelController.GetChannelHistory();

            cb_channelBox.Items.Clear();
            
            items.ForEach(x =>
            {
                cb_channelBox.Items.Add(x);
            });
            cb_channelBox.Items.Add(bn_clearHistory);
        }

        private void StartGw2Client()
        {
            var channel = cb_channelBox.Text;
            if (application.channelController.CheckChannel(channel))
            {
                if (!application.channelController.GetChannelHistory().Contains(channel))
                {
                    application.channelController.AppendChannel(channel);
                    LoadChannels();
                }

            }

            application.gw2Controller.Start(channel);
            
            if (cx_launchGw2.IsChecked.Value)
            {
                application.regController.LaunchGame();
            }
        }

        private void bn_startClient_Click(object sender, RoutedEventArgs e)
        {
            if (application.gw2Controller.State == Gw2Controller.RecordingState.Stopped)
            {
                StartGw2Client();
            }
            else
            {
                application.gw2Controller.Stop();
            }
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void bn_close_Click(object sender, RoutedEventArgs e)
        {
            this.CleanUp();
            Application.Current.Shutdown();
        }

        public void NotifyState(Gw2Controller.RecordingState state)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                
                bn_startClient.Content = state == Gw2Controller.RecordingState.Stopped ? "Start" : "Stop";

                if (state == Gw2Controller.RecordingState.Stopped)
                {
                    img_greenstatus.Visibility = System.Windows.Visibility.Hidden;
                    img_yellowstatus.Visibility = System.Windows.Visibility.Hidden;
                    img_redstatus.Visibility = System.Windows.Visibility.Visible;
                    cb_channelBox.IsEnabled = true;
                    notifyIcon.BalloonTipText = "stopped";
                }
                else if (state == Gw2Controller.RecordingState.Started)
                {
                    img_greenstatus.Visibility = System.Windows.Visibility.Hidden;
                    img_yellowstatus.Visibility = System.Windows.Visibility.Visible;
                    img_redstatus.Visibility = System.Windows.Visibility.Hidden;
                    cb_channelBox.IsEnabled = false;
                    notifyIcon.BalloonTipText = "waiting for data";
                }
                else
                {
                    img_greenstatus.Visibility = System.Windows.Visibility.Visible;
                    img_yellowstatus.Visibility = System.Windows.Visibility.Hidden;
                    img_redstatus.Visibility = System.Windows.Visibility.Hidden;
                    notifyIcon.BalloonTipText = "receiving data";
                }

                notifyIcon.ShowBalloonTip(1000);

            }));

        }

        private void bn_clearHistory_Click(object sender, RoutedEventArgs e)
        {
            application.channelController.ClearChannelHistory();
            LoadChannels();
        }

        private void cb_channelBox_TextInput(object sender, TextChangedEventArgs e)
        {
            validateCheckbox();
        }

        private void cb_channelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            validateCheckbox();
        }

        private void validateCheckbox()
        {
            if (application.channelController.CheckChannel(cb_channelBox.Text))
            {
                cb_channelBox.Foreground = System.Windows.Media.Brushes.Black;
                bn_startClient.IsEnabled = true;
            }
            else
            {
                cb_channelBox.Foreground = System.Windows.Media.Brushes.Red;
                bn_startClient.IsEnabled = false;
            }
        }

        private void bn_minify_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.WindowState = System.Windows.WindowState.Minimized;
        }

        private void GenerateNotify()
        {
            notifyIcon  = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "gw2map.com";
            notifyIcon.Icon = new System.Drawing.Icon("icon.ico");
            notifyIcon.Visible = true;

            notifyIcon.Click += notifyIcon_Click;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(1000);
        }

        private void CleanUp()
        {
            application.Dispose();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
 
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            CleanUp();
        }

        public void Dispose()
        {
            CleanUp();
        }
    }
}
