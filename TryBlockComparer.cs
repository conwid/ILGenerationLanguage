using System.Collections.Generic;

namespace ILGenerationLanguage.Plugin
{
    public class TryBlockComparer : IEqualityComparer<TryBlock>
    {
        private readonly MethodInstructionComparer methodInstructionComparer;
        public TryBlockComparer(MethodInstructionComparer methodInstructionComparer)
        {
            this.methodInstructionComparer = methodInstructionComparer;
        }
        public bool Equals(TryBlock x, TryBlock y)
        {
            return methodInstructionComparer.Equals(x.Start, y.Start) && methodInstructionComparer.Equals(x.End, y.End);
        }

        public int GetHashCode(TryBlock obj)
        {
            return (17 * 23 + methodInstructionComparer.GetHashCode(obj.Start)) * 23 + methodInstructionComparer.GetHashCode(obj.End);
        }
    }
}
