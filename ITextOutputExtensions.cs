using ICSharpCode.Decompiler;
using ICSharpCode.ILSpy;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ILGenerationLanguage.Plugin
{
    public static class ITextOutputExtensions
    {
        public static void WriteTypeOf(this ITextOutput output, TypeReference typeReference)
        {
            output.Write(typeReference.ToTypeOf());
        }
        public static void WriteUnsupported(this ITextOutput output)
        {
            var smartTextOutput = output as ISmartTextOutput;
            output.WriteLine("Disassembling this element is currently unsupported by this plugin.");
            output.WriteLine("Please visit https://github.com/conwid/ILGenerationLanguage and create a new issue or vote for an existing one.");
            output.WriteLine("Or, if you feel like it, go ahead and create a pull request!");
        }
    }
}