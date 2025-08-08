// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Webserver.Api.Gui.Settings;
using Webserver.Api.Gui.WebAppManagerEvents;

namespace Webserver.Api.Gui.CustomControls
{
    /// <summary>
    /// Interaction logic for WebAppDeploySelectionSettingsControl.xaml
    /// </summary>
    public partial class SelectionSettingsControl : UserControl
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings",
                typeof(SelectionSettings),
                typeof(SelectionSettingsControl));
        public SelectionSettings Settings
        {
            get
            {
                return (SelectionSettings)GetValue(SettingsProperty);
            }
            set
            {
                SetValue(SettingsProperty, value);
            }
        }

        public event SelectionSettingsAvailableItemsChanged SelectionSettingsAvailableItemsChanged;

        public SelectionSettingsControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.SelectedItems.Clear();
            foreach (var item in Settings.AvailableItems.Values)
            {
                Settings.SelectedItems.Add(item);
            }
            RemoveDisplayControls();
            
            CheckValidity();
        }

        private void SelectSingleBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = AvailableItemsSelect.SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
                foreach (var item in selectedItems)
                {
                    if (!Settings.SelectedItems.Contains(item.ToString()))
                    {
                        Settings.SelectedItems.Add(item.ToString());
                    }
                }
            }
            RemoveDisplayControls();
            
            CheckValidity();
        }

        private void RemoveSingleBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndicies = GetSelectedIndicies(SelectedItemsSelect);
            if (selectedIndicies.Count > 0)
            {
                List<string> items = new List<string>();
                foreach(var selectedIndex in selectedIndicies)
                {
                    string item = Settings.SelectedItems[selectedIndex];
                    items.Add(item);
                }
                foreach (var item in items)
                {
                    Settings.SelectedItems.Remove(item);
                }
            }
            RemoveDisplayControls();
            CheckValidity();
        }

        private void RemoveAllBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.SelectedItems.Clear();
            RemoveDisplayControls();
            
            CheckValidity();
        }

        #region Control Display
        public void RemoveDisplayControls()
        {
            RemoveSingleBtn.IsEnabled = Settings?.SelectedItems?.Any() ?? false;
            RemoveAllBtn.IsEnabled = Settings?.SelectedItems?.Any() ?? false;
        }
        #endregion

        #region Helper functions
        private void MoveSelectedItems(int moveCount)
        {
            var selectedIndicies = GetSelectedIndicies(SelectedItemsSelect);
            if (selectedIndicies.Count > 0)
            {
                SelectedItemsSelect.SelectedItems.Clear();
                foreach (var index in selectedIndicies)
                {
                    int newIndex = index + moveCount;

                    if (newIndex >= 0 && newIndex < SelectedItemsSelect.Items.Count)
                    {
                        Settings.SelectedItems.Move(index, newIndex);
                        SelectedItemsSelect.SelectedItems.Add(Settings.SelectedItems[newIndex]);
                    }
                }
            }
        }
        private List<int> GetSelectedIndicies(ListBox listBox)
        {
            List<int> selectedIndicies = new List<int>();
            int selectedIndex = listBox.SelectedIndex;
            if (selectedIndex == -1)
            {
                return selectedIndicies;
            }
            selectedIndicies.Add(selectedIndex);

            if (listBox.SelectedItems.Count > 1)
            {
                while (listBox.SelectedItems.Count > 0)
                {
                    listBox.SelectedItems.RemoveAt(0);
                    selectedIndex = listBox.SelectedIndex;
                    if (selectedIndex != -1)
                    {
                        selectedIndicies.Add(selectedIndex);
                    }
                }
            }
            return selectedIndicies;
        }
        #endregion


        public void CheckValidity()
        {
            Settings.IsValid = (Settings.SelectedItems != null && Settings.SelectedItems.Count > 0) ||
                               (Settings.AvailableItems != null && Settings.AvailableItems.Count > 0);
            //Settings.IsValid = true;
        }

        private void DeleteSelectedBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndicies = GetSelectedIndicies(SelectedItemsSelect);
            if (SelectedItemsSelect.SelectedItems.Count == 0) {selectedIndicies = GetSelectedIndicies(AvailableItemsSelect);}
            List<KeyValuePair<string,string>> deletedItems = new List<KeyValuePair<string, string>>();
            
            if (selectedIndicies.Count > 0)
            {
                List<string> items = new List<string>();
                foreach (var selectedIndex in selectedIndicies)
                {
                    string item;
                    if (SelectedItemsSelect.SelectedItems.Count != 0)
                    {
                        item = Settings.SelectedItems[selectedIndex];
                    }
                    else
                    {
                        item = AvailableItemsSelect.SelectedItems[selectedIndex].ToString();
                    }
                    items.Add(item);
                }
                foreach (var item in items)
                {
                    Settings.SelectedItems.Remove(item);
                    var itemToRemove = Settings.AvailableItems.First(el => el.Value == item);
                    Settings.AvailableItems.Remove(itemToRemove.Key);// does not "fire" AvailableItems on Property Change
                    /*Settings.AvailableItems = Settings.AvailableItems
                        .Where(el => el.Key != itemToRemove.Key)
                        .ToDictionary();*/
                    deletedItems.Add(itemToRemove);
                }
                Settings.AvailableItems = Settings.AvailableItems;
            }
            RemoveDisplayControls();
            CheckValidity();
            SelectionSettingsAvailableItemsChanged?.Invoke(this, new WebAppManagerEvents.WebAppMangagerEventArgs.SelectionSettingsAvailableItemsChangedArgs() { DeletedItems = deletedItems });
        }
    }
}
