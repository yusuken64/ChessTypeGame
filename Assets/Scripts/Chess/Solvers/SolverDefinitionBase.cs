using UnityEngine;

public abstract class SolverDefinitionBase : ScriptableObject
{
    internal abstract SolverBase GetSolver();
}
