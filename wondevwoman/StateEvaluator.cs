using System;
using System.Collections.Generic;
using System.Linq;

namespace CG.WondevWoman
{
    public class StateEvaluator : IStateEvaluator
    {
        public ExplainedScore Evaluate(State state, int playerIndex, Vec movedCellLocation)
        {
//            var myLoc = new List<Vec>
//            {
//                state.GetUnits(playerIndex)[0],
//                state.GetUnits(playerIndex)[1]
//            };
//            var hisLoc = new List<Vec>
//            {
//                state.GetUnits(1-playerIndex)[0],
//                state.GetUnits(1-playerIndex)[1]
//            };
            //Console.Error.WriteLine($"ME0 {myLoc[0]}  ME1 {myLoc[1]} HE0 {hisLoc[0]}  HE1 {hisLoc[1]}");
            //var myPlayers = GetPlayersLocations(state, playerIndex).Select(x => new Unit(x, playerIndex));
           // var hisPlayers = GetPlayersLocations(state, 1 - playerIndex).Select(x => new Unit(x, 1 - playerIndex));
           // return GetVoronoiAreasCount(state, playerIndex, myPlayers.Concat(hisPlayers).ToList());
           // return GetDoublecheckedVoronoiScore(state, playerIndex, GetVoronoiAreasValues);
             return MyPowsMinusHisPows(state, playerIndex, movedCellLocation);
           // return Agade(state, playerIndex);
        }

        private ExplainedScore MyPowsMinusHisPows(State state, int playerIndex, Vec movedCellLocation)
        {
            //123
            var myLocations = GetPlayersLocations(state, playerIndex);
            var hisLocation = GetPlayersLocations(state, 1 - playerIndex);
            var hisPows = 0.0;
            foreach (var unit in hisLocation)
                hisPows += WithPows(state, 1 - playerIndex, unit).Value;
            var myPows = 0.0;
            foreach (var unit in myLocations)
                myPows += WithPows(state, playerIndex, unit).Value;
            return myPows - hisPows;
        }

        private ExplainedScore Agade(State state, int playerIndex)
        {
            //209
            var voronoi = GetDoublecheckedVoronoiScore(state, playerIndex, GetVoronoiAreasValues);
            var scoresDiff = state.GetScore(playerIndex) - state.GetScore(1 - playerIndex);
            var myNeighbors = GetScoreForNeighbours(state, playerIndex);
            var hisNeighbours = GetScoreForNeighbours(state, 1 - playerIndex);
            return 2 * voronoi + scoresDiff + (myNeighbors - hisNeighbours);
        }

        private static double GetScoreForNeighbours(State state, int playerIndex)
        {
            var myNeighbors = 0;
            foreach (var point in GetPlayersLocations(state, playerIndex))
            {
                var passableNeighbours = GetPassableNeighbours(state, point).Count();
                if (passableNeighbours == 0)
                    myNeighbors -= 999;
                else myNeighbors += passableNeighbours;
            }

            return myNeighbors;
        }

        public static List<Vec> GetPlayersLocations(State state, int playerIndex)
        {
            return new List<Vec>
            {
                state.GetUnits(playerIndex)[0],
                state.GetUnits(playerIndex)[1]
            }.Where(p => !p.Equals(new Vec(-1, -1))).ToList();
        }


        private List<Unit> GetUnitsList(State state, int playerIndex)
        {
            var players = new List<Unit>();
            foreach (var myUnit in GetPlayersLocations(state, playerIndex))
                players.Add(new Unit(myUnit, playerIndex));
            foreach (var hisUnit in GetPlayersLocations(state, 1 - playerIndex))
                players.Add(new Unit(hisUnit, 1 - playerIndex));
            return players;
        }

        private static double GetDoublecheckedVoronoiScore(State state, int playerIndex,
            Func<State, int, List<Unit>, double> getVoronoiScore)
        {
            var myPlayers = GetPlayersLocations(state, playerIndex).Select(x => new Unit(x, playerIndex));
            var hisPlayers = GetPlayersLocations(state, 1 - playerIndex).Select(x => new Unit(x, 1 - playerIndex));
            var players = myPlayers.Concat(hisPlayers).ToList();
            var meHis = getVoronoiScore(state, playerIndex, players);
            players.Reverse();
            var hisMe = getVoronoiScore(state, 1 - playerIndex, players);
            return meHis + hisMe;
        }

        private double GetVoronoiAreasCount(State state, int playerIndex, List<Unit> players)
        {
            var allSigned = AssignOwners(state, players).ToList();
            var my = allSigned.Count(x => x.Owner == playerIndex);;
            var his = allSigned.Count(x => x.Owner != playerIndex);
            return my - his;
        }

        private double GetVoronoiAreasValues(State state, int playerIndex, List<Unit> players)
        {
            var allSigned = AssignOwners(state, players).ToList();
            var my = allSigned.Where(x => x.Owner == playerIndex).Sum(x => x.Value);
            var his = allSigned.Where(x => x.Owner != playerIndex).Sum(x => x.Value);
            return my - his;
        }

