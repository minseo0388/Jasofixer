using System;
using System.IO;
using System.Windows.Forms;

namespace HangulJasofixer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Enable visual styles and set text rendering
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set up global exception handling
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                // Run the main form
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                HandleGlobalException(ex, "Application startup error");
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleGlobalException(e.Exception, "Unhandled thread exception");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleGlobalException(ex, "Unhandled domain exception");
            }
        }

        private static void HandleGlobalException(Exception ex, string context)
        {
            try
            {
                string errorMessage = $"An unexpected error occurred:\n\n{ex.Message}\n\nContext: {context}";
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}: {ex}";

                // Try to log to file
                try
                {
                    string logFile = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "HangulJasofixer",
                        "error.log");

                    string? logDirectory = Path.GetDirectoryName(logFile);
                    if (!string.IsNullOrEmpty(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }
                    File.AppendAllText(logFile, logMessage + Environment.NewLine);
                }
                catch
                {
                    // If logging fails, ignore and continue with message box
                }

                // Show error to user
                MessageBox.Show(errorMessage, "Application Error", 
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Last resort - basic message box
                MessageBox.Show("A critical error occurred in the application.", 
                               "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
