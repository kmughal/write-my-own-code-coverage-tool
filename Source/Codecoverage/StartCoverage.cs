namespace Codecoverage
{
    using Codecoverage.Instructions;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class StartCoverage
    {
        StartCoverage() { }

        public static StartCoverage Instance => new StartCoverage();

        public void AddCodeForCoverage(List<string> assembliesFullPath, Action<object> cb)
        {
            foreach (var dllPath in assembliesFullPath)
            {
                try
                {
                    var copyDllPath = CopyAssemblyAndPdb(dllPath, cb);
                    var (types, moduleDefinition) = GetTypes(copyDllPath, cb);
                    cb("type fetch complete : " + types.Count);
                    InspectTypes(copyDllPath, types, cb);
                    if (File.Exists(dllPath)) File.Delete(dllPath);
                   
                    moduleDefinition.Write(dllPath, new WriterParameters { WriteSymbols = true });
                }
                catch (Exception e)
                {
                    cb(e.StackTrace);
                }
            }
        }

        //# Todo This needs to be improve
        private string CopyAssemblyAndPdb(string dllPath, Action<object> cb)
        {
            var fileNameOnly = Path.GetFileNameWithoutExtension(dllPath);
            var files = _getDllAndPdbFiles(dllPath, fileNameOnly);
            var copyPath = Path.GetTempPath();
            foreach (var file in files)
            {
                var destFilename = _createDestinationFilePath(copyPath, file);
                if (File.Exists(destFilename)) File.Delete(destFilename);
                File.Copy(file, destFilename);
            }

            return Path.Combine(copyPath, Path.GetFileName(dllPath));

            string _createDestinationFilePath(string _rootFolder, string _file) => Path.Combine(_rootFolder, Path.GetFileName(_file));

            List<string> _getDllAndPdbFiles(string _dllPath, string _fileNameOnly)
            {
                var rootFolder = Path.GetDirectoryName(_dllPath);

                var result = new List<string>();
                var pdbFile = Path.Combine(rootFolder, $"{_fileNameOnly}.pdb");
                if (!File.Exists(pdbFile)) throw new FileNotFoundException($"{pdbFile} is not present!");
                result.Add(pdbFile);

                var assemblyFile = Path.Combine(rootFolder, $"{_fileNameOnly}.dll");
                if (!File.Exists(assemblyFile)) throw new FileNotFoundException($"{assemblyFile} is not present!");
                result.Add(assemblyFile);
                return result;
            }
        }

        private void InspectTypes(string dllPath, List<TypeDefinition> types, Action<object> cb)
        {
            foreach (var type in types)
            {
                InspectType(dllPath, type, cb);
            }
        }

        private void InspectType(string dllPath, TypeDefinition type, Action<object> cb)
        {
            var methods = type.Methods.ToList();
            InspectMethods(dllPath, methods, cb);
        }

        private void InspectMethods(string dllPath, List<MethodDefinition> methods, Action<object> cb)
        {
            foreach (var method in methods)
            {
                InspectMethod(dllPath, method, cb);
            }
        }

        private void InspectMethod(string dllPath, MethodDefinition method, Action<object> cb)
        {
            if (method.HasBody)
            {
                RewriteIL(dllPath, method, cb);
            }
        }

        private void RewriteIL(string dllPath, MethodDefinition method, Action<object> cb)
        {
            var instructions = method.Body.Instructions.ToList();
            var targetInstructions = GetInstructionsWhichNeedsRewrite(instructions, cb);

            ClearMethodBodyInstructions(method);
            var processor = method.Body.GetILProcessor();
            foreach (var instruction in instructions)
            {
                foreach (var generatedInstruction in GenerateInstrumentAfterAddingCoverageHook(
                        targetInstructions,
                        instruction, method, processor))
                {
                    if (generatedInstruction == null) continue;

                    var sequencePoint = method.DebugInformation.GetSequencePoint(instruction);
                    var typeName = method.DeclaringType.Name;
                    CodeVisitor.AppendCodeInstrument(processor, sequencePoint, "coverage.txt", typeName, method.Name);

                    if (InstructionInTargetInstructionList(instruction, targetInstructions)) OverrideInstructionValues(targetInstructions, instruction);
                    else processor.Append(instruction);
                }
            }
        }

        private void OverrideInstructionValues(IDictionary<int, Instruction> targetedInstructions, Instruction instruciton)
        {
            if (instruciton == null || targetedInstructions == null || !targetedInstructions.Any()) return;

            var offSet = instruciton.Offset;
            if (!targetedInstructions.Keys.Contains(offSet)) return;
            if (instruciton == null || targetedInstructions[offSet] == null) return;

            targetedInstructions[offSet].Offset = instruciton.Offset;
            targetedInstructions[offSet].OpCode = instruciton.OpCode;
            targetedInstructions[offSet].Operand = instruciton.Operand;
            targetedInstructions[offSet].Previous = instruciton.Previous;
            targetedInstructions[offSet].Next = instruciton.Next;
        }

        private bool InstructionInTargetInstructionList(Instruction instruction, IDictionary<int, Instruction> targetedInstructions) =>
            targetedInstructions.Any(x => x.Value.Offset == instruction.Offset);

        private IEnumerable<Instruction> GenerateInstrumentAfterAddingCoverageHook(
            IDictionary<int, Instruction> targetInstructions,
            Instruction instruction,
            MethodDefinition method,
            ILProcessor processor)
        {

            yield return ILGeneratorHelper.GenerateInstruction(instruction, targetInstructions);

            var sequencePoint = method.DebugInformation.GetSequencePoint(instruction);
            var typeName = method.DeclaringType.Name;
            // foreach (var inst in CodeVisitor.GenerateCodeToAppendForInspection(processor, sequencePoint, "file.txt", typeName))
            // {
            //     yield return inst;
            // }
        }

        private void ClearMethodBodyInstructions(MethodDefinition method)
        {
            method.Body.Instructions.Clear();
        }

        private IDictionary<int, Instruction> GetInstructionsWhichNeedsRewrite(List<Instruction> instructions, Action<object> cb)
        {
            IEnumerable<int> target = instructions.Where(x => (x.Operand as Instruction) != null).Select(x => (x.Operand as Instruction).Offset);
            foreach (var i in target) cb(i.ToString());
            var result = new Dictionary<int, Instruction>();
            foreach (var instruction in instructions)
            {
                if (target.Contains(instruction.Offset))
                {
                    result.Add(instruction.Offset, Instruction.Create(OpCodes.Nop));
                }
            }

            return result;
        }

        private (List<TypeDefinition> types, ModuleDefinition moduleDefinition) GetTypes(string dllPath, Action<object> cb)
        {
            cb("dll path" + dllPath);

            try
            {
                var param = new ReaderParameters { ReadSymbols = true };
                var moduleDefinition = ModuleDefinition.ReadModule(dllPath, param);
                var types = moduleDefinition.GetTypes().ToList();
                return (types, moduleDefinition);
            }
            catch (Exception e)
            {
                cb(e.StackTrace);
            }
            return (null, null);
        }

    }
}