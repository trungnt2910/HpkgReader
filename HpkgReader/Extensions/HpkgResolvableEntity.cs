using HpkgReader.Model;

namespace HpkgReader.Extensions
{
    public class HpkgResolvableEntity
    {
        private static readonly string[] _ops = { "<", "<=", "==", "!=", ">=", ">" }; 

        public string Name { get; set; }
        public HpkgResolvableOperator? ResolvableOperator { get; set; }
        public BetterPkgVersion Version { get; set; }

        public override string ToString()
        {
            string result = Name;
            
            if (Version != null)
            {
                result += $" {_ops[(int)ResolvableOperator]} {Version}";
            }

            return result;
        }

        internal BetterAttribute ToAttribute(AttributeId id)
        {
            return new BetterAttribute(id, Name)
                .GuessTypeAndEncoding()
                .WithChildIfNotNull(Version, (v) => new BetterAttribute(v))
                .WithChildIfNotNull(ResolvableOperator, (v) =>
                        new BetterAttribute(AttributeId.PACKAGE_RESOLVABLE_OPERATOR, (uint)ResolvableOperator)
                            .GuessTypeAndEncoding()
                            );
        }
    }
}
