using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class StateEvaluator : IStateEvaluator
    {
        public ExplainedScore Evaluate(State state, int playerIndex, Vec movedCellLocation)
        {
            // return SameAsBot(state, playerIndex);
            // return ScoreAndUnitsDifference(state, playerIndex);
             //return ScoreAndMovesDifference(state, playerIndex);
            //return Ulearn(state, playerIndex);
            return WithPows(state, playerIndex, movedCellLocation);
            // return WithPows(state, playerIndex);
            //return CaptureAndHeights(state, playerIndex);
            //return TryCaptureEnemy(state, playerIndex);
            //return Smth(state, playerIndex);
        }
        private ExplainedScore Smth(State state, int playerIndex)
        {
 
            var myMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var myUnitZeroMoves = myMoves.Count(x => x.UnitIndex == 0);
            var myUnitOneMoves = myMoves.Count(x => x.UnitIndex == 1);
            var units4 = 0;
            for (var i = 0; i < state.Size; i++)
            for (var j = 0; j < state.Size; j++)
            {
                var height = state.HeightAt(i, j);
                if (height == 4)
                    units4++;
            }

            if (myUnitOneMoves == 0 && myUnitZeroMoves == 0)
                return int.MinValue;
            //            if (hisUnitOneMoves == 0 || hisUnitZeroMoves == 0)
            //                score += 9999;
            return //Math.Pow(state.GetScore(playerIndex), 4) * 5
                   + (myMoves.Count - 3 * hisMoves.Count)
                   - Math.Pow(units4, 4) * 6;
        }
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
            return 5.0 * (myScore - hisScore) + (myUnits - 3*hisUnits);
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
            return 5.0 * (myScore - hisScore) + (myMoves - 2.5 * hisMoves);
        }

        private ExplainedScore WithPows(State state, int playerIndex, Vec myLocation)
        {
            var myScore = state.GetScore(playerIndex);
            var myUnits = state.GetUnits(playerIndex);
            var averageUnitHeight = myUnits.Average(state.HeightAt);
            var myMoves = state.GetPossibleActions();
            if (myMoves.Count == 0)
                return int.MinValue;

            var neighboursHeight = myUnits.Where(x => x.IsNear8To(myLocation)).Sum(state.HeightAt);
            var malus = 0.0;
            var freeCells = 0;
            var unit3 = 0;
            var unit4 = 0;
            for (int i = myLocation.X-1; i <= myLocation.X + 1; i++)
            {
                for (int j = myLocation.Y - 1; j <= myLocation.Y + 1; j++)
                {
                    if (i < 0 || j < 0 || i >= state.Size || j >= state.Size)
                        continue;
                    if (state.HeightAt(i, j) == 0 && state.CanMove(myLocation, new Vec(i, j)))
                        freeCells++;
                    if (state.HeightAt(i, j) == 3 && state.CanMove(myLocation, new Vec(i, j)))
                        unit3++;
                    if (state.HeightAt(i, j) == 4)
                        unit4++;
                }
            }

            var bonus = 0;
            if (freeCells == 0)
                malus -= 9999;
            malus -= Math.Pow(unit4, 3) * 3;
            if (state.HeightAt(myLocation) == 3)
                bonus += 9999;



            //            var score = Math.Pow(myScore, 3) * 115 
            //                        + Math.Pow(averageUnitHeight, 3) * 2 
            //                        + Math.Pow(neighboursHeight, 5) * 3.5 
            //                        + Math.Pow(freeCells, 4) * 4.3 + malus;
            var score = Math.Pow(myScore, 3) * 5 
                        + Math.Pow(averageUnitHeight, 2) * 2
                        + Math.Pow(neighboursHeight, 2.5) * 3.5 
                        + Math.Pow(unit3, 4) * 7
                        + malus + bonus;

         //   Console.Error.WriteLine($"MyLoc {myLocation} Points {myScore} Average {averageUnitHeight} Neighbour {neighboursHeight} Free {freeCells} SCORE {score}");
           
            return score;
        }

        private ExplainedScore Ulearn(State state, int playerIndex)
        {
            //3%
            var myMoves = state.GetPossibleActions().Count;
            var averageUnitHeight = state.GetUnits(playerIndex).Average(state.HeightAt);

            return 5.0 * state.GetScore(playerIndex) + 3 * averageUnitHeight +
                   2.5 * (myMoves == 0 ? int.MinValue : myMoves);
        }

        private ExplainedScore CaptureAndHeights(State state, int playerIndex)
        {
            //57% 21
            var myMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var myUnitZeroMoves = myMoves.Count(x => x.UnitIndex == 0);
            var myUnitOneMoves = myMoves.Count(x => x.UnitIndex == 1);
            var units4 = 0;
            var units2 = state.GetUnits(playerIndex).Count(unit => state.HeightAt(unit) == 2);
            for (var i = 0; i < state.Size; i++)
            for (var j = 0; j < state.Size; j++)
            {
                var height = state.HeightAt(i, j);
                if (height == 4)
                    units4++;
            }

            if (myUnitOneMoves == 0 && myUnitZeroMoves == 0)
                return int.MinValue;
            //            if (hisUnitOneMoves == 0 || hisUnitZeroMoves == 0)
            //                score += 9999;
            return Math.Pow(state.GetScore(playerIndex), 4) * 5
                   + (myMoves.Count - 3 * hisMoves.Count)
                   + Math.Pow(units2, 4) * 3
                   - Math.Pow(units4, 4) * 6;
        }

        private ExplainedScore TryCaptureEnemy(State state, int playerIndex)
        {
            //53% 26
            var myMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            return myMoves - 3 * hisMoves;
        }
    }
}