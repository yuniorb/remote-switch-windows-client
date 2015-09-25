/* ***** BEGIN LICENSE BLOCK *****
 * Version: MIT License
 *
 * Copyright (c) 2010 Michael Sorens http://www.simple-talk.com/author/michael-sorens/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 * ***** END LICENSE BLOCK *****
 */
 
namespace RemoteSwitch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using RemoteSwitchClient;
    using RemoteSwitchClient.Properties;

    public class SwitchManager
    {
        private const int MaxTooltipLength = 63; // framework constraint

        private readonly SwitchClient switchClient;
        private readonly List<SwitchEvent> switchEvents = new List<SwitchEvent>();
        private readonly NotifyIcon notifyIcon;

        public SwitchManager(NotifyIcon notifyIcon)
        {
            this.notifyIcon = notifyIcon;
            //TODO let the user set the server url
            switchClient = new SwitchClient("http://localhost");
            switchClient.RegisterListener(HandleSwitchEvent);
        }

        public void HandleSwitchEvent(SwitchEvent switchEvent)
        {
            // TODO: how to update the ui when this event happens?
            var index = switchEvents.IndexOf(switchEvent);
            if (index != -1)
            {
                switchEvents[index] = switchEvent;
            }
            else
            {
                switchEvents.Add(switchEvent);
            }

            SetNotifyIconToolTip(); // udpdates the tooltip counts
        }

        public bool IsDecorated { get; private set; }
        public int EnabledColumnNumber = 4;

        # region context menu creation

        public void BuildContextMenu(ContextMenuStrip contextMenuStrip)
        {
            contextMenuStrip.Items.Clear();
            contextMenuStrip.Items.AddRange(switchEvents
                .OrderBy(project => project.Name)
                .Select(CreateSubMenu).ToArray());
        }

        private ToolStripMenuItem CreateSubMenu(SwitchEvent switchEvent)
        {
            var menuItem = new ToolStripMenuItem(switchEvent.Name)
            {
                Image = switchEvent.Status ? Resources.signal_green : Resources.signal_red,
                // TODO : translate in resource file
                ToolTipText = switchEvent.Status ? "ON" : "OFF"
            };
            menuItem.DropDownItems.Add(CreateToolStripMenuItem(switchEvent));
            return menuItem;
        }

        private ToolStripMenuItem CreateToolStripMenuItem(SwitchEvent switchEvent)
        {
            var item = new ToolStripMenuItem(switchEvent.EventDate.ToLocalTime().ToLongTimeString())
            {
                ToolTipText = switchEvent.EventDate.ToLocalTime().ToLongTimeString()
            };

            return item;
        }
        # endregion context menu creation

        # region hosts file analysis
        
        /// <summary>
        /// Builds the server associations into a dictionary.
        /// </summary>
        public void InitializeSwitches()
        {   
            SetNotifyIconToolTip();
        }

        private void SetNotifyIconToolTip()
        {
            int switchesOn = switchEvents.Count(x => x.Status);

            string toolTipText = $"{switchesOn} of {switchEvents.Count} switches are on.";

            notifyIcon.Text = toolTipText.Length >= MaxTooltipLength ?
                toolTipText.Substring(0, MaxTooltipLength - 3) + "..." : toolTipText;
        }
        
        # endregion hosts file analysis

        # region details form support
        
        public static readonly string EnabledLabel = "enabled";
        public static readonly string DisabledLabel = "disabled";

        public bool IsEnabled(string cellValue)
        {
            return EnabledLabel.Equals(cellValue);
        }
        
        # endregion details form support
        
        # region event handlers
        
        private ToolStripMenuItem ToolStripMenuItemWithHandler(
            string displayText, int enabledCount, int disabledCount, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null) { item.Click += eventHandler; }
            return item;
        }
        public ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, EventHandler eventHandler)
        {
            return ToolStripMenuItemWithHandler(displayText, 0, 0, eventHandler);
        }

        #endregion event handlers

        public void GenerateHostsDetails(DataGridView hostsDataGridView)
        {
            //TODO implement
        }
    }
}
