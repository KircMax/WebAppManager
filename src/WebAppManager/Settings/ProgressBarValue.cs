using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Settings
{
    public class ProgressBarValue
    {
        public int Value { get; set; }
        public ProgressBarValue(int value)
        {
            Value = value;
        }
        public override string ToString()
        {
            return $"{Value}%";
        }
    }
}
