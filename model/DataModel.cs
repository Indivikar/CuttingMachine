using SchneidMaschine.pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;
using System.Threading;
using SchneidMaschine.threads;

namespace SchneidMaschine.model
{
    public class DataModel
    {
       
        private MainWindow mainWindow;
        private Home home;
        private SchnittModus schnittModus;
        private EinzelSchritt einzelSchritt;
        private HalbAuto halbAuto;
        private Auto auto;
        private string[] portList;
        private SerialPort serialPort1;

        private MyThreads myThreads;

        delegate void SetTextCallback(string text);
        string InputData = String.Empty;

        public DataModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            init();
        }

        private void init()
        {
            this.serialPort1 = new SerialPort();
            this.portList = SerialPort.GetPortNames();

            this.home = new Home(this);
            this.schnittModus = new SchnittModus(this);
            this.einzelSchritt = new EinzelSchritt(this);
            this.halbAuto = new HalbAuto(this);
            this.auto = new Auto(this);

            this.myThreads = new MyThreads(this);
 

        }

       
        private void SetText(string text)
        {
            //this.rtbIncoming.Text += text;
        }

        // Getter
        public MainWindow MainWindow { get { return mainWindow; } }
        public Home Home { get { return home; } }
        public SchnittModus SchnittModus { get { return schnittModus; } }
        public EinzelSchritt EinzelSchritt { get { return einzelSchritt; } }
        public HalbAuto HalbAuto { get { return halbAuto; } }
        public Auto Auto { get { return auto; } }

        public MyThreads MyThreads { get { return myThreads; } }

        public string[] PortList { get { return portList; } }

        public SerialPort SerialPort1 { get => serialPort1; set => serialPort1 = value; }
    }
}
