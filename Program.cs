/*
TheftProtection - data protection against swoop & swipe laptop theft.

Copyright (C) 2015 Matthew Weeks

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace TheftProtection{
    class Program {
        //Lets us lock the workstation
        [DllImport("user32")] public static extern void LockWorkStation();
        //Keeps track of whether our screen is locked
        static bool locked = false;
        //Last time we forced a hibernate, so we don't get stuck in a hibernate cycle
        static DateTime lastHiber = DateTime.Now;
        //What happens when cord unplugged/plugged in
        static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e){
            LockWorkStation();
        }
        //What happens when we lose network access, or an ethernet cable is plugged in, etc.
        static void AddressChangedCallback(object sender, EventArgs e) {
            //If we're locked (possibly due to cord unplugged?) hibernate
            if (locked && lastHiber.Subtract(DateTime.Now).TotalSeconds > 120) {
                lastHiber = DateTime.Now;
                Process.Start("C:\\Windows\\System32\\shutdown.exe", "-h -f");
            }
        }
        //What happens when the screen locks
        static void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e) {
            if (e.Reason == SessionSwitchReason.SessionLock) {
                locked = true;
            } else if (e.Reason == SessionSwitchReason.SessionUnlock) {
                locked = false;
            }
        }
        //Main code
        static void Main(string[] args) {
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            while (true) {
                Thread.Sleep(60 * 60 * 1000);
            }
        }
    }
}