        public static IEnumerable<OwnedLocation> AssignOwners(State state, List<Unit> players)
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
                    var val = EstimatePointValue(state, track[point].Owner, nextPoint) / distance;
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

        public static IEnumerable<Vec> GetPassableNeighbours(State state, Vec point)
        {
            return GetIncidentPoints(point).Where(p => p.InArea(state.Size) && state.CanMove(point, p));
        }

        private static double EstimatePointValue(State state, int playerIndex, Vec pointLocation)
        {
            return WithPows(state, playerIndex, pointLocation).Value;
        }

        private static ExplainedScore WithPows(State state, int playerIndex, Vec myLocation)
        {
            var malus = 0.0;
            var bonus = 0.0;

            var neigbours = GetIncidentPoints(myLocation).Where(p => p.InArea(state.Size)).ToList();
            var neighboursHeight = neigbours.Average(state.HeightAt);
            var allUnitHeight = state.GetUnits(playerIndex).Where(p => p.InArea(state.Size) && state.HeightAt(p) < 4).Average(state.HeightAt);
            var unit3 = state.GetUnits(playerIndex).Where(p => p.InArea(state.Size)).Count(p => state.HeightAt(p) == 3);

            var freeCells = neigbours.Count(p => state.CanMove(myLocation, p));
            if (freeCells == 0)
                malus -= 9900;
            var myScore = state.GetScore(playerIndex);
            var currentHeight = state.HeightAt(myLocation);
            if (currentHeight == 3)
                bonus += 9999;
            
            var score = EvalPow(myScore, 2, 20) + EvalPow(unit3, 2.5, 3.5) + EvalPow(allUnitHeight, 1.7, 1.7) + malus +
                        bonus;
          //  Console.Error.WriteLine($"MyLoc {myLocation} Points {myScore} Neighbour {neigbours.Count} Free {freeCells} SCORE {score}");
            return score;
        }

        private static double EvalPow(double count, double evalSpeed, double factor)
        {
            return Math.Pow(count, evalSpeed) * factor;
        }
        private ExplainedScore CaptureAndHeights(State state, int playerIndex, Vec myLocation)
        {
            //57% 21
            var hisMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var myMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var myUnitZeroMoves = myMoves.Count(x => x.UnitIndex == 0);
            var myUnitOneMoves = myMoves.Count(x => x.UnitIndex == 1);
            var hisUnitZeroMoves = hisMoves.Count(x => x.UnitIndex == 0);
            var hisUnitOneMoves = hisMoves.Count(x => x.UnitIndex == 1);
            var units4 = 0;
            var units2 = state.GetUnits(playerIndex).Count(unit => state.HeightAt(unit) == 2);
            for (var i = 0; i < state.Size; i++)
            for (var j = 0; j < state.Size; j++)
            {
                var height = state.HeightAt(i, j);
                if (height == 4)
                    units4++;
            }

            var malus = 0;
            var bonus = 0;
            if (myUnitOneMoves == 0 || myUnitZeroMoves == 0)
                malus -=9999;
            if (hisUnitOneMoves == 0 || hisUnitZeroMoves == 0 || state.HeightAt(myLocation) == 3)
                bonus += 9999;
          
            return Math.Pow(state.GetScore(playerIndex), 4) * 5
                   + (myMoves.Count - 3 * hisMoves.Count)
                   + Math.Pow(units2, 4) * 3
                   - Math.Pow(units4, 4) * 6 + malus + bonus;
        }

        private ExplainedScore TryCaptureEnemy(State state, int playerIndex)
        {
            //53% 26
            var hisMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            var myMoves = state.GetPossibleActions().Count;
            state.ChangeCurrentPlayer();
            return myMoves - 3 * hisMoves;
        }

        private double ClassifyWalls(State state, Vec myLocation)
        {
            /*hole: cell with height -1 or 4
wall: cell at least two steps higher
drop: cell at least two steps lower
stairs: cell exactly one step higher
floor: cell at the same height
goal: cell with height 3, aka a scoring opportunity.*/

            var sameFloor = 0;
            var stairs = 0;
            var drop = 0;
            var wall = 0;
            var goal = 0;
            var hole = 0;
            var myHeight = state.HeightAt(myLocation);
            foreach (var point in GetIncidentPoints(myLocation).Where(p => p.InArea(state.Size)))
            {
                var height = state.HeightAt(point);
                if (height - myHeight >= 2)
                    wall++;
                if (myHeight - height <= 2)
                    drop++;
                if (height - myHeight == 1)
                    stairs++;
                if (height == myHeight)
                    sameFloor++;
                if (height == 3)
                    goal++;
                if (height == 4 || height == -1)
                    hole++;
            }

            var score = 0.0;
            score -= 9 * hole;
            //  score -= 9 * wall;
            score -= 2.5 * drop;
            score += 3.3 * stairs;
            score += 5.7 * sameFloor;
            score += 7.8 * goal;

            if (state.HeightAt(myLocation) == 3)
                score += 999;
            return score;
        }
    }
}