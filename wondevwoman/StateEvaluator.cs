using System;
using System.Collections.Generic;
using System.Linq;

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

    public class StateEvaluator : IStateEvaluator
    {
        public ExplainedScore Evaluate(State state, int playerIndex, Vec movedCellLocation)
        {
            // return SameAsBot(state, playerIndex);
            // return ScoreAndUnitsDifference(state, playerIndex);
            //return ScoreAndMovesDifference(state, playerIndex);
           // return WithPows(state, playerIndex, movedCellLocation);
            //return CaptureAndHeights(state, playerIndex);
            //return TryCaptureEnemy(state, playerIndex);
            // return Combine(state, playerIndex, movedCellLocation);
            //return GetDoubleVoronoi(state, playerIndex, movedCellLocation);
            // return WithPows(state, playerIndex, movedCellLocation).Value + TryBFS(state, playerIndex, movedCellLocation).Value*5;
            //return NormalVoronoiDouble(state, playerIndex, movedCellLocation);
           //return Agade(state, playerIndex, movedCellLocation);
            return Smth(state, playerIndex, movedCellLocation);
        }

        private ExplainedScore Smth(State state, int playerIndex, Vec movedCellLocation)
        {
            var hisLocation = GetEnemyLocation(state, playerIndex);
            var hisPows = 0.0;
            if (hisLocation != null)
                hisPows = WithPows(state, 1 - playerIndex, hisLocation).Value;
            var myPows = WithPows(state, playerIndex, movedCellLocation).Value;
            return myPows - hisPows;
        }
        private ExplainedScore Agade(State state, int playerIndex, Vec movedCellLocation)
        {
            var voronoi = NormalVoronoiDouble(state, playerIndex, movedCellLocation);
            var scoresDiff = state.GetScore(playerIndex) - state.GetScore(1 - playerIndex);
            var myNeighbors = GetIncidentPoints(movedCellLocation)
                .Count(p => p.InArea(state.Size) && state.CanMove(movedCellLocation, p));
           
            var hisNeighbours = 0;
            var hisLocation = GetEnemyLocation(state, playerIndex);
            if (hisLocation != null)
                hisNeighbours = GetIncidentPoints(hisLocation)
                    .Count(p => p.InArea(state.Size) && state.CanMove(hisLocation, p));

            return 2 * voronoi + scoresDiff + (myNeighbors - hisNeighbours);
        }

        private static Vec GetEnemyLocation(State state, int myPlayerIndex)
        {
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            var hisMoves0 = hisMoves.Where(m => m.UnitIndex % 2 == 0).ToList();
            var hisMoves1 = hisMoves.Where(m => m.UnitIndex % 2 == 1).ToList();
            var hisIndex = hisMoves1.Count != 0
                ? hisMoves1[0].UnitIndex
                : (hisMoves0.Count != 0 ? hisMoves0[0].UnitIndex : -999);
            if (hisIndex != -999)
            {
                var hisLoc = state.GetUnits(1 - myPlayerIndex)[hisIndex];
                return !hisLoc.Equals(new Vec(-1, -1)) ? hisLoc : null;
            }

            return null;
        }
        private ExplainedScore NewCombine(State state, int playerIndex, Vec movedCellLocation)
        {
           
           // var walls = ClassifyWalls(state, movedCellLocation);
            //     Console.Error.WriteLine($"VOR {voronoi} WALLS {walls} SUM {voronoi+walls}");
            //            if (voronoi >= 0)
            //                voronoi *= 3;
            //            else voronoi += 15;
            state.ChangeCurrentPlayer();
            var hisMoves = state.GetPossibleActions();
            state.ChangeCurrentPlayer();
            if (hisMoves.Count < 16)
                return WithPows(state, playerIndex, movedCellLocation);
            var voronoi = NormalVoronoiDouble(state, playerIndex, movedCellLocation);
            return voronoi;
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

        private double NormalVoronoiDouble(State state, int playerIndex, Vec myLocation)
        {
            var players = new List<Vec>() {myLocation};
            var enemy = GetEnemyLocation(state, playerIndex);
            if (enemy != null)
                players.Add(enemy);
            var meHis = NormalVoronoi(state, playerIndex, players);
            players.Reverse();
            var hisMe = NormalVoronoi(state, 1 - playerIndex, players);
            return meHis + hisMe;
        }

        private double NormalVoronoi(State state, int playerIndex, List<Vec> players)
        {
            var allSigned = AssignOwners(state, players).ToList();
            var my = allSigned.Count(x => x.Owner == playerIndex);
           // var my = allSigned.Where(x => x.Owner == playerIndex).Sum(x => x.Value/x.Distance);
            var his = allSigned.Count(x => x.Owner != playerIndex);
            //var his = allSigned.Where(x => x.Owner != playerIndex).Sum(x => x.Value/x.Distance); 
            //  Console.Error.WriteLine($"MY {my} HIS {his}");
            return my - his;
        }

        public static IEnumerable<OwnedLocation> AssignOwners(State state, List<Vec> players)
        {
            var track = new Dictionary<Vec, OwnedLocation>();
            var queue = new Queue<Vec>();
            for (var i = 0; i < players.Count; i++)
            {
                if (!players[i].InArea(state.Size))
                    continue;
                track[players[i]] = new OwnedLocation(i, players[i], 0, 0);
                yield return track[players[i]];
                queue.Enqueue(players[i]);
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
                    
                    track[nextPoint] = new OwnedLocation(track[point].Owner, nextPoint, EstimatePointValue(state, track[point].Owner, nextPoint), track[point].Distance + 1);
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

        private static double EstimatePointValue(State state, int distance, Vec pointLocation)
        {
            var neighboursHeight = GetIncidentPoints(pointLocation).Where(p => p.InArea(state.Size)).Sum(state.HeightAt);
            var malus = 0.0;
            var freeCells = 0;
            var unit3 = 0;
            var unit4 = 0;
            foreach (var point in GetIncidentPoints(pointLocation).Where(p => state.CanMove(pointLocation, p)))
            {
                if (!point.InArea(state.Size))
                    continue;
                if (state.HeightAt(point) == 0)
                    freeCells++;
                if (state.HeightAt(point) == 3)
                    unit3++;
                if (state.HeightAt(point) == 4)
                    unit4++;
            }
            var bonus = 0;
            if (GetIncidentPoints(pointLocation).Count(p => state.CanMove(pointLocation, p)) == 0)
                malus -= 999;
            malus -= Math.Pow(unit4, 3) * 3;
            if (state.HeightAt(pointLocation) == 3)
                bonus += 999;

            //            var score = Math.Pow(myScore, 3) * 115 
            //                        + Math.Pow(averageUnitHeight, 3) * 2 
            //                        + Math.Pow(neighboursHeight, 5) * 3.5 
            //                        + Math.Pow(freeCells, 4) * 4.3 + malus;
            var score =  Math.Pow(neighboursHeight, 2.5) * 3
                        + Math.Pow(unit3, 4) * 7
                        + malus + bonus;

            //   Console.Error.WriteLine($"MyLoc {myLocation} Points {myScore} Average {averageUnitHeight} Neighbour {neighboursHeight} Free {freeCells} SCORE {score}");

            return score;
        }

        private ExplainedScore WithPows(State state, int playerIndex, Vec myLocation)
        {
            var myScore = state.GetScore(playerIndex);
            var myUnits = state.GetUnits(playerIndex).Where(p => p.InArea(state.Size));
            var averageUnitHeight = myUnits.Average(state.HeightAt);
            var myMoves = state.GetPossibleActions();
            if (myMoves.Count == 0)
                return int.MinValue;

            var neighboursHeight = myUnits.Where(x => x.IsNear8To(myLocation)).Sum(state.HeightAt);
            var malus = 0.0;
            var freeCells = 0;
            var unit3 = 0;
            var unit4 = 0;
            foreach (var point in GetIncidentPoints(myLocation))
            {
                if (!point.InArea(state.Size))
                    continue;
                if (state.HeightAt(point) == 0 && state.CanMove(myLocation, point))
                    freeCells++;
                if (state.HeightAt(point) == 3 && state.CanMove(myLocation, point))
                    unit3++;
                if (state.HeightAt(point) == 4)
                    unit4++;
            }


            var bonus = 0;
            if (!GetPassableNeighbours(state, myLocation).Any())
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

        private static IEnumerable<Vec> GetPassableNeighbours(State state, Vec source)
        {
            return GetIncidentPoints(source).Where(p => p.InArea(state.Size) && state.CanMove(source, p));
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