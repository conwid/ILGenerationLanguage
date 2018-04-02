using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ICSharpCode.Decompiler;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace ILGenerationLanguage.Plugin
{
    public sealed class GeneratingDisassembler
    {
        private readonly ITextOutput output;
        private CancellationToken cancellationToken;
        private GeneratingMethodBodyDisassembler methodBodyDisassembler;

        public GeneratingDisassembler(ITextOutput output, CancellationToken cancellationToken)
            : this(output, new GeneratingMethodBodyDisassembler(output, cancellationToken), cancellationToken)
        {
        }

        public GeneratingDisassembler(ITextOutput output, GeneratingMethodBodyDisassembler methodBodyDisassembler, CancellationToken cancellationToken)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            this.cancellationToken = cancellationToken;
            this.methodBodyDisassembler = methodBodyDisassembler;
        }

        public void DisassembleMethod(MethodDefinition method)
        {
            DisassembleMethodHeader(method);
            DisassembleMethodBlock(method);
        }

        public void DisassembleMethodHeader(MethodDefinition method)
        {            
            DisassembleMethodHeaderInternal(method);
        }

        private void DisassembleMethodHeaderInternal(MethodDefinition method)
        {
            List<string> methodAttributes = new List<string>();
            methodAttributes.Add("MethodAttributes.HideBySig");
            if (method.IsPublic)
            {
                methodAttributes.Add("MethodAttributes.Public");
            }
            if (method.IsPrivate)
            {
                methodAttributes.Add("MethodAttributes.Private");
            }
            if (method.IsVirtual)
            {
                methodAttributes.Add("MethodAttributes.Virtual");
            }
            if (method.IsStatic)
            {
                methodAttributes.Add("MethodAttributes.Static");
            }
            var returnType = method.ReturnType.MetadataType == MetadataType.Void ? "typeof(void)" : method.ReturnType.ToTypeOf();
            var parameterTypes = method.HasParameters ? $"new Type[] {{ {string.Join(" ,", method.Parameters.Select(p => $"{p.ParameterType.ToTypeOf()}"))} }}" : "Type.EmptyTypes";
            output.WriteLine($"var methodBuilder = typeBuilder.DefineMethod({method.Name},{ string.Join(" | ", methodAttributes.Select(s => $"MethodAttributes.{s.ToString()}"))}, {returnType},{parameterTypes} )");
        }

        private void DisassembleMethodBlock(MethodDefinition method)
        {
            if (method.HasBody)
            {
                methodBodyDisassembler.Disassemble(method.Body);
            }

        }
    }
}
