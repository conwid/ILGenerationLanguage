using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using Mono.Cecil;
using System.ComponentModel.Composition;
using ICSharpCode.ILSpy;

namespace ILGenerationLanguage.Plugin
{
    [Export(typeof(Language))]
    public class ILGenerationLanguage : Language
    {
        protected bool detectControlStructure = true;

        public override string Name
        {
            get { return "IL Generation"; }
        }

        public override string FileExtension
        {
            get { return ".cs"; }
        }

       
        public override void DecompileMethod(MethodDefinition method, ITextOutput output, DecompilationOptions options)
        {
            var disassembler = new GeneratingDisassembler(output, options.CancellationToken);
            disassembler.DisassembleMethod(method);
        }

        public override void DecompileField(FieldDefinition field, ITextOutput output, DecompilationOptions options)
        {
            output.WriteUnsupported();          
        }

        public override void DecompileProperty(PropertyDefinition property, ITextOutput output, DecompilationOptions options)
        {
            output.WriteUnsupported();
        }

        public override void DecompileEvent(EventDefinition ev, ITextOutput output, DecompilationOptions options)
        {
            output.WriteUnsupported();
        }

        public override void DecompileType(TypeDefinition type, ITextOutput output, DecompilationOptions options)
        {
            output.WriteUnsupported();
        }

        public override void DecompileNamespace(string nameSpace, IEnumerable<TypeDefinition> types, ITextOutput output, DecompilationOptions options)
        {
            output.WriteUnsupported();
        }

        public override void DecompileAssembly(LoadedAssembly assembly, ITextOutput output, DecompilationOptions options)
        {
            output.WriteUnsupported();
        }
    }
}
