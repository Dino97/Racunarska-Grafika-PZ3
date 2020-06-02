using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PZ3
{
    public static class MeshFactory
    {
        public static Geometry3D Quad(Point3D center, Vector size)
        {
            MeshGeometry3D quadMesh = new MeshGeometry3D();

            Point3DCollection vertices = new Point3DCollection(4);
            vertices.Add(new Point3D(center.X - size.X / 2, 0, center.Y - size.Y / 2));
            vertices.Add(new Point3D(center.X + size.X / 2, 0, center.Y - size.Y / 2));
            vertices.Add(new Point3D(center.X + size.X / 2, 0, center.Y + size.Y / 2));
            vertices.Add(new Point3D(center.X - size.X / 2, 0, center.Y + size.Y / 2));

            Int32Collection indices = new Int32Collection(6);
            indices.Add(0); indices.Add(2); indices.Add(1);
            indices.Add(0); indices.Add(3); indices.Add(2);

            PointCollection texCoords = new PointCollection(4);
            texCoords.Add(new Point(0, 0));
            texCoords.Add(new Point(1, 0));
            texCoords.Add(new Point(1, 1));
            texCoords.Add(new Point(0, 1));

            quadMesh.Positions = vertices;
            quadMesh.TriangleIndices = indices;
            quadMesh.TextureCoordinates = texCoords;

            return quadMesh;
        }

        public static Geometry3D Cube(Point3D center, Vector3D size)
        {
            MeshGeometry3D cubeMesh = new MeshGeometry3D();

            Point3DCollection vertices = new Point3DCollection(8);
            vertices.Add(new Point3D(center.X - size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2));
            vertices.Add(new Point3D(center.X + size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2));
            vertices.Add(new Point3D(center.X + size.X / 2, center.Y + size.Y / 2, center.Z - size.Z / 2));
            vertices.Add(new Point3D(center.X - size.X / 2, center.Y + size.Y / 2, center.Z - size.Z / 2));

            vertices.Add(new Point3D(center.X - size.X / 2, center.Y - size.Y / 2, center.Z + size.Z / 2));
            vertices.Add(new Point3D(center.X + size.X / 2, center.Y - size.Y / 2, center.Z + size.Z / 2));
            vertices.Add(new Point3D(center.X + size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2));
            vertices.Add(new Point3D(center.X - size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2));

            int[] indicesArray =
            {
                0, 1, 2,
                2, 3, 0,
                0, 4, 5,
                0, 5, 1,
                1, 5, 6,
                1, 6, 2,
                2, 6, 7,
                2, 7, 3,
                3, 7, 4,
                3, 4, 0,
                4, 7, 6,
                4, 6, 5
            };

            Int32Collection indices = new Int32Collection(indicesArray);

            cubeMesh.Positions = vertices;
            cubeMesh.TriangleIndices = indices;

            return cubeMesh;
        }
    }
}