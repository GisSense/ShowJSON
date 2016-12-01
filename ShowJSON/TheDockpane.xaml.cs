/*
    Copyright GisSense 2016
    This file is part of ShowJSON.

    ShowJSON is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ShowJSON is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with ShowJSON.If not, see<http://www.gnu.org/licenses/>.
*/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShowJSON
{
    /// <summary>
    /// Interaction logic for TheDockpaneView.xaml
    /// </summary>
    public partial class TheDockpaneView : UserControl
    {
        DispatcherTimer timer = new DispatcherTimer();

        public TheDockpaneView()
        {
            InitializeComponent();
            timer.Tick += new EventHandler(ClearMessageEventHandler);
        }

        private void btnClearText_Click(object sender, RoutedEventArgs e)
        {
            this.txbJSON.Clear();
            this.txbJSON.Focus();
            ShowMessage("Text cleared", Brushes.Blue, 1);
        }

        private void btnClearGeometries_Click(object sender, RoutedEventArgs e)
        {
            if (ShowPro.ClearGraphics())
            {
                this.txbJSON.Focus();
                ShowMessage("Geometries cleared", Brushes.Blue, 1);
            }
        }

        private async void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush clr = Brushes.Blue;
            JsonHelper jsonHelper = null;
            string message = "";

            try
            {
                jsonHelper = new JsonHelper(this.txbJSON.Text, ShowPro.Current);
                bool success = await jsonHelper.ShowGraphics();
                message = jsonHelper.Message;
                if (!success)
                {
                    clr = Brushes.Red;
                }
            }
            catch
            {
                ShowPro.ClearGraphics();
                message = "Technical error; check your JSON!";
                clr = Brushes.Red;
            }
            finally
            {
                ShowMessage(message, clr, 3);
            }

        }

        private void ShowMessage(string message, Brush br, double seconds)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }

            lblMessage.Content = message;
            lblMessage.Foreground = br;

            //The message will disappear after x seconds.
            timer.Interval = TimeSpan.FromSeconds(seconds);
            timer.Start();
        }

        private void ClearMessageEventHandler(object sender, EventArgs e)
        {
            lblMessage.Content = string.Empty;
            (sender as DispatcherTimer).Stop();
            lblMessage.Foreground = Brushes.Blue;
        }

    }
}
