using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILGenerationLanguage.Plugin
{
    public abstract class ExceptionBlock
    {
        public ExceptionBlock(Instruction start, Instruction end)
        {
            this.Start = start;
            this.End = end;
        }
        public Instruction Start { get; set; }
        public Instruction End { get; set; }
    }
    public class TryBlock : ExceptionBlock
    {
        public TryBlock(Instruction start, Instruction end) : base(start, end)
        {
        }
    }
    public class CatchBlock : ExceptionBlock
    {
        public TypeReference ExceptionType { get; set; }
        public CatchBlock(Instruction start, Instruction end, TypeReference exceptionType) : base(start, end)
        {
            this.ExceptionType = exceptionType;
        }
        public FilterBlock FilterBlock { get; set; }
    }
    public class FinallyBlock : ExceptionBlock
    {
        public FinallyBlock(Instruction start, Instruction end) : base(start, end)
        {
        }
    }
    public class FilterBlock : ExceptionBlock
    {
        public FilterBlock(Instruction start, Instruction end) : base(start, end)
        {
        }
    }
    public class ExceptionHandlerBlock
    {
        public TryBlock Try { get; set; }
        public List<CatchBlock> Catch { get; set; }
        public FinallyBlock Finally { get; set; }


        private readonly MethodInstructionComparer methodInstructionComparer;
        public ExceptionHandlerBlock() : this(new MethodInstructionComparer()) { }
        public ExceptionHandlerBlock(MethodInstructionComparer methodInstructionComparer)
        {
            this.methodInstructionComparer = methodInstructionComparer;
        }
        public bool IsTryStart(Instruction instruction) => methodInstructionComparer.Equals(instruction, Try.Start);
        public bool IsTryEnd(Instruction instruction) => methodInstructionComparer.Equals(instruction, Try.End);

        public bool IsFinallyStart(Instruction instruction) => methodInstructionComparer.Equals(instruction, Finally?.Start);
        public bool IsFinallyEnd(Instruction instruction) => methodInstructionComparer.Equals(instruction, Finally?.End);

        public bool IsCatchStart(Instruction instruction) => Catch.Any(c=> methodInstructionComparer.Equals(instruction, c.Start));
        public bool IsCatchEnd(Instruction instruction) => Catch.Any(c => methodInstructionComparer.Equals(instruction, c.End));

    }    
}
