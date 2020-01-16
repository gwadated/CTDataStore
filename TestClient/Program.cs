namespace CTDataStore
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Celcat.Verto.DataStore;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Progress;
    using Common.Logging;

    internal class Program
    {
        // http://netcommon.sourceforge.net/docs/1.2.0/reference/html/logging.html
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // note that you can place expensive log calls in lambdas like this:
        // "log.Debug( m => m("Calling an expensive-slow argument: {0}”, someArgument));"
        // which delays the call until actually made

        private static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("CTDataStore.exe.config"));
            _log.Info("Starting CTDataStore");

            var sw = Stopwatch.StartNew();

            try
            {
                // note that we can also specify configuration programmatically rather than via xml
                var configuration = DataStoreConfiguration.Load("DataStoreConfig.xml");

                var controller = new Controller(configuration);
                controller.ProgressEvent += ControllerProgressEvent;
                controller.Execute();

                _log.Info("Finished CTDataStore");
            }
            catch (AggregateException ex)
            {
                Console.WriteLine(ex.InnerExceptions[0].Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($@"Duration = {sw.Elapsed}");
        }

        private static void ControllerProgressEvent(object sender, VertoProgressEventArgs e)
        {
            Console.WriteLine(e.ProgressString);
        }

        private static string GetSolutionVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var version = asm.GetName().Version;

            return version == null
                ? string.Empty
                : version.ToString();
        }
    }
}
