using System;

namespace OneCopy2017.Services
{
    public class ErrorHandlingService
    {
        public ErrorHandlingService()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
        }

        public void ThrowCatastrophicError(string message, Exception ex = null)
        {
            EndProgram(message, ex);
        }

        public void EndProgram(string message, Exception ex = null)
        {
            if (ex != null)
                Console.WriteLine(ex.ToString());

            Console.WriteLine(message);
            Console.WriteLine("Program ended");
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }

        private void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            EndProgram(e.ExceptionObject.ToString());
        }
    }
}