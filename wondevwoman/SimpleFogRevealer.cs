using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class SimpleFogRevealer
    {
        private State prevState;
        private IGameAction prevAction;

        public void ConsiderStateBeforeMove(State state, Countdown countdown)
        {
            // countdown нужен на будущее, когда захочется делать тут что-то тяжёлое.
            if (prevState != null)
            {
                prevAction.ApplyTo(prevState);
                RevealFromFog(state, prevState);
            }
            prevState = state.MakeCopy();
        }
        
        private static void RevealFromFog(State state, State prevState)
        {
            // Считает, что невидимые юниты остаются там, где их видели последний раз,
            // если там вообще можно стоять.
            for (int i = 0; i < state.HisUnits.Count; i++)
                if (state.HisUnits[i].X < 0)
                {
                    var prevLoc = prevState.MyUnits[i];
                    if (prevLoc.X >= 0 && !state.MyUnits.Any(u => u.IsNear8To(prevLoc)))
                    {
                      //  var newLoc = FindBestMovement(state, prevLoc);
                        state.MoveUnit(1 - state.CurrentPlayer, i, prevLoc);
                    }
                }
                else if (!state.IsPassableHeightAt(state.HisUnits[i]))
                    state.MoveUnit(1-state.CurrentPlayer, i, new Vec(-1, -1));
        }

        private static Vec FindBestMovement(State state, Vec prevLocation)
        {
            var none = new Vec(-1,-1);
            var currentPlayerLoc = StateEvaluator.GetPlayersLocations(state, state.CurrentPlayer);
            var neighs =  StateEvaluator.GetPassableNeighbours(state, prevLocation)
                .Where(p => p.DistTo(currentPlayerLoc[0]) > 1 && p.DistTo(currentPlayerLoc[1]) > 1).MaxBy(p => state.HeightAt(p));
            return neighs ?? none;
        }

        public void RegisterAction(IGameAction action)
        {
            prevAction = action;
        }
    }
}
