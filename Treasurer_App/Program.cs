using System.Configuration;
using System.Collections.Specialized;
using System.Reflection;
using Treasurer_App.Classes.StaticClasses;

namespace Treasurer_App
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Init();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static void Init () {
            try {
                Logger.Init();
                PersistentDataManager.Init();
            } catch (Exception ex) {
                Console.Error.WriteLine("Error encountered during Init: " + ex.ToString());
                throw;
            }
        }

    }
}