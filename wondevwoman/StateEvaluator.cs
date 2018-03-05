using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CG.WondevWoman
{
    public class StateEvaluator : IStateEvaluator
    {
        private ExplainedScore SameAsBot(State state, int playerIndex)
        {
            return 0.1 * state.GetPossibleActions().Count + state.MyUnits.Sum(state.HeightAt);
        }
        private ExplainedScore ScoreAndUnitsDifference(State state, int playerIndex)
        {
            // 4%
            var myScore = state.GetScore(playerIndex);
            var hisScore = state.GetScore(1 - playerIndex);
            var myUnits = state.GetUnits(playerIndex).Count;
            var hisUnits = state.GetUnits(1 - playerIndex).Count;
            return 5.0 * (myScore - hisScore) +  (myUnits - hisUnits);
        }

        private ExplainedScore ScoreAndMovesDifference(State state, int playerIndex)
        {
            //49% 21
            var myScore = state.GetScore(playerIndex);
            var hisScore = state.GetScore(1 - playerIndex);
            var myMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            return 5.0 * (myScore - hisScore) + (myMoves - 2.5*hisMoves);
        }
        private ExplainedScore WithPows(State state, int playerIndex)
        {
           
            var myScore = state.GetScore(playerIndex);
            var averageUnitHeight = state.GetUnits(playerIndex).Average(state.HeightAt);
            var myMoves = state.GetPossibleActions().Count;
            var myUnits4 = state.GetUnits(playerIndex).Count(unit => state.HeightAt(unit) == 4);
            //16%
            return (Math.Pow(myScore, 3)) * 5 + Math.Pow(averageUnitHeight, 4) * 2 + Math.Pow(myMoves, 5) * 1.5 - Math.Pow(myUnits4, 3) * 1.2;

        }
        private ExplainedScore Ulearn(State state, int playerIndex)
        {
            //3%
            var myMoves = state.GetPossibleActions().Count;
            var averageUnitHeight = state.GetUnits(playerIndex).Average(state.HeightAt);

            return 5.0* state.GetScore(playerIndex) + 3* averageUnitHeight + 2.5* (myMoves == 0 ? int.MinValue : myMoves);

        }
//        private ExplainedScore ABC(State state, int playerIndex)
//        {
//            var neighbours = state.GetUnits(playerIndex);
//            var averageUnitHeight = state.GetUnits(playerIndex).Average(state.HeightAt);
//            return 5.0 * state.GetScore(playerIndex) + 3 * averageUnitHeight + 2.5 * (myMoves == 0 ? int.MinValue : myMoves);
//
//        }
        private ExplainedScore Smth(State state, int playerIndex)
        {
            //55% 27
            var score = 0;
            var myMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var myUnitZeroMoves = myMoves.Count(x => x.UnitIndex == 0);
            var myUnitOneMoves = myMoves.Count(x => x.UnitIndex == 1);
            var hisUnitZeroMoves = myMoves.Count(x => x.UnitIndex == 0);
            var hisUnitOneMoves = myMoves.Count(x => x.UnitIndex == 1);
            var averageUnitHeight = state.GetUnits(playerIndex).Average(state.HeightAt);
            var units4 = 0;
            var units2 = 0; 
                for (int i = 0; i < state.Size; i++)
                {
                    for (int j = 0; j < state.Size; j++)
                    {
                        var height = state.HeightAt(i, j);
                        if (height == 4)
                            units4++;
                        if (height == 2)
                            units2++;
                    }
                }
            Console.Error.WriteLine("4 LEVEL " + units4);
            if (myUnitOneMoves == 0 && myUnitZeroMoves == 0)
                return int.MinValue;
//            if (hisUnitOneMoves == 0 || hisUnitZeroMoves == 0)
//                score += 9999;
            return Math.Pow(state.GetScore(playerIndex), 4) * 5 
                   + (myMoves.Count - 2 * hisMoves.Count)
          +  Math.Pow(units2, 4) * 3
            - Math.Pow(units4, 4) * 6;
        }

        private ExplainedScore TryCaptureEnemy(State state, int playerIndex)
        {
            //54% 17
            var myMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            return (myMoves - 3*hisMoves);
        }
        public ExplainedScore Evaluate(State state, int playerIndex)
        {
           // return SameAsBot(state, playerIndex);
          //  return ScoreAndUnitsDifference(state, playerIndex);
           // return ScoreAndMovesDifference(state, playerIndex);
            //return Ulearn(state, playerIndex);
           // return WithPows(state, playerIndex);
           // return WithPows(state, playerIndex);
            return Smth(state, playerIndex);
           // return TryCaptureEnemy(state, playerIndex);
        }

    }
}
