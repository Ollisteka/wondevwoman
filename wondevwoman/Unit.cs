namespace CG.WondevWoman
{
    public class Unit
    {
        public Vec Location;
        public int PlayerIndex;

        public Unit(Vec location, int playerIndex)
        {
            Location = location;
            PlayerIndex = playerIndex;
        }
    }
}