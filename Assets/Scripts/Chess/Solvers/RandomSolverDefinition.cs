using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomSolver", menuName = "Solvers/Random")] 
public class RandomSolverDefinition : SolverDefinitionBase
{
    internal override SolverBase GetSolver()
    {
        return new RandomSolver();
    }

    internal class RandomSolver : SolverBase
    {
        public override Move GetNextMove(ChessGameRecord game, ChessColor color, IEnumerable<Move> legalMoves)
        {
            return legalMoves.OrderBy(x => Guid.NewGuid()).First();
        }
    }
}
