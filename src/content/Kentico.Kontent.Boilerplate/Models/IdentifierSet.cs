using System;

namespace Kentico.Kontent.Boilerplate.Models
{
    public class IdentifierSet : IEquatable<IdentifierSet>
    {
        public string Type { get; set; }
        public string Codename { get; set; }

        public bool Equals(IdentifierSet other)
        {
            if (other != null && ReferenceEquals(this, other))
            {
                return true;
            }

            return Type.Equals(other.Type, StringComparison.Ordinal) && Codename.Equals(other.Codename, StringComparison.Ordinal);
        }
    }
}
