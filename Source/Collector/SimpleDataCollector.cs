using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace custom_data_collector
{

    [DataCollectorFriendlyName("SimpleDataCollector")]
    [DataCollectorTypeUri("my://simple/datacollector")]
    public class SimpleDataCollector : DataCollector
    {
        private DataCollectionLogger _logger;
        private DataCollectionEnvironmentContext _context;
        private string _tempPath = string.Empty;

        public override void Initialize(
            System.Xml.XmlElement configurationElement,
            DataCollectionEvents events,
            DataCollectionSink dataSink,
            DataCollectionLogger logger,
            DataCollectionEnvironmentContext environmentContext)
        {
            events.SessionStart += this.SessionStarted_Handler;
            events.TestCaseStart += this.Events_TestCaseStart;
            _logger = logger;
            _context = environmentContext;
        }

        private void SessionStarted_Handler(object sender, SessionStartEventArgs args)
        {
            LogMessage("Building session");
            _tempPath = Path.GetTempPath();
            var desFile = @"C:\code\prac\custom-data-collector\Code.Coverage\Source\UnitTest\bin\Debug\netcoreapp3.0\Service.dll";
            var srcFilePath = Directory
                .GetFiles(_tempPath, "*.dll", SearchOption.AllDirectories).ToList()
                .FirstOrDefault(x => string.Equals(x, Path.GetFileName(desFile), StringComparison.InvariantCultureIgnoreCase));
            LogMessage("fILE FOUND : " + (srcFilePath == null));
            if (srcFilePath != null)
            {
                LogMessage("Delete original file");
                File.Delete(desFile);
                File.Copy(srcFilePath, Path.GetDirectoryName(desFile));
                LogMessage("Files copied");
            }
            // var sorucefile = @"C:\code\prac\custom-data-collector\Code.Coverage\Source\UnitTest\Service.dll";

            // LogMessage("getting temp path :" + _tempPath);
            // File.Copy(desFile, Path.Combine(_tempPath, Path.GetFileName(desFile)));
            // File.Delete(desFile);
            // File.Copy(sorucefile, desFile, true);

        }
        private void LogMessage(string message)
        {
            _logger.LogWarning(_context.SessionDataCollectionContext, message);
        }
        private void Events_TestCaseStart(object sender, TestCaseStartEventArgs e)
        {
            // var filesCompletePath = Directory.GetFiles(_tempPath).Select(x => new { Filename = Path.GetFileName(x), FullPath = x }).ToList();
            // LogMessage(filesCompletePath.Count.ToString() + " files found");
            // var allAssemblies = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll", SearchOption.AllDirectories);
            // string directoryname = null;
            // foreach (var f1 in allAssemblies)
            // {

            //     var f2 = filesCompletePath
            //     .FirstOrDefault(x => string.Equals(x.Filename, Path.GetFileName(f1), StringComparison.InvariantCultureIgnoreCase));
            //     if (f2 != null)
            //     {
            //         LogMessage("entry found");
            //         if (directoryname == null)
            //         {
            //             directoryname = Path.GetDirectoryName(f2.FullPath);
            //         }
            //         File.Delete(f2.FullPath);
            //         File.Copy(f1, directoryname, true);
            //         _logger.LogWarning(_context.SessionDataCollectionContext, f1 + "is copied to " + directoryname);
            //     }
            // }

            // _logger.LogWarning(_context.SessionDataCollectionContext, "Session ends : " + allAssemblies.Count());
        }
    }
}