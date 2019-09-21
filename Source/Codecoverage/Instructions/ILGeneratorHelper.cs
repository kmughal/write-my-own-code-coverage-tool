namespace Codecoverage.Instructions
{
    using System.Collections.Generic;
    using System;
    using Mono.Cecil.Cil;
    using Mono.Cecil;

    public static class ILGeneratorHelper
    {
        public static Instruction GenerateInstruction(Instruction instruction, IDictionary<int, Instruction> targetedInstruction)
        {
            var(OperandType, opCode, operand) = (instruction.OpCode.OperandType, instruction.OpCode, instruction.Operand);

            if (OperandType == OperandType.InlineNone) return Instruction.Create(opCode);
            else if (OperandType == OperandType.InlineI) return Instruction.Create(opCode, (int) operand);
            else if (OperandType == OperandType.InlineI8) return Instruction.Create(opCode, (long) operand);
            else if (OperandType == OperandType.ShortInlineI) return GenerateInstructionForByte(opCode, operand);
            else if (OperandType == OperandType.InlineR) return Instruction.Create(opCode, (double) operand);
            else if (OperandType == OperandType.ShortInlineR) return Instruction.Create(opCode, (float) operand);
            else if (OperandType == OperandType.InlineString) return Instruction.Create(opCode, (string) operand);
            else if (OperandType == OperandType.InlineBrTarget || OperandType == OperandType.ShortInlineBrTarget)
                return GenerateInstructionForLineBreak(opCode, operand, targetedInstruction);
            else if (OperandType == OperandType.ShortInlineVar || OperandType == OperandType.InlineVar) return GenerateInstructionForInlineVariable(opCode, operand);
            else if (OperandType == OperandType.ShortInlineArg || OperandType == OperandType.InlineArg) return GenerateInstructionForParameters(opCode, operand);
            else if (OperandType == OperandType.InlineTok || OperandType == OperandType.InlineType) return GenerateInstructionForType(opCode, operand);
            else if (OperandType == OperandType.InlineField) return Instruction.Create(opCode, (FieldReference) operand);
            else if (OperandType == OperandType.InlineMethod) return Instruction.Create(opCode, (MethodReference) operand);

            throw new NotSupportedException();
        }

        private static Instruction GenerateInstructionForType(OpCode opCode, object operand)
        {
            var operandAsParameter = (TypeReference) operand;
            return Instruction.Create(opCode, operandAsParameter);
        }

        private static Instruction GenerateInstructionForParameters(OpCode opCode, object operand)
        {
            var operandAsParameter = (ParameterDefinition) operand;
            return Instruction.Create(opCode, operandAsParameter);
        }

        private static Instruction GenerateInstructionForInlineVariable(OpCode opCode, object operand)
        {
            var operandAsVariable = (VariableDefinition) operand;
            return Instruction.Create(opCode, operandAsVariable);
        }

        private static Instruction GenerateInstructionForLineBreak(OpCode opCode, object operand, IDictionary<int, Instruction> targetedInstruction)
        {
            var target = (Instruction) operand;
            var mappedOpCode = MapInstructionOpCodeForBreakStatement(opCode);
            var instructionByOffSet = targetedInstruction[target.Offset];
            var result = Instruction.Create(mappedOpCode, instructionByOffSet);
            return result;
        }

        private static OpCode MapInstructionOpCodeForBreakStatement(OpCode opCode)
        {
            if (opCode == OpCodes.Br_S) return OpCodes.Br;
            else if (opCode == OpCodes.Brfalse_S) return OpCodes.Brfalse;
            else if (opCode == OpCodes.Brtrue_S) return OpCodes.Brtrue;
            else if (opCode == OpCodes.Beq_S) return OpCodes.Beq;
            else if (opCode == OpCodes.Bge_S) return OpCodes.Bge;
            else if (opCode == OpCodes.Bgt_S) return OpCodes.Bgt;
            else if (opCode == OpCodes.Ble_S) return OpCodes.Ble;
            else if (opCode == OpCodes.Blt_S) return OpCodes.Blt;
            else if (opCode == OpCodes.Bne_Un_S) return OpCodes.Bne_Un;
            else if (opCode == OpCodes.Bge_Un_S) return OpCodes.Bge_Un;
            else if (opCode == OpCodes.Bgt_Un_S) return OpCodes.Bgt;
            else if (opCode == OpCodes.Ble_Un_S) return OpCodes.Ble;
            else if (opCode == OpCodes.Blt_Un_S) return OpCodes.Blt;
            else if (opCode == OpCodes.Leave_S) return OpCodes.Leave;
            return opCode;
        }

        private static Instruction GenerateInstructionForByte(OpCode opCode, object operand)
        {
            if (opCode == OpCodes.Ldc_I4_S) return Instruction.Create(opCode, (sbyte) operand);
            return Instruction.Create(opCode, (byte) operand);
        }
    }
}