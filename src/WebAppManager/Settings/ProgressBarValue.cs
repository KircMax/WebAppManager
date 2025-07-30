using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Settings
{
    public class ProgressBarValue : PropertyChangedBase
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
