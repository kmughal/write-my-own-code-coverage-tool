namespace Build
{
    using Codecoverage;
    using System;
    using System.Collections.Generic;
    using static System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            
            WriteLine(new DateTime().ToString());
            
            var parameters = new List<string> { @"C:\code\prac\custom-data-collector\Code.Coverage\Source\Service\obj\Debug\netstandard2.0\Service.dll" };
            StartCoverage.Instance.AddCodeForCoverage(parameters, m => WriteLine(m));
            Read();
        }
    }
}
