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
