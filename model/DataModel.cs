using SchneidMaschine.pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchneidMaschine.model
{
    public class DataModel
    {
        private MainWindow mainWindow;
        private Home home;
        private Streifen40 streifen40;
        private Streifen70 streifen70;

        public DataModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            init();
        }

        private void init()
        {
            this.home = new Home(this);
            this.streifen40 = new Streifen40(this);
            this.streifen70 = new Streifen70(this);
        }

        // Getter
        public MainWindow MainWindow { get { return mainWindow; } }
        public Home Home { get { return home; } }
        public Streifen40 Streifen40 { get { return streifen40; } }
        public Streifen70 Streifen70 { get { return streifen70; } }
    }
}
