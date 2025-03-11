using System;
using UnityEngine;

[Serializable]
public class WeightedEvaluator
{
    [SerializeReference]
    public EvaluatorBase Evaluator;

    [Range(0, 30)]
    public float Weight;
}