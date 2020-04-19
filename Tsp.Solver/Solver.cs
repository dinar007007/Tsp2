using System;
using System.Collections.Generic;

namespace Tsp.Solver
{
    public class Solver
    {
        private readonly int _n;
        private readonly bool[] _visited;
        private readonly int[] _finalPath;

        public IReadOnlyList<int> FinalPath => _finalPath;

        public double FinalRes { get; private set; } = double.MaxValue;

        public Solver(int count)
        {
            _n = count;
            _finalPath = new int[_n + 1];
            _visited = new bool[_n];
        }

        public void Tsp(double[,] adj)
        {
            var currentPath = new int[_n + 1];
            for (var i = 0; i < _n + 1; i++)
            {
                currentPath[i] = -1;
            }

            var currentBound = 0.0;

            for (var i = 0; i < _n; i++)
            {
                currentBound += FirstMin(adj, i) + SecondMin(adj, i);
            }

            currentBound = Math.Abs(currentBound - 1) < double.Epsilon ? currentBound / 2 + 1 : currentBound / 2;

            _visited[0] = true;
            currentPath[0] = 0;

            TspRec(adj, currentBound, 0, 1, currentPath);
        }

        private void CopyToFinal(IReadOnlyList<int> currentPath)
        {
            for (var i = 0; i < _n; i++)
            {
                _finalPath[i] = currentPath[i];
            }

            _finalPath[_n] = currentPath[0];
        }

        private double FirstMin(double[,] adj, int i)
        {
            var min = double.MaxValue;
            for (var k = 0; k < _n; k++)
            {
                if (adj[i, k] < min && i != k)
                {
                    min = adj[i, k];
                }
            }

            return min;
        }

        private double SecondMin(double[,] adj, int i)
        {
            var first = double.MaxValue;
            var second = double.MaxValue;
            for (var j = 0; j < _n; j++)
            {
                if (i == j)
                {
                    continue;
                }

                if (adj[i, j] <= first)
                {
                    second = first;
                    first = adj[i, j];
                }
                else
                {
                    if (adj[i, j] <= second && Math.Abs(adj[i, j] - first) > double.Epsilon)
                        second = adj[i, j];
                }
            }

            return second;
        }

        private void TspRec(double[,] adj,
            double currentBound,
            double currentWeight,
            int level,
            int[] currentPath)
        {
            if (level == _n)
            {
                if (Math.Abs(adj[currentPath[level - 1], currentPath[0]]) < double.Epsilon)
                {
                    return;
                }

                var currentRes = currentWeight + adj[currentPath[level - 1], currentPath[0]];

                if (currentRes >= FinalRes)
                {
                    return;
                }

                CopyToFinal(currentPath);
                FinalRes = currentRes;

                return;
            }

            for (var i = 0; i < _n; i++)
            {
                if (Math.Abs(adj[currentPath[level - 1], i]) > double.Epsilon && _visited[i] == false)
                {
                    var temp = currentBound;
                    currentWeight += adj[currentPath[level - 1], i];

                    if (level == 1)
                    {
                        currentBound -= ((FirstMin(adj, currentPath[level - 1]) + FirstMin(adj, i)) / 2);
                    }
                    else
                    {
                        currentBound -= ((SecondMin(adj, currentPath[level - 1]) + FirstMin(adj, i)) / 2);
                    }

                    if (currentBound + currentWeight < FinalRes)
                    {
                        currentPath[level] = i;
                        _visited[i] = true;
                        TspRec(adj, currentBound, currentWeight, level + 1, currentPath);
                    }

                    currentWeight -= adj[currentPath[level - 1], i];
                    currentBound = temp;

                    for (var j = 0; j < _visited.Length; j++)
                    {
                        _visited[j] = false;
                    }

                    for (var j = 0; j <= level - 1; j++)
                    {
                        _visited[currentPath[j]] = true;
                    }
                }
            }
        }
    }
}
