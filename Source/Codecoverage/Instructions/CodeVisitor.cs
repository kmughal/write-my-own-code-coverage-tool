namespace Codecoverage.Instructions
{
    using Mono.Cecil.Cil;
    using System.IO;

    public static class CodeVisitor
    {
        public static void AppendCodeInstrument(ILProcessor processor, SequencePoint sequencePoint, string filePath, string typeName, string methodName)
        {
            if (sequencePoint == null || sequencePoint.StartLine == 16707566) return;
            processor.Append(Instruction.Create(OpCodes.Ldstr, filePath));
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Type:{typeName},MethodName:{methodName}:Start{sequencePoint?.StartLine}:End:{sequencePoint?.EndLine}\n"));
            var fileOp = processor.Body.Method.Module.ImportReference(typeof(File)
                .GetMethod("AppendAllText", new[] { typeof(string), typeof(string) }));
            processor.Append(Instruction.Create(OpCodes.Call, fileOp));

        }
    }
}