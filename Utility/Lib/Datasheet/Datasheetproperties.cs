using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Lib.Datasheet
{
    public class Datasheetproperties : BaseUtility
    {
        public string NCP
        {
            get => GetValue(() => NCP);
            set => SetValue(() => NCP, value);
        }
        public string RefSystemSN
        {
            get => GetValue(() => RefSystemSN);
            set => SetValue(() => RefSystemSN, value);
        }
        public string SystemSN
        {
            get => GetValue(() => SystemSN);
            set => SetValue(() => SystemSN, value);
        }
        public string SapNumber
        {
            get => GetValue(() => SapNumber);
            set => SetValue(() => SapNumber, value);
        }
        public string CustomerAbbr
        {
            get => GetValue(() => CustomerAbbr);
            set => SetValue(() => CustomerAbbr, value);
        }
        public string Datasheet
        {
            get => GetValue(() => Datasheet);
            set => SetValue(() => Datasheet, value);
        }

        public Datasheetproperties(string ncp, string refsystemsn, string systemsn, string sapnumber, string customerabbr, string datasheet)
        {
            NCP = ncp;
            RefSystemSN = refsystemsn;
            SystemSN = systemsn;
            SapNumber = sapnumber;
            CustomerAbbr = customerabbr;
            Datasheet = datasheet;
        }
        public Datasheetproperties() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        { }
        public bool Valid()
        {
            return !string.IsNullOrWhiteSpace(NCP) && !string.IsNullOrWhiteSpace(SystemSN) && !string.IsNullOrWhiteSpace(SapNumber) && !string.IsNullOrWhiteSpace(CustomerAbbr);
        }
        public void Copy(Datasheetproperties source)
        {
            NCP = source.NCP;
            SystemSN = source.SystemSN;
            SapNumber = source.SapNumber;
            CustomerAbbr = source.CustomerAbbr;
            RefSystemSN = source.RefSystemSN;
            Datasheet = source.Datasheet;
        }
    }
}
