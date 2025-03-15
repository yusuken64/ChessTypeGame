using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MinimaxAB", menuName = "Solvers/MinimaxAB")]
public class MinimaxABSolverDefinition : SolverDefinitionBase
{
    [Range(2, 10)]
    public int SolveDepth;

    [Range(1, 10)]
    public float TimeLimitSeconds;
    public List<WeightedEvaluator> Evaluators;

    internal override SolverBase GetSolver()
    {
        var solver = new MinimaxABSolver();
        solver.SolveDepth = SolveDepth;
        solver.TimeLimitSeconds = TimeLimitSeconds;
        solver.Evaluators = Evaluators;

        return solver;
    }
}