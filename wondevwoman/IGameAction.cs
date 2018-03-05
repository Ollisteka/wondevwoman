namespace CG.WondevWoman
{
    public interface IGameAction
    {
        string Message { get; set; }
        int UnitIndex { get; }
        ExplainedScore Score { get; set; }
        Cancelable ApplyTo(State state);
    }
}
