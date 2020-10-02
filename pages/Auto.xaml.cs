﻿using SchneidMaschine.model;
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

namespace SchneidMaschine.pages
{
    /// <summary>
    /// Interaktionslogik für Auto.xaml
    /// </summary>
    public partial class Auto : Page
    {
        private DataModel dataModel;

        public Auto(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
        }

        private void BtnClickHome(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.Home;
        }

        private void BtnClickSchnittModus(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void BtnClickModusAutoStart(object sender, RoutedEventArgs e)
        {

        }

        private void BtnClickModusAutoStop(object sender, RoutedEventArgs e)
        {

        }
    }
}
