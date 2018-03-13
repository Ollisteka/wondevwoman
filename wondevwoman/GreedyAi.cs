using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class GreedyAi
    {
        private readonly IStateEvaluator evaluator;

        public GreedyAi(IStateEvaluator evaluator)
        {
            this.evaluator = evaluator;
        }

        public IGameAction GetAction(State state, Countdown countdown)
        {
            var actions = state.GetPossibleActions();
            foreach (var action in actions)
            {
                if (countdown.IsFinished) break;
                action.Score = Evaluate(state, action, countdown);
                
            }
            return actions.MaxBy(a => a.Score ?? double.NegativeInfinity);
        }

        private ExplainedScore Evaluate(State state, IGameAction action, Countdown countdown, int playerIndex=0)
        {
            using (action.ApplyTo(state))
            {
                //if (playerIndex == 0)
                //{
                //    var bestEnemyAction = FindBestEnemyAction(state, countdown);
                //    if (bestEnemyAction != null)
                //        using (bestEnemyAction.ApplyTo(state))
                //        {
                            var movedCellLocation = state.GetUnits(playerIndex)[action.UnitIndex];
                            return evaluator.Evaluate(state, playerIndex, movedCellLocation);
                        //}

                //    {
                //        var movedCellLocation = state.GetUnits(playerIndex)[action.UnitIndex];
                //        return evaluator.Evaluate(state, playerIndex, movedCellLocation);
                //    }
                //}
                //else
                //{
                //    var movedCellLocation = state.GetUnits(playerIndex)[action.UnitIndex];
                //    return evaluator.Evaluate(state, playerIndex, movedCellLocation);
                //}
            }
        }

        private static IGameAction FindBestEnemyAction(State state, Countdown countdown)
        {
            //state.ChangeCurrentPlayer();
            var moves = state.GetPossibleActions();
           // state.ChangeCurrentPlayer();
            if (moves.Count == 0 || moves.First() is AcceptDefeatAction)
            return null;
            foreach (var action in moves)
            {
                if (countdown.IsFinished) break;
                action.Score = StateEvaluator.TryCaptureEnemy(state, 1);
            }
            return moves.MaxBy(a => a.Score ?? double.NegativeInfinity);
        }
    }
}
