using System;

namespace Iitrust.CRLPublication.Client
{
    /// <summary>
    /// Основа для приложения.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Входная точка запуска приложения.
        /// </summary>
        /// <param name="args">Аргументы, которые передаются при запуске приложения.</param>
        [STAThread]
        public static void Main(String[] args)
        {
            new WindowsFormsApp().Run(args);
        }
    }
}
