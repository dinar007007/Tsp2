using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Msagl.Drawing;
using Microsoft.Win32;
using Tsp.Common;
using Tsp.Solver;
using Tsp.Wpf.Common;
using Color = Microsoft.Msagl.Drawing.Color;

namespace Tsp.Wpf
{
    public class MainWindowViewModel: ViewModelBase
    {
        private Model _model = new Model();
        private Graph _graph;
        private double _minDistance;
        private string _finalPath;

        public ICommand CalculateCommand { get; set; }
        public ICommand FileOpenCommand { get; set; }
        public ICommand FileSaveCommand { get; set; }
        public ICommand CellEditEndingCommand { get; set; }
        public ICommand GraphSaveCommand { get; set; }

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
            set
            {
                SetValue(ref _finalPath, value);
                OnPropertyChanged(nameof(SaveEnable));
            }
        }

        public bool SaveEnable => !string.IsNullOrEmpty(FinalPath);

        public double[,] DataTable => Model.MatrixDistance;

        public List<int> BestPath { get; set; }

        public MainWindowViewModel()
        {
            CalculateCommand = new DelegateCommand(Calculate);
            FileOpenCommand = new DelegateCommand(FileOpen);
            FileSaveCommand = new DelegateCommand(FileSave);
            CellEditEndingCommand = new DelegateCommand(CellEditEnding);
            GraphSaveCommand = new DelegateCommand(GraphSave);
        }

        private static void GraphSave(object o)
        {
            var fileSaveDialog = new SaveFileDialog();
            if (fileSaveDialog.ShowDialog() != true)
            {
                return;
            }

            var toSave = (FrameworkElement) o;
            var encoder = new PngBitmapEncoder();
            var bitmap = new RenderTargetBitmap((int) toSave.ActualWidth, (int) toSave.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(toSave);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileSaveDialog.FileName))
            {
                encoder.Save(stream);
            }
        }

        private void CellEditEnding()
        {
            BestPath.Clear();
            UpdateGraph();
        }

        private void FileSave()
        {
            try
            {
                var fileSaveDialog = new SaveFileDialog();
                if (fileSaveDialog.ShowDialog() != true)
                {
                    return;
                }

                SaveResult(fileSaveDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка при сохранении файла");
            }
        }

        private void SaveResult(string fileName)
        {
            var outputResult = new List<string>
            {
                "Оптимальный маршрут",
                FinalPath, 
                "Минимальное расстояние",
                MinDistance.ToString(CultureInfo.InvariantCulture)
            };
            File.WriteAllLines(fileName, outputResult);
        }

        private void Calculate()
        {
            var solver = new Solver.Solver(Model.MatrixDistance.GetLength(0));
            solver.Tsp(Model.MatrixDistance);
            MinDistance = solver.FinalRes;
            BestPath = solver.FinalPath.ToList();
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
            try
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() != true)
                {
                    return;
                }

                var filepath = openFileDialog.FileName;
                Model = MatrixLoader.LoadMatrix(filepath);
            }
            catch (Exception)
            {
                MessageBox.Show("Некорректный формат входного файла");
                return;
            }

            foreach (var modelPoint in Model.Points)
            {
                modelPoint.PropertyChanged += ModelPoint_PropertyChanged;
            }
            UpdateGraph();
            OnPropertyChanged(nameof(DataTable));
        }

        private void ModelPoint_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateGraph();
            if (BestPath != null && BestPath.Count > 0)
            {
                AddBestPath(BestPath);
            }
        }

        private void UpdateGraph()
        {
            var g = new Graph
            {
                Attr = {BackgroundColor = Color.LightCyan}
            };

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
