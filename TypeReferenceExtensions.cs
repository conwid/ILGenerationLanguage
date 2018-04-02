using Mono.Cecil;

namespace ILGenerationLanguage.Plugin
{
    public static class TypeReferenceExtensions
    {
        public static string ToTypeOf(this TypeReference typeReference)
        {
            return $"typeof({typeReference.ToClrType()})";
        }
        public static string ToClrType(this TypeReference typeReference)
        {
            return typeReference.ToString();
        }
    }
}