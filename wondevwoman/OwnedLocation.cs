namespace CG.WondevWoman
{
    public class OwnedLocation
    {
        public readonly int Owner;
        public readonly Vec Location;
        public readonly int Distance;
        public readonly double Value;

        public OwnedLocation(int owner, Vec location, double value, int distance)
        {
            Owner = owner;
            Location = location;
            Value = value;
            Distance = distance;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is OwnedLocation))
                return false;
            var other = (OwnedLocation) obj;
            return Owner.Equals(other.Owner) && Location.Equals(other.Location) && Value.Equals(other.Value) && Distance.Equals(other.Distance);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Owner;
                hashCode = (hashCode * 397) ^ Location.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Value;
                hashCode = (hashCode * 397) ^ Distance;
                return hashCode;
            }
        }


        public override string ToString()
        {
            return $"[Location: {Location}, Owner: {Owner}, Distance: {Distance}, Value: {Value}]";
        }
    }
}