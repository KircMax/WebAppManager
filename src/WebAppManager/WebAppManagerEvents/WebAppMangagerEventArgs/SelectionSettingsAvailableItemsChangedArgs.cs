// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

namespace Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs
{
    public class SelectionSettingsAvailableItemsChangedArgs : EventArgs
    {
        public List<KeyValuePair<string, string>> DeletedItems;
    }
}
