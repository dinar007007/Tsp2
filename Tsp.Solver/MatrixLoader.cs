using System.IO;
using System.Linq;
using Tsp.Common;

namespace Tsp.Solver
{
    public class MatrixLoader
    {
        public static Model LoadMatrix(string path)
        {
            var data = File.ReadAllLines(path);
            var n = int.Parse(data[0]);

            var result = new Model
            {
                Points = data[1].Split(' ').Select(p => new Point
                {
                    Description = p,
                    Name = p
                }).ToList()
            };

            var matrixDistance = new double[n, n];

            for (var i = 0; i < n; i++)
            {
                var line = data[i + 2].Split(' ');
                for (var j = 0; j < n; j++)
                {
                    if (line[j] == '-'.ToString())
                    {
                        matrixDistance[i, j] = double.PositiveInfinity;
                    }
                    else
                    {
                        matrixDistance[i, j] = double.Parse(line[j]);
                    }
                }
            }

            result.MatrixDistance = matrixDistance;

            return result;
        }
    }
}
