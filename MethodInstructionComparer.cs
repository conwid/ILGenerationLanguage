using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILGenerationLanguage.Plugin
{
    public class MethodInstructionComparer : IEqualityComparer<Instruction>
    {
        public bool Equals(Instruction x, Instruction y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            if (x.OpCode == y.OpCode && x.Offset == y.Offset)
                return true;

            return false;
        }

        public int GetHashCode(Instruction obj)
        {
            if (obj == null)
                throw new ArgumentException(nameof(obj));
            return (17 * 23 + obj.Offset.GetHashCode()) * 23 + obj.OpCode.GetHashCode();
        }
    }
}
