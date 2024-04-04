using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private SquareGrid squareGrid;
    [SerializeField] private List<Vector3> vertices;
    [SerializeField] private List<int> triangles;
    [SerializeField] private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    [SerializeField] private List<List<int>> outlines = new List<List<int>>();
    [SerializeField] private HashSet<int> checkedVertices = new HashSet<int>();
    public void GenerateMesh(int[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;

            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;

            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;

            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;

            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;

            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;

            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;

            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;

            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;

            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;

            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;
        }
    }

    private void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3) 
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }

    private void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    private void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
            triangleDictionary[vertexIndexKey].Add(triangle);
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }

    }

    private void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) 
        {

        }
    }

    private int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++) 
            {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex)
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                        return vertexB;
                }

                
            }
        }

        return -1;
    }

    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                    break;
            }
        }
        return sharedTriangleCount == 1;
    }

    private void OnDrawGizmos()
    {
        /*if (squareGrid != null)
        {
            for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
                {
                    Gizmos.color = (squareGrid.squares[x, y].topLeft.active)? Color.black: Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * 0.4f);

                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, Vector3.one * 0.15f);
                }
            }
        }*/
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a; 
            vertexIndexB = b; 
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }
        
        public int this[int i]
        { 
            get 
            { 
                return vertices[i]; 
            } 
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid {
		public Square[,] squares;

		public SquareGrid(int[,] map, float squareSize) {
			int nodeCountX = map.GetLength(0);
			int nodeCountY = map.GetLength(1);
			float mapWidth = nodeCountX * squareSize;
			float mapHeight = nodeCountY * squareSize;

			ControlNode[,] controlNodes = new ControlNode[nodeCountX,nodeCountY];

			for (int x = 0; x < nodeCountX; x ++) {
				for (int y = 0; y < nodeCountY; y ++) {
					Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize + squareSize/2);
					controlNodes[x,y] = new ControlNode(pos,map[x,y] == 1, squareSize);
				}
			}

			squares = new Square[nodeCountX -1,nodeCountY -1];
			for (int x = 0; x < nodeCountX-1; x ++) {
				for (int y = 0; y < nodeCountY-1; y ++) {
					squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
				}
			}

		}
	}

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            {
                this.topLeft = topLeft;
                this.topRight = topRight;
                this.bottomRight = bottomRight;
                this.bottomLeft = bottomLeft;

                centreTop = topLeft.right;
                centreRight = bottomRight.above;
                centreBottom = bottomLeft.right;
                centreLeft = bottomLeft.above;

                if (topLeft.active)
                    configuration += 8;
                if (topRight.active)
                    configuration += 4;
                if (bottomRight.active)
                    configuration += 2;
                if (bottomLeft.active)
                    configuration += 1;
            }
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 position)
        {
            this.position = position;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
        {
            this.active = active;
            above = new Node(position + Vector3.forward * squareSize/2f);
            right = new Node(position + Vector3.right * squareSize/ 2f);
        }
    }

}
