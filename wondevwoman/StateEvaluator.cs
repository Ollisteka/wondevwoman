using System;
using System.Collections.Generic;
using System.Linq;

namespace CG.WondevWoman
{
    public class StateEvaluator : IStateEvaluator
    {
        public static double SCORE_COEF = 1;
        public static double ACTIONS_COEF = 0.1;
        public static double VORONOI_COEF = 1;

        public ExplainedScore Evaluate(State state, int playerIndex, Vec movedCellLocation)
        {
            
            return GetDoublecheckedVoronoiScore(state, playerIndex, GetVoronoiAreasCount);
        }

        public static ExplainedScore GetDoublecheckedVoronoiScore(State state, int playerIndex,
            Func<State, int, List<PlayerPiece>, ExplainedScore> getVoronoiScore)
        {
            var myPlayers = GetPlayerPieces(state, playerIndex);
            var hisPlayers = GetPlayerPieces(state, 1 - playerIndex);
            var players = myPlayers.Concat(hisPlayers).ToList();
            var meHis = getVoronoiScore(state, playerIndex, players);
            players.Reverse();
            var hisMe = getVoronoiScore(state, 1 - playerIndex, players);
            return new ExplainedScore(meHis.Value - hisMe.Value,
                $"ME-HIM {meHis.Explanation} HIM-ME {hisMe.Explanation}");
        }

        public static ExplainedScore GetVoronoiAreasCount(State state, int playerIndex, List<PlayerPiece> players)
        {
            var allSigned = AssignOwners(state, players).ToList();
            var myPlayers = players.Count(p => p.PlayerIndex == playerIndex);
            var hisPlayers = players.Count(p => p.PlayerIndex != playerIndex);
            var my = allSigned.Count(x => x.Owner == playerIndex) - myPlayers;
            var his = allSigned.Count(x => x.Owner != playerIndex) - hisPlayers;
            return new ExplainedScore(my - his, $"P{playerIndex}: {my}  P{1 - playerIndex}: {his}");
        }

        private static ExplainedScore GetVoronoiAreasValues(State state, int playerIndex, List<PlayerPiece> players)
        {
            var allSigned = AssignOwners(state, players).ToList();
            var my = allSigned.Where(x => x.Owner == playerIndex).Sum(x => x.Value);
            var his = allSigned.Where(x => x.Owner != playerIndex).Sum(x => x.Value);
            return my - his;
        }

        public static IEnumerable<OwnedLocation> AssignOwners(State state, List<PlayerPiece> players)
        {
            var track = new Dictionary<Vec, OwnedLocation>();
            var queue = new Queue<Vec>();
            foreach (var player in players)
            {
                if (!player.Location.InArea(state.Size))
                    continue;
                track[player.Location] = new OwnedLocation(player.PlayerIndex, player.Location, 0, 0);
                yield return track[player.Location];
                queue.Enqueue(player.Location);
            }


            while (queue.Count != 0)
            {
                var point = queue.Dequeue();
                if (!point.InArea(state.Size))
                    continue;
                foreach (var nextPoint in GetIncidentPoints(point))
                {
                    //Console.Error.WriteLine($"{point} -> {nextPoint}");
                    if (track.ContainsKey(nextPoint) || !nextPoint.InArea(state.Size) ||
                        !state.CanMove(point, nextPoint)) continue;
                    var distance = track[point].Distance + 1;
                    var val = state.HeightAt(nextPoint);
                    //   var val = EvaluteForPoint(state, track[point].Owner, nextPoint).Value / distance;
                    track[nextPoint] = new OwnedLocation(track[point].Owner, nextPoint, val, distance);
                    yield return track[nextPoint];
                    queue.Enqueue(nextPoint);
                }
            }
        }

        private static IEnumerable<Vec> GetIncidentPoints(Vec point)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                yield return point + dir;
        }

        public static IEnumerable<Vec> GetPassableNeighbours(State state, Vec initialLocation)
        {
            return GetIncidentPoints(initialLocation)
                .Where(p => p.InArea(state.Size) && state.CanMove(initialLocation, p));
        }

