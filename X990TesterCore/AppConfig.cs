using System;
using System.Configuration;

namespace X990TesterCore
{
    public static class AppConfig
    {
        public static string DefaultIpAddress => 
            ConfigurationManager.AppSettings["TerminalIP"] ?? "192.168.1.50";
        
        public static int DefaultPort => 
            int.TryParse(ConfigurationManager.AppSettings["TerminalPort"], out int port) 
                ? port : 7800;
        
        public static int ConnectionTimeout => 
            int.TryParse(ConfigurationManager.AppSettings["ConnectionTimeout"], out int timeout) 
                ? timeout : 5000;
        
        public static int DefaultCurrency => 
            int.TryParse(ConfigurationManager.AppSettings["DefaultCurrency"], out int currency) 
                ? currency : 840; // USD

        /// <summary>
        /// Saves a setting to the App.config file
        /// </summary>
        /// <param name="key">Setting key (e.g., "TerminalIP")</param>
        /// <param name="value">Setting value</param>
        public static void SaveSetting(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new InvalidOperationException($"Error saving configuration setting '{key}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves the terminal IP address to App.config
        /// </summary>
        /// <param name="ipAddress">IP address to save</param>
        public static void SaveTerminalIP(string ipAddress)
        {
            SaveSetting("TerminalIP", ipAddress);
        }

        /// <summary>
        /// Saves the terminal port to App.config
        /// </summary>
        /// <param name="port">Port number to save</param>
        public static void SaveTerminalPort(int port)
        {
            SaveSetting("TerminalPort", port.ToString());
        }

        /// <summary>
        /// Saves the default currency to App.config
        /// </summary>
        /// <param name="currencyCode">Currency code (376 for ILS, 840 for USD)</param>
        public static void SaveDefaultCurrency(int currencyCode)
        {
            SaveSetting("DefaultCurrency", currencyCode.ToString());
        }
    }
}
