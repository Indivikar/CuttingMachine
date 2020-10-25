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

    public enum STREIFEN
    {
        C4_40_Schachtel_KURZ = 320,
        C4_40_Schachtel_LANG = 700,
        C4_70_Deckel = 650,
        C5_40_Deckel = 400,
    }

    public enum COMMAND
    {
        allesStop,
        allesGestoppt,
        Connected,
        steps,
        stepperStart,
        stepperStop,
        stepperFinished,
        handradOn,
        handradOff,
        schneidenStart,
        schneidenStartet,
        schneidenBeendet,
        autoStart,
        autoStop,
        resetIstWert
    }

    public enum DIRECTION
    {
        forward,
        backward
    }

    public partial class DataModel
    {
        // Config
        private double stepToMillimeter = 2.0; // wieviel Steps sind 1mm

        private MainWindow mainWindow;
        private Home home;
        private SchnittModus schnittModus;
        private EinzelSchritt einzelSchritt;
        private HalbAuto halbAuto;
        private Auto auto;      
        private string[] portList;
        private SerialPort serialPort1;

        private CommandLine commandLine;

        private MyThreads myThreads;

        private int selectedLength; // Streifen-Länge Sollwert in mm
        private int istWertInMM;    // Streifen-Länge Istwert in mm

        private bool isCutFinished = true; // wenn der Motor zum Abschneiden steht, dann -> true
        private bool isStepperFinished = true;

        private delegate void SetTextCallback(string text);
        private string InputData = String.Empty;
        

        public DataModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            init();
        }

        private void init()
        {
            this.serialPort1 = new SerialPort();
            this.portList = SerialPort.GetPortNames();
            this.commandLine = new CommandLine(this);

            this.home = new Home(this);
            this.schnittModus = new SchnittModus(this);
            this.einzelSchritt = new EinzelSchritt(this);
            this.halbAuto = new HalbAuto(this);
            this.auto = new Auto(this);

            this.myThreads = new MyThreads(this);
        }

       
        public void sendText(string text)
        {
            serialPort1.Write(text + "#");
        }

        
        public int mmToSteps(double mm)
        {
            double result = mm * stepToMillimeter;
            return Convert.ToInt32(Math.Round(result));
        }

        public int stepsToMM(double steps)
        {
            double result = steps / stepToMillimeter;
            return Convert.ToInt32(Math.Round(result));
        }

        // Getter
        public MainWindow MainWindow { get { return mainWindow; } }
        public Home Home { get { return home; } }
        public SchnittModus SchnittModus { get { return schnittModus; } }
        public EinzelSchritt EinzelSchritt { get { return einzelSchritt; } }
        public HalbAuto HalbAuto { get { return halbAuto; } }
        public Auto Auto { get { return auto; } }

        public CommandLine CommandLine { get { return commandLine; } }

        public MyThreads MyThreads { get { return myThreads; } }

        public string[] PortList { get { return portList; } }

        public SerialPort SerialPort1 { get => serialPort1; set => serialPort1 = value; }

        public bool IsCutFinished { get => isCutFinished; set => isCutFinished = value; }
        public bool IsStepperFinished { get => isStepperFinished; set => isStepperFinished = value; }

        public void setSelectedLength(STREIFEN wert)
        {
            SelectedLength = (int) wert;           
        }

        public void setSelectedLength(string wert)
        {
            SelectedLength = Int32.Parse(wert);
        }

        public int SelectedLength { get => selectedLength; 
            set  
            { 
                selectedLength = value;

                schnittModus.StreifenSollWert.Text = value.ToString();

                einzelSchritt.StreifenSollWert.Text = value.ToString();
                einzelSchritt.BtnSollwert.Content = value.ToString() + " mm";

                halbAuto.StreifenSollWert.Text = value.ToString();

                auto.StreifenSollWert.Text = value.ToString();
            }
        }

        public void setIstWert(string wert)
        {
            IstWertInMM = Int32.Parse(wert);
        }

        public int IstWertInMM
        {
            get => istWertInMM;
            set
            {
                istWertInMM = stepsToMM(value);

                einzelSchritt.streifenIstWert.Text = istWertInMM.ToString();               

                halbAuto.streifenIstWert.Text = istWertInMM.ToString();

                auto.streifenIstWert.Text = istWertInMM.ToString();
            }
        }
    }

}
