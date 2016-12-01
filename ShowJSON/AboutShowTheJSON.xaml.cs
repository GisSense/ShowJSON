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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShowJSON
{
    /// <summary>
    /// Interaction logic for AboutShowTheJSON.xaml
    /// </summary>
    public partial class AboutShowTheJSON : Window
    {
        public AboutShowTheJSON()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rtbAbout.Document.Blocks.Clear();
            rtbAbout.AppendText(Properties.Resources.AboutTxt);
            rtbAbout.AppendText("\r\n");

            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assName = assembly.GetName();
            object[] attribs = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            String company = "";
            String copyright = "";
            if (attribs.Length > 0)
            {
                company = ((AssemblyCompanyAttribute)attribs[0]).Company;
            }
            attribs = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
            if (attribs.Length > 0)
            {
                copyright = ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
            }
            rtbAbout.AppendText("Company: " + company + "\r");
            rtbAbout.AppendText("Assembly: " + assName.Name + "\r");
            rtbAbout.AppendText("Version: " + assName.Version.ToString() + "\r");
            rtbAbout.AppendText("Copyright: " + "\r" + copyright + "\r");

        }

        private void image1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //open website
            System.Diagnostics.Process.Start("http://www.gissense.com");
        }
    }
}
