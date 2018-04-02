using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace ILGenerationLanguage.Plugin
{
    public class GeneratingMethodBodyDisassembler
    {
        private readonly ITextOutput output;
        private readonly CancellationToken cancellationToken;
        private readonly MethodInstructionComparer instructionComparer = new MethodInstructionComparer();

        public GeneratingMethodBodyDisassembler(ITextOutput output, CancellationToken cancellationToken)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            this.cancellationToken = cancellationToken;
        }

        private List<TResult> FlattenStructure<TResult, TAggregate>(TAggregate root, Func<TAggregate, IEnumerable<TAggregate>> propertySelector, Func<TAggregate, int, TResult> resultSelector)
        {
            List<TResult> flatList = new List<TResult>();
            flatList.Add(resultSelector(root, 0));
            InternalFlattenStructure(propertySelector(root), propertySelector, flatList, 1, resultSelector);
            return flatList;
        }

        private void InternalFlattenStructure<TResult, TAggregate>(IEnumerable<TAggregate> listToFlatten, Func<TAggregate, IEnumerable<TAggregate>> propertySelector, List<TResult> currentAggregate, int level, Func<TAggregate, int, TResult> resultSelector)
        {
            foreach (var element in listToFlatten)
            {
                currentAggregate.Add(resultSelector(element, level));
            }
            foreach (var element in listToFlatten)
            {
                InternalFlattenStructure(propertySelector(element), propertySelector, currentAggregate, level + 1, resultSelector);
            }
        }

        public virtual void Disassemble(MethodBody body)
        {
            Dictionary<VariableReference, string> locals = ProcessLocalVariables(body);
            Dictionary<Instruction, string> labelDictionary = ProcessLabels(body);
            ILStructure iLStructure = new ILStructure(body);
            var flattenedStructure = FlattenStructure(iLStructure, il => il.Children, (il, level) => new IlStructureWithLevel { Level = level, Structure = il });
            foreach (var inst in body.Method.Body.Instructions)
            {
                WriteInstruction(output, inst, flattenedStructure, labelDictionary, locals);
            }
        }

        private Dictionary<Instruction, string> ProcessLabels(MethodBody body)
        {
            Dictionary<Instruction, string> labelDictionary = GenerateLabels(body.Method.Body.Instructions);
            WriteLabels(output, labelDictionary);
            output.WriteLine();
            return labelDictionary;
        }
        private Dictionary<Instruction, string> GenerateLabels(Collection<Instruction> instructions)
        {
            var labels =
                instructions.Select(i => i.Operand)
                .OfType<Instruction>()
                .Distinct()
                .ToDictionary(i => i, i => $"label_{i.Offset}", instructionComparer);
            var multiLabels =
                instructions.Select(i => i.Operand)
                .OfType<Instruction[]>()
                .SelectMany(l => l)
                .Distinct()
                .ToDictionary(i => i, i => $"label_{i.Offset}", instructionComparer);

            foreach (var multiLabel in multiLabels)
            {
                if (!labels.Any(l => l.Key.Offset == multiLabel.Key.Offset))
                {
                    labels.Add(multiLabel.Key, multiLabel.Value);
                }
            }
            return labels;
        }
        private void WriteLabels(ITextOutput output, Dictionary<Instruction, string> labelDictionary)
        {
            foreach (var label in labelDictionary.Values)
            {
                output.WriteLine($"var {label} = generator.DefineLabel();");
            }
            output.WriteLine();
        }

        private Dictionary<VariableReference, string> ProcessLocalVariables(MethodBody body)
        {
            var locals = GetLocalVariables(body);
            WriteLocalVariables(locals);
            output.WriteLine();
            return locals;
        }
        private Dictionary<VariableReference, string> GetLocalVariables(MethodBody body)
        {
            return body.Variables.ToDictionary(v => (VariableReference)v, v => $"variable_{v.Index}");
        }
        private void WriteLocalVariables(Dictionary<VariableReference, string> variables)
        {
            foreach (var local in variables)
            {
                output.Write($"Localbuilder {local.Value} = generator.DeclareLocal({local.Key.VariableType.ToTypeOf()});");
                output.WriteLine();
            }
        }

        protected virtual void WriteInstruction(ITextOutput output, Instruction instruction, List<IlStructureWithLevel> iLStructures, Dictionary<Instruction, string> labels, Dictionary<VariableReference, string> variables)
        {
            if (labels.TryGetValue(instruction, out var labelName))
            {
                output.WriteLine($"generator.MarkLabel({labelName});");
            }

            var startStructures = iLStructures.Where(s=>s.Structure.Type!=ILStructureType.Root && s.Structure.Type != ILStructureType.Loop)
                                              .Where(s => s.Structure.StartOffset == instruction.Offset)
                                              .ToList();

            foreach (var startStructure in startStructures)            
            {
                switch (startStructure.Structure.Type)
                {
                    case ILStructureType.Try:
                        output.WriteLine("generator.BeginExceptionBlock();");
                        break;
                    case ILStructureType.Handler:
                        switch (startStructure.Structure.ExceptionHandler.HandlerType)
                        {
                            case ExceptionHandlerType.Catch:
                            case ExceptionHandlerType.Filter:
                                output.WriteLine($"generator.BeginCatchBlock({startStructure.Structure.ExceptionHandler.CatchType?.ToTypeOf() ?? string.Empty});");
                                break;
                            case ExceptionHandlerType.Finally:
                                output.WriteLine("generator.BeginFinallyBlock();");
                                break;
                            case ExceptionHandlerType.Fault:
                                output.WriteLine("generator.BeginFaultBlock();");
                                break;
                        }
                        break;
                    case ILStructureType.Filter:
                        output.WriteLine("generator.BeginExceptFilterBlock();");
                        break;
                }
            }

            var endStructures = iLStructures.Where(s => s.Structure.Type != ILStructureType.Root && s.Structure.Type != ILStructureType.Loop)
                                            .Where(s => s.Structure.EndOffset == instruction.Offset)
                                            .ToList();
            foreach (var endStructure in endStructures)
            {
                switch (endStructure.Structure.Type)
                {
                    case ILStructureType.Try:
                        output.WriteLine("// end try");
                        break;
                    case ILStructureType.Handler:
                        switch (endStructure.Structure.ExceptionHandler.HandlerType)
                        {
                            case ExceptionHandlerType.Catch:
                            case ExceptionHandlerType.Filter:
                                if (iLStructures.Where(il => il.Level == endStructure.Level).OrderBy(il => il.Structure.EndOffset).Last() == endStructure)
                                    output.WriteLine("generator.EndExceptionBlock();");
                                output.WriteLine("// end catch");
                                break;
                            case ExceptionHandlerType.Finally:
                                output.WriteLine("generator.EndExceptionBlock();");
                                output.WriteLine("// end finally");
                                break;
                            case ExceptionHandlerType.Fault:
                                output.WriteLine("// end fault");
                                break;
                        }
                        break;
                    case ILStructureType.Filter:
                        output.WriteLine("// end filter");
                        break;
                }
            }

            output.Write($"generator.Emit(OpCodes.{instruction.OpCode.Code}");
            if (instruction.Operand != null)
            {
                output.Write(",");
                WriteOperand(output, instruction.Operand, labels, variables);
            }
            output.WriteLine(");");
        }
        private void WriteOperand(ITextOutput output, object operand, Dictionary<Instruction, string> labels, Dictionary<VariableReference, string> locals)
        {
            if (operand is MethodReference methodReference)
            {
                var methodDefinition = methodReference.Resolve();
                output.WriteTypeOf(methodDefinition.DeclaringType);

                if (methodDefinition.IsGetter)
                {
                    output.Write($".GetProperty(\"{methodDefinition.Name.Split('_')[1]}\").GetGetMethod(true)");
                }
                else if (methodDefinition.IsSetter)
                {
                    output.Write($".GetProperty(\"{methodDefinition.Name.Split('_')[1]}\").GetSetMethod(true)");
                }
                else
                {
                    if (methodDefinition.IsConstructor)
                    {
                        output.Write($".GetConstructor(");
                    }
                    else
                    {
                        output.Write($".GetMethod(\"{methodDefinition.Name}\",");
                    }

                    if (methodReference.HasParameters)
                    {
                        output.Write($"new Type[] {{ {string.Join(", ", methodDefinition.Parameters.Select(p => p.ParameterType.ToTypeOf()))} }}");
                    }
                    else
                    {
                        output.Write("Type.EmptyTypes");
                    }
                    output.Write(")");
                }
            }
            else if (operand is string stringOperand)
            {
                output.Write($"\"{stringOperand}\"");
            }
            else if (operand is double doubleOperand)
            {
                output.Write(doubleOperand.ToString(CultureInfo.InvariantCulture));
            }
            else if (operand is float floatOperand)
            {
                output.Write(floatOperand.ToString(CultureInfo.InvariantCulture));
            }
            else if (operand is byte byteOperand)
            {
                output.Write(byteOperand.ToString(CultureInfo.InvariantCulture));
            }
            else if (operand is int intOperand)
            {
                output.Write(intOperand.ToString(CultureInfo.InvariantCulture));
            }
            else if (operand is long longOperand)
            {
                output.Write(longOperand.ToString(CultureInfo.InvariantCulture));
            }
            else if (operand is sbyte sbyteOperand)
            {
                output.Write(sbyteOperand.ToString(CultureInfo.InvariantCulture));
            }
            else if (operand is Instruction instruction)
            {
                output.Write(labels.Single(l => l.Key.Offset == instruction.Offset).Value);
            }
            else if (operand is Instruction[] instructions)
            {
                var labelParams = string.Join(", ", instructions.Select(i => labels[i]));
                output.Write($"new Label[] {{ {labelParams} }}");
            }
            else if (operand is FieldReference fieldReference)
            {
                output.WriteTypeOf(fieldReference.FieldType);
                output.Write($".GetField(\"{fieldReference.Name}\", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)");
            }
            else if (operand is VariableReference variable)
            {
                output.Write(locals.Single(l => l.Key.Index == variable.Index).Value);
            }
            else if (operand is TypeReference type)
            {
                output.WriteTypeOf(type);
            }
            else if (operand is ParameterReference param)
            {
                output.Write(param.Index.ToString());
            }
            else
            {
                output.WriteLine($"Unsupported parameter: {operand.GetType()}");
            }
        }
    }
}
