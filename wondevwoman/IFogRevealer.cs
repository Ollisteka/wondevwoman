namespace CG.WondevWoman
{
    public interface IFogRevealer
    {
        void ConsiderStateBeforeMove(State state, Countdown countdown);
        void RegisterAction(IGameAction action);
    }
}