using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;

namespace PZ3
{
    public partial class MainWindow : Window
    {
        private Viewport3D viewport;
        private PerspectiveCamera camera;
        private Model3DGroup modelGroup;

        private NodeMap nodeMap;

        private double mapSizeX;
        private double mapSizeZ;

        private double rotationX;
        private double rotationY;
        private Point lastMousePos;

        private ToolTip tooltip;
        private GeometryModel3D selectedModel1;
        private GeometryModel3D selectedModel2;
        private Material oldMaterial1;
        private Material oldMaterial2;

        private Dictionary<Model3D, PowerNode> nodeModels;
        private Dictionary<Model3D, PowerLine> lineModels;

        private const double SW_LAT = 45.2325;
        private const double SW_LON = 19.793909;
        private const double NE_LAT = 45.277031;
        private const double NE_LON = 19.894459;

        private const float CUBE_SIZE = 10;
        private const float LINE_WIDTH = 3;



        public MainWindow()
        {
            InitializeComponent();
            InitScene();

            nodeMap = NodeMap.LoadFromXML("Geographic.xml", NodeFilterFunction);
            nodeModels = new Dictionary<Model3D, PowerNode>(nodeMap.Nodes.Count);
            lineModels = new Dictionary<Model3D, PowerLine>(nodeMap.Lines.Count);

            DrawNodes();
            DrawLines();
        }

        private void InitScene()
        {
            BitmapImage mapImg = new BitmapImage(new Uri("Images/map.jpg", UriKind.Relative));
            mapSizeX = mapImg.Width;
            mapSizeZ = mapImg.Height;

            viewport = new Viewport3D();
            viewport.MouseDown += ViewportMouseDown;

            // Add camera
            camera = new PerspectiveCamera();
            camera.FieldOfView = 60.0;
            camera.NearPlaneDistance = 1.0;
            camera.FarPlaneDistance = 10000.0;
            camera.LookDirection = new Vector3D(0, -1, 0);
            camera.UpDirection = new Vector3D(0, 0, -1);
            camera.Position = new Point3D(0, 2000, 0);

            viewport.Camera = camera;

            // Create map quad
            Geometry3D quadMesh = MeshFactory.Quad(new Point3D(), new Vector(mapImg.Width, mapImg.Height));
            Material quadMaterial = new DiffuseMaterial(new ImageBrush(mapImg));
            GeometryModel3D quad = new GeometryModel3D(quadMesh, quadMaterial);

            // Create directional & ambient light
            DirectionalLight dirLight = new DirectionalLight();
            dirLight.Color = Colors.White;
            dirLight.Direction = new Vector3D(0, -1, 0);

            AmbientLight ambientLight = new AmbientLight();
            ambientLight.Color = Color.FromRgb(62, 73, 89);

            modelGroup = new Model3DGroup();
            modelGroup.Children.Add(dirLight);
            modelGroup.Children.Add(ambientLight);
            modelGroup.Children.Add(quad);

            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = modelGroup;

            viewport.Children.Add(modelVisual);
            Content = viewport;
        }

