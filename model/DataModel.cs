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
        private double stepToMillimeter = 300.0; // wieviel Steps sind 1mm

        private DBHandler dbHandler;

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
        private int streifenProSchachtel;
        private int selectedLength;         // Streifen-Länge Sollwert in mm      
        private long istWertInMM;           // Streifen-Länge Istwert in mm

        private string rollenLaenge;
        private string rollenLaengeAktuell;

        private bool isCutFinished = true; // wenn der Motor zum Abschneiden steht, dann -> true
        private bool isStepperFinished = true;

        private delegate void SetTextCallback(string text);
        private string InputData = String.Empty;
        

        public DataModel(MainWindow mainWindow)
        {
            initDB();
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

        private void initDB()
        {
            this.dbHandler = new DBHandler();
            this.rollenLaenge = dbHandler.GetRollenLaenge();
            this.rollenLaengeAktuell = dbHandler.GetRollenLaengeAktuell();

            //dbHandler.CreateTable();

            //dbHandler.InsertTest();
            //Console.WriteLine("SelectTest");
            //dbHandler.SelectTest();
            //Console.WriteLine("UpdateTest");
            //dbHandler.UpdateTest(70);
            //Console.WriteLine("SelectTest");
            //dbHandler.SelectTest();
        }

        public void SelectTest()
        {

            string sql = "SELECT * FROM [Table]";

            string cn_string = Properties.Settings.Default.DatabaseConnectionString;
            SqlConnection cn = new SqlConnection(cn_string);
            cn.Open();

            SqlCommand command = new SqlCommand(sql, cn);

            //SqlDataAdapter sql_Adapter = new SqlDataAdapter(sql, cn);

            //DataTable tblData = new DataTable();
            //sql_Adapter.Fill(tblData);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine("DB -> " + String.Format("{0}", reader["Email"]));
                }
            }

            cn.Close();

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

        public long stepsToMM(double steps)
        {
            double result = steps / stepToMillimeter;
            return Convert.ToInt64(Math.Round(result));
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

        public int StreifenProSchachtel { get { return streifenProSchachtel; } }

        public SerialPort SerialPort1 { get => serialPort1; set => serialPort1 = value; }

        public string RollenLaenge { get => rollenLaenge; set => rollenLaenge = value; }
        public string RollenLaengeAktuell { get => rollenLaengeAktuell; set => rollenLaengeAktuell = value; }

        

        public bool IsCutFinished { get => isCutFinished; set => isCutFinished = value; }
        public bool IsStepperFinished { get => isStepperFinished; set => isStepperFinished = value; }

        public STREIFEN SelectedStreifen { get => selectedStreifen; 
            set 
            { 
                selectedStreifen = value;

                if (selectedStreifen.Equals(STREIFEN.C4_40_Schachtel_KURZ)) streifenProSchachtel = 6;
                if (selectedStreifen.Equals(STREIFEN.C4_40_Schachtel_LANG)) streifenProSchachtel = 2;
                if (selectedStreifen.Equals(STREIFEN.C4_70_Deckel)) streifenProSchachtel = 1;
                if (selectedStreifen.Equals(STREIFEN.C5_40_Deckel)) streifenProSchachtel = 2;
            } 
        }

        public void setSelectedLength(STREIFEN wert)
        {
            this.SelectedStreifen = wert;
            SelectedLength = (int) wert;
            auto.SetMaxDurchlauf();
        }

        public void setSelectedLength(string wert)
        {
            SelectedLength = Int32.Parse(wert);
            auto.SetMaxDurchlauf();
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
            IstWertInMM = Int64.Parse(wert);
        }

        public long IstWertInMM
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
