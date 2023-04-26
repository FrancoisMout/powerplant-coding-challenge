namespace PowerPlantAPI.Utils
{
    public class KnapsackProblemSolver
    {
        private const int ThresholdValue = 100000;
        private const int ThresholdLength = 20;

        public static (decimal, bool[]) SolveKnapsackProblem(decimal[] decimalSet, decimal decimalCapacity)
        {
            if (decimalCapacity <= 0)
            {
                return (0, new bool[decimalSet.Length]);
            }

            var set = decimalSet
                .Select(x => (int)(x * 10))
                .ToArray();

            var target = (int)(decimalCapacity * 10);

            var solution = SolveKnapsackProblem(set, target);

            return (((decimal)solution.Item1) / 10, solution.Item2);
        }

        public static (int, bool[]) SolveKnapsackProblem(int[] set, int capacity)
        {
            if (capacity > ThresholdValue || set.Length <= ThresholdLength)
            {
                return GetKnapsackBruteForce(set, capacity);
            }

            return GetKnapSackDynamicProgramming(set, capacity);
        }

        private static (int, bool[]) GetKnapsackBruteForce(int[] weights, int capacity)
        {
            int n = weights.Length;

            var bestSubset = new bool[n];
            int bestValue = 0;

            for (int i = 0; i < (1 << n); i++)
            {
                int currentWeight = 0;
                for (int j = 0; j < n; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        currentWeight += weights[j];
                    }
                }

                if (currentWeight <= capacity && currentWeight > bestValue)
                {
                    bestValue = currentWeight;
                    Array.Clear(bestSubset, 0, n); // clear the best subset array
                    for (int j = 0; j < n; j++)
                    {
                        if ((i & (1 << j)) != 0)
                        {
                            bestSubset[j] = true;
                        }
                    }
                }
            }

            return (bestValue, bestSubset);
        }

        private static (int, bool[]) GetKnapSackDynamicProgramming(int[] weights, int Capacity)
        {
            var n = weights.Length;

            var table = new int[n + 1, Capacity + 1];

            for (int i = 0; i <= n; i++)
            {
                for (int w = 0; w <= Capacity; w++)
                {
                    if (i == 0 || w == 0)
                    {
                        table[i, w] = 0;
                    }
                    else if (weights[i - 1] <= w)
                    {
                        table[i, w] = Math.Max(
                            weights[i - 1] + table[i - 1, w - weights[i - 1]],
                            table[i - 1, w]);
                    }
                    else
                    {
                        table[i, w] = table[i - 1, w];
                    }
                }
            }

            return (table[n, Capacity], ExtractDPSolution(table, weights));
        }

        private static bool[] ExtractDPSolution(int[,] subset, int[] weights)
        {
            var i = subset.GetLength(0) - 1;
            var j = subset.GetLength(1) - 1;
            var solution = new bool[i];

            while (true)
            {
                if (subset[i, j] != subset[i - 1, j])
                {
                    solution[i - 1] = true;
                    j -= weights[i - 1];
                }

                i--;

                if (i == 0 || j == 0)
                {
                    break;
                }
            }

            return solution;
        }
    }
}
