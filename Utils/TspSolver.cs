using System;
using System.Collections.Generic;

namespace EcoRoute.Utils
{
    public static class TspSolver
    {
        /// <summary>
        /// Solves TSP using Nearest Neighbor heuristic followed by 2-Opt improvement.
        /// </summary>
        /// <param name="matrix">NxN distance matrix (in meters)</param>
        /// <returns>Ordered list of indices representing the optimal path</returns>
        public static List<int> Solve(double[][] matrix)
        {
            int n = matrix.Length;
            if (n == 0) return new List<int>();
            if (n == 1) return new List<int> { 0, 0 };

            // Step 1: Initial solution using Nearest Neighbor
            var route = new List<int>();
            var visited = new bool[n];
            
            // Start at index 0 (Assuming 0 is the Hub/Start point)
            int current = 0;
            visited[current] = true;
            route.Add(current);

            for (int step = 1; step < n; step++)
            {
                int next = -1;
                double minDistance = double.MaxValue;

                for (int j = 0; j < n; j++)
                {
                    if (!visited[j] && matrix[current][j] < minDistance)
                    {
                        minDistance = matrix[current][j];
                        next = j;
                    }
                }

                if (next != -1)
                {
                    visited[next] = true;
                    route.Add(next);
                    current = next;
                }
            }

            // Step 2: Optimize using 2-Opt Swap
            // This uncrosses any crossed paths to reduce total distance
            route = TwoOpt(route, matrix);

            return route;
        }

        private static List<int> TwoOpt(List<int> route, double[][] matrix)
        {
            int n = route.Count;
            bool improved = true;

            while (improved)
            {
                improved = false;
                for (int i = 0; i < n - 1; i++)
                {
                    for (int k = i + 1; k < n; k++)
                    {
                        // Calculate change in distance if we swap
                        // Current edges: (i, i+1) and (k, k+1)
                        // New edges: (i, k) and (i+1, k+1)
                        
                        int a = route[i];
                        int b = route[(i + 1) % n];
                        int c = route[k];
                        int d = route[(k + 1) % n];

                        // Skip if edges are adjacent (cannot swap)
                        if (b == c || a == d) continue;

                        double currentDist = matrix[a][b] + matrix[c][d];
                        double newDist = matrix[a][c] + matrix[b][d];

                        if (newDist < currentDist)
                        {
                            // Perform the swap (Reverse segment between i+1 and k)
                            route.Reverse(i + 1, k - i);
                            improved = true;
                        }
                    }
                }
            }
            return route;
        }
    }
}