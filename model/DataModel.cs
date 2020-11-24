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
using System.Data.SqlClient;
using System.Data;
using SchneidMaschine.db;
using System.Globalization;
using System.Windows.Threading;
using System.Windows;
using System.IO;

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
        kopfSchnittStart,
        kopfSchnittBeendet,
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
        private double stepToMillimeter = 14.2; // wieviel Steps sind 1mm

        private long c4_40_schachtel_kurz = 6;       // Anzahl streifen pro Schachtel
        private long c4_40_schachtel_lang = 2;       // Anzahl streifen pro Schachtel
        private long c4_70_schachtel_Deckel = 1;     // Anzahl streifen pro Schachtel
        private long c5_40_deckel = 2;               // Anzahl streifen pro Schachtel

        private long streifenProSchnitt40er = 24;    // wieviel Streifen kommen raus pro Schnitt
        private long streifenProSchnitt70er = 14;    // wieviel Streifen kommen raus pro Schnitt

        private DBHandler dbHandler;

        private Statistik stats;

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

        private STREIFEN selectedStreifen;
        private long streifenProSchachtel;
        private int selectedLength;         // Streifen-Länge Sollwert in mm      
        private long istWertInMM;           // Streifen-Länge Istwert in mm

        //private string rollenLaenge;
        //private string rollenLaengeAktuell;

        private bool isCutFinished = true; // wenn der Motor zum Abschneiden steht, dann -> true
        private bool isStepperFinished = true;

        private delegate void SetTextCallback(string text);
        private string InputData = String.Empty;
        

        public DataModel(MainWindow mainWindow)
        {
            initDB();          
            this.mainWindow = mainWindow;
            init();
            initStats();
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

        private void initDB()
        {
            this.dbHandler = new DBHandler(this);
            //this.rollenLaenge = dbHandler.GetRollenLaenge();
            //this.rollenLaengeAktuell = dbHandler.GetRollenLaengeAktuell();

            //dbHandler.CreateTable();

            //dbHandler.InsertTest();
            //Console.WriteLine("SelectTest");
            //dbHandler.SelectTest();
            //Console.WriteLine("UpdateTest");
            //dbHandler.UpdateTest(70);
            //Console.WriteLine("SelectTest");
            //dbHandler.SelectTest();
        }

        private void initStats()
        {
            this.stats = new Statistik(this);

            stats.ChangedStreifenLaengeIst += () => {
                einzelSchritt.streifenIstWert.Text = stats.StreifenLaengeIst + "";
                halbAuto.streifenIstWert.Text = stats.StreifenLaengeIst + "";
                auto.streifenIstWert.Text = stats.StreifenLaengeIst + "";
            };

            // Heute-Total
            stats.ChangedHeuteStreifen40erKurz += () => {
                long wert = stats.HeuteStreifen40erKurz;
                mainWindow.HeuteStreifen40erKurz.Text = wert + "";
                mainWindow.HeuteSchachtel40erKurz.Text = wert / StreifenProSchachtel_C4_40_Kurz + "";
            };

            stats.ChangedHeuteStreifen40erLang += () => {
                long wert = stats.HeuteStreifen40erLang;
                mainWindow.HeuteStreifen40erLang.Text = wert + "";
                mainWindow.HeuteSchachtel40erLang.Text = wert / StreifenProSchachtel_C4_40_Lang + "";
            };

            stats.ChangedHeuteStreifen70erDeckel += () => {
                long wert = stats.HeuteStreifen70erDeckel;
                mainWindow.HeuteStreifen70erDeckel.Text = wert + "";
                mainWindow.HeuteSchachtel70erDeckel.Text = wert / StreifenProSchachtel_C4_70_Deckel + "";
            };

            stats.ChangedHeuteRolleAbgewickelt += () => {
                long wert = stats.HeuteRolleAbgewickelt;
                mainWindow.HeuteRolleAbgewickelt.Text = wert + "";
            };

            // Rollen-Total
            stats.ChangedRolleStreifen40erKurz += () => {
                long wert = stats.RolleStreifen40erKurz;
                mainWindow.RolleStreifen40erKurz.Text = wert + "";
                mainWindow.RolleSchachtel40erKurz.Text = wert / StreifenProSchachtel_C4_40_Kurz + "";
            };
            stats.ChangedRolleStreifen40erLang += () => {
                long wert = stats.RolleStreifen40erLang;
                mainWindow.RolleStreifen40erLang.Text = wert + "";
                mainWindow.RolleSchachtel40erLang.Text = wert / StreifenProSchachtel_C4_40_Lang + "";
            };
            stats.ChangedRolleStreifen70erDeckel += () => {
                long wert = stats.RolleStreifen70erDeckel;
                mainWindow.RolleStreifen70erDeckel.Text = wert + "";
                mainWindow.RolleSchachtel70erDeckel.Text = wert / StreifenProSchachtel_C4_70_Deckel + "";
            };
            stats.ChangedRolleIstLaenge += () => { mainWindow.RolleIstLaenge.Text = stats.RolleIstLaenge + ""; };
            //stats.ChangedRolleIstLaenge += () => { auto.RestLaengeRolle.Text = stats.RolleIstLaenge + ""; };
            stats.ChangedRolleLaengeGesamt += () => { mainWindow.RolleTotal.Text = stats.RolleLaengeGesamt + ""; };

            // Langzeit-Total
            stats.ChangedLangzeitStreifen40erKurz += () => {
                long wert = stats.LangzeitStreifen40erKurz;
                mainWindow.LangzeitStreifen40erKurz.Text = wert + "";
                mainWindow.LangzeitSchachtel40erKurz.Text = wert / StreifenProSchachtel_C4_40_Kurz + "";
            };
            stats.ChangedLangzeitStreifen40erLang += () => {
                long wert = stats.LangzeitStreifen40erLang;
                mainWindow.LangzeitStreifen40erLang.Text = wert + "";
                mainWindow.LangzeitSchachtel40erLang.Text = wert / StreifenProSchachtel_C4_40_Lang + "";
            };
            stats.ChangedLangzeitStreifen70erDeckel += () => {
                long wert = stats.LangzeitStreifen70erDeckel;
                mainWindow.LangzeitStreifen70erDeckel.Text = stats.LangzeitStreifen70erDeckel + "";
                mainWindow.LangzeitSchachtel70erDeckel.Text = wert / StreifenProSchachtel_C4_70_Deckel + "";
            };
            stats.ChangedLangzeitVerbrauchteRollen += () => { mainWindow.LangzeitVerbrauchteRollen.Text = stats.LangzeitVerbrauchteRollen + ""; };

            // wenn die Daten aus der DB nicht vom selben Tag sind, dann reset
            if (dbHandler.isResetHeute())
            {
                dbHandler.resetHeute();
            }

            initStatsHeute();
            initStatsRolle();
            initStatsLangzeit();
        }

        public void initStatsHeute() {
            // Heute-Total
            stats.HeuteStreifen40erKurz = dbHandler.GetStreifen40erKurzHeute();
            stats.HeuteStreifen40erLang = dbHandler.GetStreifen40erLangHeute();
            stats.HeuteStreifen70erDeckel = dbHandler.GetStreifen70erDeckelHeute();
            stats.HeuteRolleAbgewickelt = dbHandler.GetRolleAbgewickeltHeute();
        }
    
        public void initStatsRolle()
        {
            // Rollen-Total
            stats.RolleStreifen40erKurz = dbHandler.GetStreifen40erKurzRolle();
            stats.RolleStreifen40erLang = dbHandler.GetStreifen40erLangRolle();
            stats.RolleStreifen70erDeckel = dbHandler.GetStreifen70erDeckelRolle();
            stats.RolleIstLaenge = dbHandler.GetRollenLaengeAktuell();
            stats.RolleLaengeGesamt = dbHandler.GetRollenLaengeGesamt();
        }

        public void initStatsLangzeit()
        {
            // Langzeit-Total
            stats.LangzeitStreifen40erKurz = dbHandler.GetStreifen40erKurzLangzeit();
            stats.LangzeitStreifen40erLang = dbHandler.GetStreifen40erLangLangzeit();
            stats.LangzeitStreifen70erDeckel = dbHandler.GetStreifen70erDeckelLangzeit();
            stats.LangzeitVerbrauchteRollen = dbHandler.GetVerbrauchteRollenLangzeit();
        }

        public void sendText(string text)
        {
            try
            {
                serialPort1.Write(text + "#");
            }
            catch (Exception e)
            {              
                Console.WriteLine("IOException  source: {0}", e.Source);
                MessageBox.Show("Der Serial-Port zum Arduino wurde nicht geöffnet.\n\nDas Programm muss neu gestartet werden.", "Serial-Port Error");
            }           
        }

        
        public int mmToSteps(double mm)
        {
            double result = mm * stepToMillimeter;
            return Convert.ToInt32(Math.Round(result));
        }

        public long stepsToMM(double steps)
        {
            double result = steps / stepToMillimeter;
            return Convert.ToInt64(Math.Round(result));
        }

        // Getter
        public DBHandler DBHandler { get { return dbHandler; } }
        public Statistik Statistik { get { return stats; } }

        public MainWindow MainWindow { get { return mainWindow; } }
        public Home Home { get { return home; } }
        public SchnittModus SchnittModus { get { return schnittModus; } }
        public EinzelSchritt EinzelSchritt { get { return einzelSchritt; } }
        public HalbAuto HalbAuto { get { return halbAuto; } }
        public Auto Auto { get { return auto; } }

        public CommandLine CommandLine { get { return commandLine; } }

        public MyThreads MyThreads { get { return myThreads; } }

        public string[] PortList { get { return portList; } }

        public long StreifenProSchachtel { get { return streifenProSchachtel; } }

        public long StreifenProSchachtel_C4_40_Kurz { get { return c4_40_schachtel_kurz; } }
        public long StreifenProSchachtel_C4_40_Lang { get { return c4_40_schachtel_lang; } }
        public long StreifenProSchachtel_C4_70_Deckel { get { return c4_70_schachtel_Deckel; } }
        public long StreifenProSchachtel_C5_40_Deckel { get { return c5_40_deckel; } }

        public long StreifenProSchnitt40er { get { return streifenProSchnitt40er; } }
        public long StreifenProSchnitt70er { get { return streifenProSchnitt70er; } }

        public SerialPort SerialPort1 { get => serialPort1; set => serialPort1 = value; }

        //public string RollenLaenge { get => rollenLaenge; set => rollenLaenge = value; }
        //public string RollenLaengeAktuell { get => rollenLaengeAktuell; set => rollenLaengeAktuell = value; }

        

        public bool IsCutFinished { get => isCutFinished; set => isCutFinished = value; }
        public bool IsStepperFinished { get => isStepperFinished; set => isStepperFinished = value; }

        public STREIFEN SelectedStreifen { get => selectedStreifen; 
            set 
            { 
                selectedStreifen = value;

                if (selectedStreifen.Equals(STREIFEN.C4_40_Schachtel_KURZ)) streifenProSchachtel = c4_40_schachtel_kurz;
                if (selectedStreifen.Equals(STREIFEN.C4_40_Schachtel_LANG)) streifenProSchachtel = c4_40_schachtel_lang;
                if (selectedStreifen.Equals(STREIFEN.C4_70_Deckel)) streifenProSchachtel = c4_70_schachtel_Deckel;
                if (selectedStreifen.Equals(STREIFEN.C5_40_Deckel)) streifenProSchachtel = c5_40_deckel;
            } 
        }

        public void setSelectedLength(STREIFEN wert)
        {
            this.SelectedStreifen = wert;
            SelectedLength = (int) wert;
            auto.SetMaxDurchlauf();
            auto.output();
        }

        public void setSelectedLength(string wert)
        {
            SelectedLength = Int32.Parse(wert);
            auto.SetMaxDurchlauf();
            auto.output();
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
            Console.WriteLine("setIstWert vor -> " + wert);
            IstWertInMM = Int64.Parse(wert);
            Console.WriteLine("setIstWert nach -> " + IstWertInMM);
        }

        public long IstWertInMM
        {
            get => istWertInMM;
            set
            {
                istWertInMM = stepsToMM(value);

                stats.StreifenLaengeIst = istWertInMM;

                //einzelSchritt.streifenIstWert.Text = istWertInMM.ToString();               

                //halbAuto.streifenIstWert.Text = istWertInMM.ToString();

                //auto.streifenIstWert.Text = istWertInMM.ToString();
            }
        }
    }

}