        #region notvoronoi

        private static ExplainedScore WithPows(State state, int playerIndex, Vec myLocation)
        {
            var malus = 0.0;
            var bonus = 0.0;

            var neigbours = GetIncidentPoints(myLocation).Where(p => p.InArea(state.Size)).ToList();
            // var neighboursHeight = neigbours.Average(state.HeightAt);
            var allUnitHeight = state.GetUnits(playerIndex).Where(p => p.InArea(state.Size) && state.HeightAt(p) < 4)
                .Average(state.HeightAt);
            var unit3 = state.GetUnits(playerIndex).Where(p => p.InArea(state.Size)).Count(p => state.HeightAt(p) == 3);

            var freeCells = neigbours.Count(p => state.CanMove(myLocation, p));
            if (freeCells == 0)
                malus -= 9900;
            var myScore = state.GetScore(playerIndex);
            var currentHeight = state.HeightAt(myLocation);
            if (currentHeight == 3)
                bonus += 9999;

            state.ChangeCurrentPlayer();
            var moves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            var score = EvalPow(myScore, 2, 20)
                        + EvalPow(unit3, 2.5, 3.5)
                        + EvalPow(allUnitHeight, 1.7, 1.7)
                        //  + EvalPow(neighboursHeight, 1.2, 0.7)
                        + EvalPow(moves, 1, 23)
                        + malus +
                        bonus;
            //Console.Error.WriteLine($"MyLoc {myLocation} Points {myScore} Neighbour {neigbours.Count} Free {freeCells} SCORE {score}");
            return score;
        }

        private static double EvalPow(double count, double evalSpeed, double factor)
        {
            return Math.Pow(count, evalSpeed) * factor;
        }

        private ExplainedScore MyPowsMinusHisPows(State state, int playerIndex, Vec movedCellLocation)
        {
            //123
            // var myLocations = GetPlayersLocations(state, playerIndex);
            var hisLocation = GetPlayersLocations(state, 1 - playerIndex);
            var hisPows = 0.0;
            var upper = hisLocation.Count >= 1 ? 1 : 0;
            for (int i = 0; i < upper; i++)
            {
                hisPows += WithPows(state, 1 - playerIndex, hisLocation[0]).Value;
            }

            var myPows = 0.0;
            //foreach (var unit in myLocations)
            myPows += WithPows(state, playerIndex, movedCellLocation).Value;
            return myPows - hisPows;
        }

        #endregion

        public static ExplainedScore TryCaptureEnemy(State state, int playerIndex)
        {
            //53% 26
            var hisMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            var myMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            return myMoves - 3 * hisMoves;
        }

        public static ExplainedScore PractiseEvaluate(State state, int playerIndex)
        {
            state.ChangeCurrentPlayer();
            var actions = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            if (actions.Count == 0 || actions.First() is AcceptDefeatAction)
                return state.GetScore(playerIndex) * SCORE_COEF;

            var myPlayers = GetPlayerPieces(state, playerIndex);
            var hisPlayers = GetPlayerPieces(state, 1 - playerIndex);

            var players = myPlayers.Concat(hisPlayers).ToList();

            return state.GetScore(playerIndex) * SCORE_COEF
                   + actions.Count * ACTIONS_COEF
                   + GetDoublecheckedVoronoiScore(state, playerIndex, GetVoronoiAreasValues).Value *
                   VORONOI_COEF; //сумма высот
        }

        public static List<Vec> GetPlayersLocations(State state, int playerIndex)
        {
            return new List<Vec>
            {
                state.GetUnits(playerIndex)[0],
                state.GetUnits(playerIndex)[1]
            }.Where(p => !p.Equals(new Vec(-1, -1))).ToList();
        }

        public static List<PlayerPiece> GetPlayerPieces(State state, int playerIndex)
        {
            return GetPlayersLocations(state, playerIndex).Select(x => new PlayerPiece(x, playerIndex)).ToList();
        }
    }
}