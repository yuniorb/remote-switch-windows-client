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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RemoteSwitchClient.Properties;

namespace RemoteSwitchClient
{
    public class SwitchManager
    {
        private const int MaxTooltipLength = 63; // framework constraint

        private readonly SwitchClient switchClient;
        private readonly List<SwitchEvent> switchEvents = new List<SwitchEvent>();
        private readonly NotifyIcon notifyIcon;

        public SwitchManager(NotifyIcon notifyIcon)
        {
            this.notifyIcon = notifyIcon;
            var configuredUrl = ConfigurationManager.AppSettings["SwitchServerUrl"];
            Uri url;
            if (!Uri.TryCreate(configuredUrl, UriKind.Absolute, out url))
            {
                // TODO translate
                throw new ArgumentException(
                    "An error occurred while trying to retrieve the switch server url. Make sure the switch server url is set and it is correct");
            }
            switchClient = new SwitchClient(url);
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
        
        public void BuildContextMenu(ContextMenuStrip contextMenuStrip)
        {
            contextMenuStrip.Items.Clear();
            contextMenuStrip.Items.AddRange(switchEvents
                .OrderBy(project => project.Name)
                .Select(CreateSubMenu).ToArray());
        }

        public ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null) { item.Click += eventHandler; }
            return item;
        }

        private ToolStripMenuItem CreateSubMenu(SwitchEvent switchEvent)
        {
            var menuItem = new ToolStripMenuItem(switchEvent.Name)
            {
                Image = switchEvent.Status ? Resources.SquareGreenButton : Resources.SquareRedButton,
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

        private void ChangeTrayIcon(int onSwitches, int totalSwitches)
        {
            Icon status;
            if (onSwitches == totalSwitches)
            {
                status = Resources.GreenButton;
            }
            else if (onSwitches > 0 && onSwitches < totalSwitches)
            {
                status = Resources.OrangeButton;
            }
            else
            {
                status = Resources.RedButton;
            }

            this.notifyIcon.Icon = status;
        }

        private void SetNotifyIconToolTip()
        {
            int switchesOn = switchEvents.Count(x => x.Status);

            //TODO: translate
            string toolTipText = $"{switchesOn} of {switchEvents.Count} switches are on.";

            notifyIcon.Text = toolTipText.Length >= MaxTooltipLength ?
                toolTipText.Substring(0, MaxTooltipLength - 3) + "..." : toolTipText;

            ChangeTrayIcon(switchesOn, switchEvents.Count);
        }
    }
}
