using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchneidMaschine.model
{
    public class Statistik
    {

        private DataModel dataModel;

        private long _streifen_laenge_ist;

        // Heute-Total
        private long _heute_streifen_40er_kurz;
        private long _heute_streifen_40er_lang;
        private long _heute_streifen_70er_deckel;
        private long _heute_schachtel_40er_kurz;
        private long _heute_schachtel_40er_lang;
        private long _heute_schachtel_70er_deckel;
        private long _heute_rolle_abgewickelt;

        // Rollen-Total
        private long _rolle_streifen_40er_kurz;
        private long _rolle_streifen_40er_lang;
        private long _rolle_streifen_70er_deckel;
        private long _rolle_schachtel_40er_kurz;
        private long _rolle_schachtel_40er_lang;
        private long _rolle_schachtel_70er_deckel;
        private long _rolle_ist_laenge;
        private long _rolle_laenge_gesamt;

        // Langzeit-Total
        private long _langzeit_streifen_40er_kurz;
        private long _langzeit_streifen_40er_lang;
        private long _langzeit_streifen_70er_deckel;
        private long _langzeit_schachtel_40er_kurz;
        private long _langzeit_schachtel_40er_lang;
        private long _langzeit_schachtel_70er_deckel;
        private long _langzeit_verbrauchte_rollen;



        public Action ChangedStreifenLaengeIst;

        // Heute-Total
        public Action ChangedHeuteStreifen40erKurz;
        public Action ChangedHeuteStreifen40erLang;
        public Action ChangedHeuteStreifen70erDeckel;
        public Action ChangedHeuteSchachtel40erKurz;
        public Action ChangedHeuteSchachtel40erLang;
        public Action ChangedHeuteSchachtel70erDeckel;
        public Action ChangedHeuteRolleAbgewickelt;

        // Rollen-Total
        public Action ChangedRolleStreifen40erKurz;
        public Action ChangedRolleStreifen40erLang;
        public Action ChangedRolleStreifen70erDeckel;
        public Action ChangedRolleSchachtel40erKurz;
        public Action ChangedRolleSchachtel40erLang;
        public Action ChangedRolleSchachtel70erDeckel;
        public Action ChangedRolleIstLaenge;
        public Action ChangedRolleLaengeGesamt;

        // Langzeit-Total
        public Action ChangedLangzeitStreifen40erKurz;
        public Action ChangedLangzeitStreifen40erLang;
        public Action ChangedLangzeitStreifen70erDeckel;
        public Action ChangedLangzeitSchachtel40erKurz;
        public Action ChangedLangzeitSchachtel40erLang;
        public Action ChangedLangzeitSchachtel70erDeckel;
        public Action ChangedLangzeitVerbrauchteRollen;

        public Statistik(DataModel dataModel) 
        {
            this.dataModel = dataModel;
        }

        public void resetHeuteTotal() {
            HeuteStreifen40erKurz = 0;
            HeuteStreifen40erLang = 0;
            HeuteStreifen70erDeckel = 0;
            HeuteRolleAbgewickelt = 0;
        }


        public long StreifenLaengeIst
        {
            get => _streifen_laenge_ist;

            set
            {
                _streifen_laenge_ist = value;
                OnChangedStreifenLaengeIst();
            }
        }


        // Heute-Total
        public long HeuteStreifen40erKurz
        {
            get => _heute_streifen_40er_kurz;

            set
            {            
                _heute_streifen_40er_kurz = value;
                OnChangedHeuteStreifen40erKurz();
            }
        }

        public long HeuteStreifen40erLang
        {
            get => _heute_streifen_40er_lang;

            set
            {
                _heute_streifen_40er_lang = value;
                OnChangedHeuteStreifen40erLang();
            }
        }

        public long HeuteStreifen70erDeckel
        {
            get => _heute_streifen_70er_deckel;

            set
            {
                _heute_streifen_70er_deckel = value;
                OnChangedHeuteStreifen70erDeckel();
            }
        }

        public long HeuteSchachtel40erKurz
        {
            get => _heute_schachtel_40er_kurz;

            set
            {
                _heute_schachtel_40er_kurz = value;
                OnChangedHeuteSchachtel40erKurz();
            }
        }

        public long HeuteSchachtel40erLang
        {
            get => _heute_schachtel_40er_lang;

            set
            {
                _heute_schachtel_40er_lang = value;
                OnChangedHeuteSchachtel40erLang();
            }
        }

        public long HeuteSchachtel70erDeckel
        {
            get => _heute_schachtel_70er_deckel;

            set
            {
                _heute_schachtel_70er_deckel = value;
                OnChangedHeuteSchachtel70erDeckel();
            }
        }

        public long HeuteRolleAbgewickelt
        {
            get => _heute_rolle_abgewickelt;

            set
            {
                _heute_rolle_abgewickelt = value;
                OnChangedHeuteRolleAbgewickelt();
            }
        }

        // Rollen-Total
        public long RolleStreifen40erKurz
        {
            get => _rolle_streifen_40er_kurz;

            set
            {              
                _rolle_streifen_40er_kurz = value;
                OnChangedRolleStreifen40erKurz();
            }
        }

        public long RolleStreifen40erLang
        {
            get => _rolle_streifen_40er_lang;

            set
            {
                _rolle_streifen_40er_lang = value;
                OnChangedRolleStreifen40erLang();
            }
        }

        public long RolleStreifen70erDeckel
        {
            get => _rolle_streifen_70er_deckel;

            set
            {
                _rolle_streifen_70er_deckel = value;
                OnChangedRolleStreifen70erDeckel();
            }
        }

        public long RolleSchachtel40erKurz
        {
            get => _rolle_schachtel_40er_kurz;

            set
            {
                _rolle_schachtel_40er_kurz = value;
                OnChangedRolleSchachtel40erKurz();
            }
        }

        public long RolleSchachtel40erLang
        {
            get => _rolle_schachtel_40er_lang;

            set
            {
                _rolle_schachtel_40er_lang = value;
                OnChangedRolleSchachtel40erLang();
            }
        }

        public long RolleSchachtel70erDeckel
        {
            get => _rolle_schachtel_70er_deckel;

            set
            {
                _rolle_schachtel_70er_deckel = value;
                OnChangedRolleSchachtel70erDeckel();
            }
        }

        public long RolleIstLaenge
        {
            get => _rolle_ist_laenge;

            set
            {
                _rolle_ist_laenge = value;
                OnChangedRolleIstLaenge();
            }
        }

        public long RolleLaengeGesamt
        {
            get => _rolle_laenge_gesamt;

            set
            {
                _rolle_laenge_gesamt = value;
                OnChangedRolleLaengeGesamt();
            }
        }

        // Langzeit-Total
        public long LangzeitStreifen40erKurz
        {
            get => _langzeit_streifen_40er_kurz;

            set
            {
                _langzeit_streifen_40er_kurz = value;
                OnChangedLangzeitStreifen40erKurz();
            }
        }

        public long LangzeitStreifen40erLang
        {
            get => _langzeit_streifen_40er_lang;

            set
            {
                _langzeit_streifen_40er_lang = value;
                OnChangedLangzeitStreifen40erLang();
            }
        }

        public long LangzeitStreifen70erDeckel
        {
            get => _langzeit_streifen_70er_deckel;

            set
            {
                _langzeit_streifen_70er_deckel = value;
                OnChangedLangzeitStreifen70erDeckel();
            }
        }

        public long LangzeitSchachtel40erKurz
        {
            get => _langzeit_schachtel_40er_kurz;

            set
            {
                _langzeit_schachtel_40er_kurz = value;
                OnChangedLangzeitSchachtel40erKurz();
            }
        }

        public long LangzeitSchachtel40erLang
        {
            get => _langzeit_schachtel_40er_lang;

            set
            {
                _langzeit_schachtel_40er_lang = value;
                OnChangedLangzeitSchachtel40erLang();
            }
        }

        public long LangzeitSchachtel70erDeckel
        {
            get => _langzeit_schachtel_70er_deckel;

            set
            {
                _langzeit_schachtel_70er_deckel = value;
                OnChangedLangzeitSchachtel70erDeckel();
            }
        }

        public long LangzeitVerbrauchteRollen
        {
            get => _langzeit_verbrauchte_rollen;

            set
            {
                _langzeit_verbrauchte_rollen = value;
                OnChangedLangzeitVerbrauchteRollen();
            }
        }


        protected virtual void OnChangedStreifenLaengeIst() => ChangedStreifenLaengeIst?.Invoke();


        // Heute-Total
        protected virtual void OnChangedHeuteStreifen40erKurz() => ChangedHeuteStreifen40erKurz?.Invoke();
        protected virtual void OnChangedHeuteStreifen40erLang() => ChangedHeuteStreifen40erLang?.Invoke();
        protected virtual void OnChangedHeuteStreifen70erDeckel() => ChangedHeuteStreifen70erDeckel?.Invoke();
        protected virtual void OnChangedHeuteSchachtel40erKurz() => ChangedHeuteSchachtel40erKurz?.Invoke();
        protected virtual void OnChangedHeuteSchachtel40erLang() => ChangedHeuteSchachtel40erLang?.Invoke();
        protected virtual void OnChangedHeuteSchachtel70erDeckel() => ChangedHeuteSchachtel70erDeckel?.Invoke();
        protected virtual void OnChangedHeuteRolleAbgewickelt() => ChangedHeuteRolleAbgewickelt?.Invoke();

        // Rollen-Total
        protected virtual void OnChangedRolleStreifen40erKurz() => ChangedRolleStreifen40erKurz?.Invoke();
        protected virtual void OnChangedRolleStreifen40erLang() => ChangedRolleStreifen40erLang?.Invoke();
        protected virtual void OnChangedRolleStreifen70erDeckel() => ChangedRolleStreifen70erDeckel?.Invoke();
        protected virtual void OnChangedRolleSchachtel40erKurz() => ChangedRolleSchachtel40erKurz?.Invoke();
        protected virtual void OnChangedRolleSchachtel40erLang() => ChangedRolleSchachtel40erLang?.Invoke();
        protected virtual void OnChangedRolleSchachtel70erDeckel() => ChangedRolleSchachtel70erDeckel?.Invoke();
        protected virtual void OnChangedRolleIstLaenge() => ChangedRolleIstLaenge?.Invoke();
        protected virtual void OnChangedRolleLaengeGesamt() => ChangedRolleLaengeGesamt?.Invoke();

        // Langzeit-Total
        protected virtual void OnChangedLangzeitStreifen40erKurz() => ChangedLangzeitStreifen40erKurz?.Invoke();
        protected virtual void OnChangedLangzeitStreifen40erLang() => ChangedLangzeitStreifen40erLang?.Invoke();
        protected virtual void OnChangedLangzeitStreifen70erDeckel() => ChangedLangzeitStreifen70erDeckel?.Invoke();
        protected virtual void OnChangedLangzeitSchachtel40erKurz() => ChangedLangzeitSchachtel40erKurz?.Invoke();
        protected virtual void OnChangedLangzeitSchachtel40erLang() => ChangedLangzeitSchachtel40erLang?.Invoke();
        protected virtual void OnChangedLangzeitSchachtel70erDeckel() => ChangedLangzeitSchachtel70erDeckel?.Invoke();
        protected virtual void OnChangedLangzeitVerbrauchteRollen() => ChangedLangzeitVerbrauchteRollen?.Invoke();
    }
}
