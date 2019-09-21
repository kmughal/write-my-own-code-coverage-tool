namespace Codecoverage.Common
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.PortableExecutable;
    using System;

    public static class ModulePathHelpers
    {

        public static List<string> GetModulesForCoverage(List<string> assemblyNames, Action<string> cb)
        {
            cb("ok ia ok");
            var currentDirectory =@"C:\code\prac\custom-data-collector\Code.Coverage\Source\UnitTest\bin\Debug\netcoreapp3.0";
            cb($"current directory ${currentDirectory}");
            var assemblyNamesToInstrument = GetAllDllsFromDebugFolder(assemblyNames, currentDirectory, cb);
            var result = GetModuleNamesWhichRequiresInstruments(assemblyNamesToInstrument, cb);
            return result;
        }

        private static List<string> GetAllDllsFromDebugFolder(List<string> assemblyNames, string baseDirectoryName, Action<string> cb)
        {

            var debugFolderPath = baseDirectoryName;
            var allAssemblies = Directory.GetFiles(debugFolderPath, "*.dll", SearchOption.AllDirectories).ToList();
            var result = allAssemblies.Where(n => assemblyNames.Any(n1 => n.Contains(n1))).ToList();
            cb("inside : " + string.Join(",", result));
            return result;
        }

        private static bool TestPDBExists(string dllPath, Action<string> cb)
        {
            using(var reader = new PEReader(File.OpenRead(Path.Combine(dllPath))))
            {

                foreach (var entry in reader.ReadDebugDirectory())
                {
                    //cb(reader.ReadCodeViewDebugDirectoryData(entry).Path);
                    if (entry.Type == DebugDirectoryEntryType.CodeView)
                    {
                        var codeViewData = reader.ReadCodeViewDebugDirectoryData(entry);
                        var pdbDirectory = Path.GetDirectoryName(dllPath);
                        var result = File.Exists(Path.Combine(pdbDirectory, Path.GetFileName(codeViewData.Path)));
                        //cb(pdbDirectory + ">>>>>" + codeViewData.Path);
                        return result;
                    }

                }
            }
            return false;
        }

        private static List<string> GetModuleNamesWhichRequiresInstruments(List<string> assemblyNames, Action<string> cb)
        {
            List<string> result = assemblyNames.Where(n => TestPDBExists(n, cb)).ToList();
            return result;
        }
    }
}