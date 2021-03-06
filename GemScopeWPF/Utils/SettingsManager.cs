﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Microsoft.Win32;

namespace GemScopeWPF.Utils
{
    class SettingsManager
    {

        public static void UpdateSetting(string key,string value ) 
        {
            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ////config.AppSettings.Settings[key].Value = value;

            //if (config.AppSettings.Settings[key] == null)
            //{
            //    config.AppSettings.Settings.Add(key, value);
            //}
            //else
            //{
            //    config.AppSettings.Settings[key].Value = value;
            //}

            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("appSettings");

            RegistryKey regkey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\GEMSCOPE");

            if (regkey != null)
            {
                regkey.SetValue(key, value);
                regkey.Close();

            }

        }
        public static string ReadSetting(string key)
        {
            //return ConfigurationManager.AppSettings[key];
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\GEMSCOPE");

            string result = String.Empty;

            if (regkey != null)
            {
                result = (string)regkey.GetValue(key);
                regkey.Close();

            }

            return result;

        }
        public static bool ReadBoolSetting(string key)
        {
            if (SettingsManager.ReadSetting(key) == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void WriteBoolSetting(string key,bool val)
        {
            if (val)
            {
                UpdateSetting(key, "1");
            }
            else
            {
                UpdateSetting(key, "0");
            }
        }

        
    }
}