        private void DrawNodes()
        {
            foreach (PowerNode node in nodeMap.Nodes)
            {
                double xNormalized = MathUtility.InverseLerp(SW_LON, NE_LON, node.X);
                double yNormalized = MathUtility.InverseLerp(SW_LAT, NE_LAT, node.Y);

                double xPos = MathUtility.Lerp(-mapSizeX / 2, mapSizeX / 2, xNormalized);
                double zPos = MathUtility.Lerp(-mapSizeZ / 2, mapSizeZ / 2, yNormalized);

                double cubeHeight = CUBE_SIZE / 2 + 0.1;
                bool intersectsWithNode;
                Geometry3D cubeMesh;

                // Check AABB intersections
                do
                {
                    cubeMesh = MeshFactory.Cube(new Point3D(xPos, cubeHeight, -zPos), new Vector3D(CUBE_SIZE, CUBE_SIZE, CUBE_SIZE));
                    intersectsWithNode = false;

                    foreach (Model3D cubeModel in modelGroup.Children)
                    {
                        if (cubeMesh.Bounds.IntersectsWith(cubeModel.Bounds))
                        {
                            cubeHeight += CUBE_SIZE + 0.1;
                            intersectsWithNode = true;
                            break;
                        }
                    }
                } while (intersectsWithNode);

                Material material;
                if (node.ConnectionCount <= 3)
                    material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 130, 130)));
                else if (node.ConnectionCount <= 5)
                    material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 80, 80)));
                else
                    material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(240, 0, 0)));

                GeometryModel3D cube = new GeometryModel3D(cubeMesh, material);
                nodeModels.Add(cube, node);

                modelGroup.Children.Add(cube);
            }
        }

        private void DrawLines()
        {
            foreach (PowerLine lineNode in nodeMap.Lines)
            {
                for (int i = 0; i < lineNode.Vertices.Count - 1; i++)
                {
                    MeshGeometry3D lineMesh = new MeshGeometry3D();

                    Point3D v1 = LatLonToPosition(lineNode.Vertices[i    ].X, lineNode.Vertices[i    ].Y);
                    Point3D v2 = LatLonToPosition(lineNode.Vertices[i + 1].X, lineNode.Vertices[i + 1].Y);
                    v1 = new Point3D(v1.X, 0, -v1.Z);
                    v2 = new Point3D(v2.X, 0, -v2.Z);

                    Vector3D dir = v2 - v1;
                    dir.Normalize();
                    Vector3D side = Vector3D.CrossProduct(dir, new Vector3D(0, 1, 0));
                    
                    Point3D p1 = v1 + side * LINE_WIDTH / 2;
                    Point3D p2 = v1 - side * LINE_WIDTH / 2;
                    Point3D p3 = v2 + side * LINE_WIDTH / 2;
                    Point3D p4 = v2 - side * LINE_WIDTH / 2;

                    Point3DCollection vertices = new Point3DCollection(4);
                    vertices.Add(new Point3D(p1.X, 1, p1.Z));
                    vertices.Add(new Point3D(p2.X, 1, p2.Z));
                    vertices.Add(new Point3D(p3.X, 1, p3.Z));
                    vertices.Add(new Point3D(p4.X, 1, p4.Z));

                    Int32Collection indices = new Int32Collection(6);
                    indices.Add(3); indices.Add(1); indices.Add(0);
                    indices.Add(0); indices.Add(2); indices.Add(3);

                    lineMesh.Positions = vertices;
                    lineMesh.TriangleIndices = indices;

                    Material material = new DiffuseMaterial(Brushes.Black);
                    GeometryModel3D line = new GeometryModel3D(lineMesh, material);
                    lineModels.Add(line, lineNode);

                    modelGroup.Children.Add(line);
                }
            }
        }

        private Point3D LatLonToPosition(double lon, double lat)
        {
            double xNormalized = MathUtility.InverseLerp(SW_LON, NE_LON, lon);
            double yNormalized = MathUtility.InverseLerp(SW_LAT, NE_LAT, lat);

            double xPos = MathUtility.Lerp(-mapSizeX / 2, mapSizeX / 2, xNormalized);
            double zPos = MathUtility.Lerp(-mapSizeZ / 2, mapSizeZ / 2, yNormalized);

            return new Point3D(xPos, 0, zPos);
        }

        private bool NodeFilterFunction(PowerNode node)
        {
            if (node.X < SW_LON || node.X > NE_LON || node.Y < SW_LAT || node.Y > NE_LAT)
                return false;

            return true;
        }

        #region WPF events
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // Pan
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Vector3D right = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);
                Vector delta = e.GetPosition(this) - lastMousePos;

                Point3D camPos = camera.Position;
                camPos += right * -delta.X + camera.UpDirection * delta.Y;
                camera.Position = camPos;
            }
            // Rotate around scene center
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Vector delta = e.GetPosition(this) - lastMousePos;
                rotationX += delta.Y;
                rotationY += delta.X;

                rotationX = MathUtility.Clamp(rotationX, -90.0, 0.0);

                Transform3DGroup transformGroup = new Transform3DGroup();
                RotateTransform3D rotation1 = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rotationX), new Point3D());
                RotateTransform3D rotation2 = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotationY), new Point3D());
                transformGroup.Children.Add(rotation2);
                transformGroup.Children.Add(rotation1);

                modelGroup.Transform = transformGroup;
            }

            if (tooltip != null && (e.LeftButton == MouseButtonState.Pressed || 
                                    e.RightButton == MouseButtonState.Pressed || 
                                    e.MiddleButton == MouseButtonState.Pressed))
            {
                tooltip.IsOpen = false;
            }

            lastMousePos = e.GetPosition(this);
        }

        private void OnWheelScroll(object sender, MouseWheelEventArgs e)
        {
            Point3D camPos = camera.Position;
            camPos += camera.LookDirection * e.Delta;
            camera.Position = camPos;

            if (tooltip != null)
                tooltip.IsOpen = false;
        }

        private void ViewportMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (tooltip != null)
                tooltip.IsOpen = false;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(viewport);
                PointHitTestParameters htParams = new PointHitTestParameters(mousePos);

                VisualTreeHelper.HitTest(viewport, null, HitTestCallback, htParams);
            }
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            RayHitTestResult htResult = result as RayHitTestResult;

            if (htResult != null)
            {
                if (nodeModels.ContainsKey(htResult.ModelHit))
                {
                    tooltip = new ToolTip();
                    tooltip.Content = nodeModels[htResult.ModelHit].ToolTip;
                    tooltip.IsOpen = true;
                    tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;

                    return HitTestResultBehavior.Stop;
                }

                if (selectedModel1 != null)
                {
                    Material mat = new DiffuseMaterial(Brushes.Red);
                    selectedModel1.Material = oldMaterial1;
                    selectedModel2.Material = oldMaterial2;
                }

                if (lineModels.ContainsKey(htResult.ModelHit))
                {
                    PowerLine line = lineModels[htResult.ModelHit];
                    PowerNode pn1 = nodeMap.IdToNodeDictionary[line.FirstEnd];
                    PowerNode pn2 = nodeMap.IdToNodeDictionary[line.SecondEnd];

                    GeometryModel3D pn1Model = nodeModels.FirstOrDefault(n => n.Value == pn1).Key as GeometryModel3D;
                    GeometryModel3D pn2Model = nodeModels.FirstOrDefault(n => n.Value == pn2).Key as GeometryModel3D;

                    // Save old materials to restore on deselect
                    selectedModel1 = pn1Model;
                    selectedModel2 = pn2Model;
                    oldMaterial1 = pn1Model.Material;
                    oldMaterial2 = pn2Model.Material;

                    Material mat = new DiffuseMaterial(Brushes.GreenYellow);
                    pn1Model.Material = mat;
                    pn2Model.Material = mat;

                    // Show tooltip
                    tooltip = new ToolTip();
                    tooltip.Content = lineModels[htResult.ModelHit].ToolTip;
                    tooltip.IsOpen = true;
                    tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
                }
            }

            return HitTestResultBehavior.Stop;
        }
        #endregion WPF events
    }
}
