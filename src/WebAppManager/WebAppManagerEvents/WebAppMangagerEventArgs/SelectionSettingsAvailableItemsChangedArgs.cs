// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs
{
    public class SelectionSettingsAvailableItemsChangedArgs : EventArgs
    {
        public List<KeyValuePair<string,string>> DeletedItems;
    }
}
