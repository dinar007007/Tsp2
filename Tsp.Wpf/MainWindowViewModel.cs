using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Msagl.Drawing;
using Microsoft.Win32;
using Tsp.Common;
using Tsp.Solver;
using Tsp.Wpf.Common;

namespace Tsp.Wpf
{
    public class MainWindowViewModel: ViewModelBase
    {
        private Model _model;
        private Graph _graph;
        private double _minDistance;
        private string _finalPath;
        public ICommand CalculateCommand { get; set; }

        public ICommand FileOpenCommand { get; set; }

        public Model Model
        {
            get => _model;
            set => SetValue(ref _model, value);
        }

        public Graph Graph
        {
            get => _graph;
            set => SetValue(ref _graph, value);
        }

        public double MinDistance
        {
            get => _minDistance;
            set => SetValue(ref _minDistance, value);
        }

        public string FinalPath
        {
            get => _finalPath;
            set => SetValue(ref _finalPath, value);
        }

        public double[,] DataTable => Model.MatrixDistance;

        public MainWindowViewModel()
        {
            CalculateCommand = new DelegateCommand(Calculate);
            FileOpenCommand = new DelegateCommand(FileOpen);
#if DEBUG
            Model = MatrixLoader.LoadMatrix(@"C:\Users\Администратор\Documents\input.txt");
            UpdateGraph();
#endif
        }

        private void Calculate()
        {
            var solver = new Solver.Solver(Model.MatrixDistance.GetLength(0));
            solver.Tsp(Model.MatrixDistance);
            MinDistance = solver.FinalRes;
            AddBestPath(solver.FinalPath);
            FinalPath = GenerateFinalPath(solver.FinalPath);
        }

        private string GenerateFinalPath(IReadOnlyList<int> solverFinalPath)
        {
            var stringBuilder = new StringBuilder();
            foreach (var i in solverFinalPath)
            {
                stringBuilder.AppendLine(Model.Points[i].Name);
            }

            return stringBuilder.ToString();
        }

        private void AddBestPath(IReadOnlyList<int> solverFinalPath)
        {
            for (var index = 0; index < solverFinalPath.Count - 1; index++)
            {
                var path = solverFinalPath[index];
                var n = Graph.FindNode(Model.Points[path].Name);
                var e = n.OutEdges.FirstOrDefault(t => t.TargetNode.Id == Model.Points[solverFinalPath[index + 1]].Name);
                if (e != null)
                {
                    e.Attr.Color = Color.Red;
                }
            }
        }

        private void FileOpen()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            var filepath = openFileDialog.FileName;
            Model = MatrixLoader.LoadMatrix(filepath);
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            var g = new Graph();

            for (var i = 0; i < Model.Points.Count; i++)
            {
                for (var j = 0; j < Model.Points.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var e = g.AddEdge(Model.Points[i].Name, Model.Points[j].Name);
                    e.LabelText = Model.MatrixDistance[i, j].ToString(CultureInfo.InvariantCulture);
                }
            }

            Graph = g;
        }
    }
}
