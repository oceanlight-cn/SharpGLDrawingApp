using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.WPF;
using SharpGL.SceneGraph.Primitives;
using SharpGL.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Shapes;

namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        private string currentTool; // 当前选中的工具
        private OpenGL gl;
        private bool isMousePressed;
        private double mouseX;
        private double mouseY;

        public List<Shape_Point> shape_points = new List<Shape_Point>();
        public List<Shape_Line> shape_lines = new List<Shape_Line>();
        public List<Shape_Rectangle> shape_Rectangles = new List<Shape_Rectangle>();
        public List<Shape_Circle> shape_Circles = new List<Shape_Circle>();

        public interface CustomShape
        {
            int layerId { get; set; }
        }

        public class Shape_Point : CustomShape
        {
            public Vertex Position;
            public int layerId { get; set; }
        }
        public class Shape_Line : CustomShape
        {
            public Vertex startPos;
            public Vertex endPos;
            public int layerId { get; set; }
        }

        public class Shape_Rectangle : CustomShape
        {
            public List<Vertex> Postions;
            public int layerId { get; set; }
        }

        public class Shape_Circle : CustomShape 
        {
            public Vertex centerPos;
            public double radius;
            public int layerId { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            AddLayer();
        }

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取选中的工具
            Button button = (Button)sender;
            currentTool = button.Tag.ToString();
        }

        private void openGLControl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            gl = openGLControl.OpenGL;
            gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f); // 设置清屏颜色为黑色

        }

        private void openGLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            //if (!shapes.Any())
            //    CreateShapes();

            //  Get the OpenGL instance.
            var gl = args.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.PointSize(2.0f);

            //foreach (var shape in shapes)
            //{
            //    gl.Color(shape.Red, shape.Green, shape.Blue);

            //    gl.Begin(BeginMode.LineLoop);
            //    shape.Points.ForEach(sp => gl.Vertex(sp.Position));
            //    gl.End();
            //}


            gl.Color(0.5, 0.8, 0);

            foreach (var point in shape_points)
            {
                if(!LayerManager.Instance.GetLayerById(point.layerId).Removed && LayerManager.Instance.GetLayerById(point.layerId).Visible)
                {
                    gl.Begin(BeginMode.Points);
                    gl.Vertex(point.Position);
                    gl.End();
                }
            }
            
            foreach(var line in shape_lines)
            {
                if (!LayerManager.Instance.GetLayerById(line.layerId).Removed && LayerManager.Instance.GetLayerById(line.layerId).Visible)
                {
                    gl.Begin(BeginMode.Lines);
                    gl.Vertex(line.startPos);
                    gl.Vertex(line.endPos);
                    gl.End();
                }
            }

            foreach (var rec in shape_Rectangles)
            {
                if (!LayerManager.Instance.GetLayerById(rec.layerId).Removed && LayerManager.Instance.GetLayerById(rec.layerId).Visible)
                {
                    gl.Begin(BeginMode.Quads);
                    foreach (var point in rec.Postions)
                    {
                        gl.Vertex(point);
                    }

                    gl.End();
                }
            }

            foreach (var circle in shape_Circles)
            {
                int numSegments = 50; // 圆的分段数，用于绘制圆的边缘
                double angleStep = 2 * Math.PI / numSegments;

                if (!LayerManager.Instance.GetLayerById(circle.layerId).Removed && LayerManager.Instance.GetLayerById(circle.layerId).Visible)
                {
                    gl.Begin(BeginMode.TriangleFan);

                    // 绘制圆的边缘顶点
                    for (int i = 0; i <= numSegments; i++)
                    {
                        double angle = i * angleStep;
                        double x = circle.centerPos.X + circle.radius * Math.Cos(angle);
                        double y = circle.centerPos.Y + circle.radius * Math.Sin(angle);
                        gl.Vertex(x, y, 0);
                    }

                    gl.End();
                }
            }

            }


        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Default);
            renderTargetBitmap.Render(openGLControl);

            BitmapEncoder bitmapEncoder = new JpegBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            string filePath = "output.jpg";

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                bitmapEncoder.Save(fs);
            }

            MessageBox.Show("图像已导出为JPG格式，保存路径为：" + filePath);
        }
        private void ApplyVelocity(ref Vertex position, ref Vertex velocity)
        {
            float newX = position.X + velocity.X;
            if (newX < 0)
            {
                position.X = -newX;
                velocity.X *= -1;
            }
            else if (newX > openGLControl.ActualWidth)
            {
                position.X -= (newX - (float)openGLControl.ActualWidth);
                velocity.X *= -1;
            }
            else
            {
                position.X = newX;
            }


            float newy = position.Y + velocity.Y;
            if (newy < 0)
            {
                position.Y = -newy;
                velocity.Y *= -1;
            }
            else if (newy > openGLControl.ActualHeight)
            {
                position.Y -= (newy - (float)openGLControl.ActualHeight);
                velocity.Y *= -1;
            }
            else
            {
                position.Y = newy;
            }
        }

        //private void CreateShapes()
        //{
        //    //  Create some shapes...
        //    int shapeCount = random.Next(2, 5);
        //    for (int i = 0; i < shapeCount; i++)
        //    {
        //        //  Create the shape.
        //        var shape = new CrazyShape { Red = RandomFloat(), Green = RandomFloat(), Blue = RandomFloat() };

        //        //  Create the points.
        //        int pointCount = random.Next(3, 6);
        //        for (int j = 0; j < pointCount; j++)
        //            shape.Points.Add(new ShapePoint
        //            {
        //                Position = new Vertex(RandomFloat() * (float)openGLControl.ActualWidth,
        //                                         RandomFloat() * (float)openGLControl.ActualHeight, 0),
        //                Velocity = new Vertex(RandomFloat(1f, 10f), RandomFloat(1f, 10f), 0)
        //            });

        //        //  Add the shape.
        //        shapes.Add(shape);
        //    }
        //}

        private float RandomFloat(float min = 0, float max = 1)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }


        private readonly Random random = new Random();

        //private readonly List<CrazyShape> shapes = new List<CrazyShape>();

        //public class CrazyShape
        //{
        //    public List<ShapePoint> Points = new List<ShapePoint>();
        //    public float Red;
        //    public float Green;
        //    public float Blue;
        //}

        //public class ShapePoint
        //{
        //    public Vertex Position;
        //    public Vertex Velocity;
        //}

        private void openGLControl1_Resized(object sender, OpenGLRoutedEventArgs args)
        {
            //  Get the OpenGL instance.
            var gl = args.OpenGL;

            //  Create an orthographic projection.
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();
            gl.Ortho(0, openGLControl.ActualWidth, openGLControl.ActualHeight, 0, -10, 10);

            //  Back to the modelview.
            gl.MatrixMode(MatrixMode.Modelview);
        }

        private void openGLControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isMousePressed = true;
            System.Windows.Point mousePos = e.GetPosition(openGLControl);
            mouseX = mousePos.X;
            mouseY = mousePos.Y;
            // 根据当前选中的工具在OpenGL中绘制相应的图形
            switch (currentTool)
            {
                case "Point":
                    // TODO: 实现绘制点的方法
                    Shape_Point shape_point = new Shape_Point();
                    shape_point.Position = new Vertex((float)mouseX, (float)mouseY, 0);
                    shape_point.layerId = LayerManager.Instance.CurrentLayerId;
                    shape_points.Add(shape_point);
                    break;
                case "Line":
                    // TODO: 实现绘制线段的方法
                    Shape_Line line = new Shape_Line();
                    line.startPos = new Vertex((float)(mouseX - 30), (float)mouseY, 0);
                    line.endPos = new Vertex((float)(mouseX + 30), (float)mouseY, 0);
                    line.layerId = LayerManager.Instance.CurrentLayerId;
                    shape_lines.Add(line);
                    break;
                case "Rectangle":
                    // TODO: 实现绘制矩形的方法
                    Shape_Rectangle shape_rectangle = new Shape_Rectangle();
                    shape_rectangle.Postions = new List<Vertex>();
                    Vertex pos1 = new Vertex((float)(mouseX - 30), (float)(mouseY - 30), 0);
                    shape_rectangle.Postions.Add(pos1);
                    Vertex pos2 = new Vertex((float)(mouseX + 30), (float)(mouseY - 30), 0);
                    shape_rectangle.Postions.Add(pos2);
                    Vertex pos3 = new Vertex((float)(mouseX + 30), (float)(mouseY + 30), 0);
                    shape_rectangle.Postions.Add(pos3);
                    Vertex pos4 = new Vertex((float)(mouseX - 30), (float)(mouseY + 30), 0);
                    shape_rectangle.Postions.Add(pos4);
                    shape_rectangle.layerId = LayerManager.Instance.CurrentLayerId;
                    shape_Rectangles.Add(shape_rectangle);
                    break;
                case "RotatedRectangle":
                    // TODO: 实现绘制旋转矩形的方法
                    break;
                case "Polygon":
                    // TODO: 实现绘制多边形的方法
                    break;
                case "Circle":
                    // TODO: 实现绘制圆的方法
                    Shape_Circle shape_Circle = new Shape_Circle();
                    shape_Circle.centerPos = new Vertex((float)mouseX, (float)mouseY, 0);
                    shape_Circle.radius = 30;
                    shape_Circle.layerId = LayerManager.Instance.CurrentLayerId;
                    shape_Circles.Add(shape_Circle);
                    break;
                case "Ellipse":
                    // TODO: 实现绘制椭圆的方法
                    break;
                case "Arc":
                    // TODO: 实现绘制圆弧的方法
                    break;
            }

            

            


            openGLControl.InvalidateVisual();
        }

        private void openGLControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMousePressed = false;
            openGLControl.InvalidateVisual();
        }

        public void SetCurrentTool(string tool)
        {
            currentTool = tool;
        }

        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 处理导入图像的逻辑
        }

        private void AddLayer()
        {
            Layer newLayer = LayerManager.Instance.AddLayer();
            LayerButton layerButton = new LayerButton();
            layerButton.Id = newLayer.Id;
            layerButton.LabelContent = "Layer" + newLayer.Id;
            if (1 == newLayer.Id)
                layerButton.HideRemoveButton();
            layerPanel.Children.Add(layerButton);
        }

        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 处理新增图层的逻辑
            AddLayer();
        }

        private void DeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 处理删除图层的逻辑
        }
    }
}
