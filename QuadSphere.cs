
/*-----------------------------------//
author: adhi.widagdo@oulu.fi
date: 2019/2020

This code creates 4x3 cross-cubemap
*/
//-----------------------------------//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class QuadSphere : MonoBehaviour
{
    
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;   //cubemap
    private Vector2[] UVs;  //equirectangular
    private Vector3[] normals;

    public int roundness;
    public int size = 4;

    private Vector2[] uve;

    private void Generate()
    {        
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Cube";
        CreateVertices();
        CreateTriangles();
       // mesh.triangles = mesh.triangles.Reverse().ToArray();
        RecalculateNormals(60);
        //mesh.RecalculateNormals();  

        mesh.RecalculateBounds();

        CreateUV();       
        //CreateEquiUV();

    }

    private void CreateVertices()
    {
       
        //the total number of vertices: (cube width(4) x 4 faces(FRBL) + 1 ) * height+1(5) + cube width+1(5)*height(4) *2(since it's for T&B) =  125
        int totalVertices = (size *4 + 1)*(size+1)+(size+1)*size*2;

        vertices = new Vector3[totalVertices];
   
        // Create nxn Cube
        for (int i = 0; i<=size; i++) //4x4cube -->//4
        {
            // Left Face (x,y,0)
            for (int j=0; j<= size; j++)    //4x4cube -->//4
            {
                vertices[i * (4 * size +1) + j] = new Vector3(j, i, 0);
                //4x4cube -->//vertices[i * 17 + j] = new Vector3(j, i, 0);
            }

            // Front Face (4,y,z)
            for (int j = size+1; j <= 2* size; j++) //4x4cube -->//j = 5; j <= 8
            {
                vertices[i * (4 * size + 1) + j] = new Vector3(size, i, j - size);
                //4x4cube -->//vertices[i * 17 + j] = new Vector3(4, i, j - 4);
            }

            // Right Face (x,y,4)
            for (int j = 2 * size+1; j <= 3 * size; j++)   //4x4cube -->//j = 9; j <= 12
            {
                vertices[i * (4 * size + 1) + j] = new Vector3(3*size - j, i, size);
                //4x4cube -->//vertices[i * 17 + j] = new Vector3(12 - j, i, 4);
            }

            // Back Face (0,y,z)
            for (int j = 3 * size + 1; j <= 4 * size; j++)  //4x4cube -->//j = 13; j <= 16
            {
                vertices[i * (4 * size + 1) + j] = new Vector3(0, i, 4 * size - j);
                //4x4cube -->//vertices[i * 17 + j] = new Vector3(0, i, 16 - j);
            }
        }

        // Top & Down
        for (int i = 0; i <= size-1; i++)    //4x4cube -->// 3
        {
            // Top Face (x,4,z)
            for (int j = 0; j <= size; j++) //4x4cube -->//4
            {
                vertices[i * (size+1) + ((size*4 + 1)*(size+1)) + j] = new Vector3(size-1 - i, size, j);
                //4x4cube -->//vertices[i * 5 + 85 + j] = new Vector3(3 - i, 4, j);
            }

            //Down Face (x,0,z)
            for (int j = 0; j <= size; j++) ///4x4cube -->/4
            {
                vertices[i * (size + 1) + ((size * 4 + 1) * (size + 1)) + ((size + 1) * size) + j] = new Vector3(size - 1 - i, 0, j);
                //4x4cube -->//vertices[i * 5 + 105 + j] = new Vector3(3 - i, 0, j);
            }
        }

        // Convert a cube to sphere & vice versa
        for (int i=0; i<vertices.Length; i++)
        {               
            SetVertex(i, (int)vertices[i].x, (int)vertices[i].y, (int)vertices[i].z);

            //Move Origin  to the center of cube / Origin is the center of rotation (0, 0, 0)
            vertices[i].x = vertices[i].x - size / 2;
            vertices[i].y = vertices[i].y - size / 2;
            vertices[i].z = vertices[i].z - size / 2;
        }

        mesh.vertices = vertices;
       
    }

    private void SetVertex(int i, int x, int y, int z)
    {
        Vector3 inner = vertices[i] = new Vector3(x, y, z);
        Vector3[] norm = new Vector3[vertices.Length];

        if (x < roundness)
        {
            inner.x = roundness;
        }
        else if (x > size - roundness) //xSize
        {
            inner.x = size - roundness;
        }
        if (y < roundness)
        {
            inner.y = roundness;
        }
        else if (y > size - roundness) //ySize
        {
            inner.y = size - roundness;
        }
        if (z < roundness)
        {
            inner.z = roundness;
        }
        else if (z > size - roundness) //zSize
        {
            inner.z = size - roundness;
        }

        norm[i] = (vertices[i] - inner).normalized;
        vertices[i] = inner + norm[i] * roundness;

    }

    private void CreateTriangles()
    {
        int quads = (size*size + size * size + size * size) * 2;      //4x4cube -->//(4*4 + 4*4 + 4*4) * 2; //(xx+yy+zz)
        int[] triangles = new int[(quads) * 6]; // 8 is duplication

        int ring = 4 * size + 1;    //4x4cube -->//4 * 4 + 1;
        int n = 0, v = 0;

        int vt = ring*size + size , vn = ring * (size+1); // For Top / vt & vn are the vertex (starting point)
        int gt = size*3+1; // gap for top vertices
        //4x4cube -->//int vt = ring * 4 + 4, vn = ring * 5;
        //4x4cube -->//int gt = 13; 

        int vd = ring * (size+1) + size*(size+1),   vm = vd + (size-1) * (size+1); // For down  / vd & vm are the vertex (starting point)
        int gd = vd - size;  // gap for down vertices
        //4x4cube -->//int vd = ring * 5 + 4 * 5, vm = 120;
        //4x4cube -->//int gd = 101; 

        int f = size + 1;


        // Make Triangle for Left-Front-Right-Back faces
        for (int i = 0; i < size; i++, v++)    //4x4cube -->//4
        {
            for (int j = 0; j < ring - 1; j++, v++)
            {
                n = MakeQuad(triangles, n, v, v + 1, v + ring, v + ring + 1); //v00,v10, v01, v11 
            }
        }
       

        //-----------------------------------------------------------------------------
        // Make Triangle for Top Face
        // Vertex start from 72-76 and connect to 85-89 (1 row only) -->//4x4cube 
        for (int j = 0; j < size; j++, vt++)   //4x4cube -->//4
        {
            n = MakeQuad(triangles, n, vt, vt + 1, vt + gt, vt + gt + 1);
        }

        // Vertex start from 85 - 104 (3 rows & 4 columns)    -->//4x4cube 
        for (int i = 0; i < size - 1; i++, vn++)   //4x4cube  -->//3
        {
            for (int j = 0; j < size; j++, vn++)   //4x4cube  -->//4
            {
                n = MakeQuad(triangles, n, vn, vn + 1, vn + f, vn + f + 1);
                //4x4cube  -->//n = MakeQuad(triangles, n, vn, vn + 1, vn + 5, vn + 5 + 1);
            }
        }

        //------------------------------------------------------------------------------
        // Make Triangle for Down Face 
        for (int j = 0; j < size; j++)   //4x4cube  -->//4
        {
            n = MakeQuad(triangles, n, j + vd, j + vd + 1, j + vd - gd, j + vd - gd + 1);
        }

        for (int i = 0; i < size - 1; i++) //4x4cube  -->//3
        {
            for (int j = 0; j < size; j++)
            {
                n = MakeQuad(triangles, n, j + vm - (i * f), j + vm - (i * f) + 1, j + vm - ((i + 1) * f), j + vm - (i + 1) * f + 1);
            }
            //4x4cube  -->//n = MakeQuad(triangles, n, vm, vm + 1, vm - 5, vm - 5 + 1);
            //4x4cube  -->//n = MakeQuad(triangles, n, vm-5, vm-5 + 1, vm-10, vm-10 + 1);
        }

        mesh.triangles = triangles;       
    }

    private static int MakeQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private void CreateUV()
    {
        if (vertices == null)
        {
            return;
        }

        uv = new Vector2[vertices.Length];

        // Make uv for LFRB faces
        for(int i=0; i<=size; i++) //4x4cube -->//4
        {
            for(int j=0; j<=4*size; j++) //4x4cube -->//16
            {
                //uv[i * (4 * size + 1) + j] = new Vector2(10*j / (4 * size), 10*(i + size) / (3 * size));
                uv[i * (4*size+1) + j] = new Vector2((float)j / (4*size), (float)(i + size) / (3*size));
                // 4x4cube-- >//uv[i * 17 + j] = new Vector2((float)j / 16, (float)(i + 4) / 12);
            }
        }

            //Make uv for Top
            for (int i = 0; i <= size - 1; i++)    //4x4cube -->//3
            {
                for (int j = 0; j <= size; j++)    //4x4cube -->//4
                {
                    uv[i * (size + 1) + ((size * 4 + 1) * (size + 1)) + j] = new Vector2((float)(j + size) / (4 * size), (float)(i + (2 * size + 1)) / (3 * size));
                    //4x4cube -->//uv[i * 5 + 85 + j] = new Vector2((float)(j + 4) / 16, (float)(i + 9) / 12);
                }
            }

            // Make uv for Down
            for (int i = 0; i <= size - 1; i++) //4x4cube -->//3
            {
                for (int j = 0; j <= size; j++)     //4x4cube -->//4
                {
                    uv[i * (size + 1) + ((size * 4 + 1) * (size + 1)) + ((size + 1) * size) + j] = new Vector2((float)(j + size) / (4 * size), (float)(size - 1 - i) / (3 * size));
                    //4x4cube -->//uv[i * 5 + 105 + j] = new Vector2((float)(j + 4) / 16, (float)(3-i) / 12);
                }
            }

        RemoveSeams();

        mesh.uv = uv; //--> use this if you want to use cubemap texture coordinate
        
    }

    private void RemoveSeams()
    {
        //The 6 seams lines are visible due to the cubemap converter reduces the pixel
        // So we need to rematch the UV coordinate, however, the seams will remain existing but less visible

        float t = 0.0002f; //tolerance value

        int vu = (4 * size + 1) * size; //for vertical upper line //4x4 cube --> 68
        
        // For Fixing Vertical pixel (y)
        for(int i=0; i<=size; i++)
        {   //LRB bottom line        
            uv[i].y += t;
            uv[i + (2*size)].y += t;
            uv[i + (3 * size)].y += t;

            //LRB upper line
            uv[vu + i].y -= t;
            uv[vu + i + (2 * size)].y -= t;
            uv[vu + i + (3 * size)].y -= t;
        }
        //Middle points or Intersection points
        uv[size].y += t;
        uv[2*size].y += t;

        uv[vu + size].y -= t;
        uv[vu + (2 * size)].y -= t;


        int ht = (size * 4 + 1) * (size + 1); //for horizontal top face
        int hb = ((size * 4 + 1) * (size + 1)) + ((size + 1) * size);  //for horizontal down face

        //For fixing Horizontal (x)
        for (int i=0; i<=size-1; i++)
        {   //Top Face
            uv[i* (size + 1) + ht].x += t;
            uv[i * (size + 1) + ht + size].x -= t;

            //Bottom Face
            uv[i * (size + 1) + hb].x += t;
            uv[i * (size + 1) + hb + size].x -= t;
        }      

    }


    private void CreateEquiUV()
    {
        Vector3[] ver = new Vector3[vertices.Length];
        float[] theta = new float[vertices.Length];       
        float[] phi = new float[vertices.Length];
        uve = new Vector2[vertices.Length];

        for (int i=0; i<ver.Length; i++)
        {
            float x = vertices[i].x;
            float y = vertices[i].y;
            float z = vertices[i].z;

            float rho = Mathf.Sqrt(x * x + y * y + z * z);

            theta[i] = Mathf.Atan2(x, z);
            phi[i] = Mathf.Asin(y / rho);

            uve[i].x = Mathf.Abs(theta[i] / Mathf.PI) - 0.5f;         //-0.5 is used to make easier debugging
            uve[i].y = (phi[i] / Mathf.PI) + 0.5f;

          //  Debug.Log(i + ": " + uve[i].x + ", " + uve[i].y + ": " + phi[i] + ", " + theta[i]);

        }


       //Finding the gap
        float[] gx = new float[vertices.Length];
        int d = size / 2;
        int ring = 4 * size + 1;

        for (int i= d+1; i<=size; i++)
        {            
            gx[i] = uve[i-1].x - uve[i].x;

        }
        for(int i=0; i<=size; i++)
        {
            gx[i+1] = gx[size - i];
           // Debug.Log(i + ": " + gx[i]);
        }

        // Correcting the UV value
        // Left-Face
        for (int i = 0; i <= size; i++)
        {
            for (int j = 0; j <= size; j++)
            {
                uve[j].x = uve[j].x;
                if (j > d)
                    uve[j+(i * ring)].x = uve[j + (i * ring) - 1].x + gx[j];
            }
        }

        // Front-Face
        for (int i = 0; i <= size; i++)
        {
            for (int j = size + 1; j <= 2 * size; j++)
            {
                uve[j + (i * ring)].x = uve[j + (i * ring) - 1].x + gx[j - size];
            }
        }

        // Right-Face
        for (int i = 0; i <= size; i++)
        {
            for (int j = 2 * size + 1; j <= 3 * size; j++)
            {
                uve[j + (i * ring)].x = uve[j + (i * ring) - 1].x + gx[j - 2 * size];

            }
        }

        for (int i = 0; i <= size; i++)
        {
            // Most Right
            for (int j = 3 * size + 1; j <= 4 * size; j++)
            {
                uve[j + (i * ring)].x = uve[j + (i * ring) - 1].x + gx[j - 3 * size];

            }
        }

        /*
        // Back-Face --> We slice by half for the most left (0) & most right (1) --> there is problem
        for (int i = 0; i <= size; i++)
        {
            //// Most Right
            //for (int j = 3 * size + 1; j <= 4 * size - size/2; j++)
            //{
            //    uve[j + (i * ring)].x = uve[j + (i * ring) - 1].x + gx[j - 3 * size];

            //}

            // Most Left
            for (int j = 0; j < size/2; j++)
            {
                float gap = gx[size-1];


            if (j > 0) gap = gx[size-j-1]; 
            uve[j + (3 * size + size / 2 + 1) + (i * ring)].x = gap + gx[size/2 - j -1];

            }
        }
        */
        //----------


        // Top-Face
        for (int i = 0; i <= size-1; i++)   //3
        {
            int t = (size * 4 + 1) * (size + 1);        //t=85

            for (int j = 0; j <= size; j++)     //4
            {                
                int at = i * (size + 1) + t + j;        //above half top
                int mt = (size / 2) * (size + 1) + t + size / 2;     //middle above half top (97)

                if (i < size / 2)
                    //Creat uv below half top --> 85-94  
                    uve[at].x = uve[at].x * -1 + 1f;

                else
                {
                    // Create uv above half top --> 95-104                   
                    if (at < mt )                                         
                        uve[at].x = uve[at].x;
                    else
                       uve[at].x = uve[at].x + 2;
                }

                    //   Debug.Log(t + j + ": " + uve[t + j].x);
            }
            // exception: middle top (92)
             uve[(size / 2 - 1)*(size+1) + t + (size/2)].x = uve[size + size / 2].x;
        }

        // Bottom-Face
        for (int i = 0; i <= size-1; i++)       //3
        {
            int b = ((size * 4 + 1) * (size + 1)) + ((size + 1) * size);        //b=105
            
            for (int j = 0; j <= size; j++)     //4
            {
                int ab = i * (size + 1) + b + j;        //above half top
                int mb = (size / 2) * (size + 1) + b + size / 2;     //middle above half bottom (117)

                if (i < size / 2)
                    //Creat uv above half bottom --> 105-114  
                    uve[ab].x = uve[ab].x * -1 + 1f;

                else
                {
                    // Create uv below half bottom --> 115-124                   
                    if (ab < mb)
                        uve[ab].x = uve[ab].x;
                    else
                        uve[ab].x = uve[ab].x + 2;
                }
            }
            // exception: middle bottom (112)
            uve[(size / 2 - 1) * (size + 1) + b + (size / 2)].x = uve[size + size / 2].x;
        }

        // Don't forget to Normalize 0-1 
        for (int i = 0; i < vertices.Length; i++)
        {
            uve[i].x = uve[i].x / 2 - 0.125f;
      //      Debug.Log(i + ": " + uve[i].x + ": " + uve[i].y);
        }

        mesh.uv = uve;
    }


    private void OnDrawGizmos()
    {
        Generate();

        if (vertices == null) { return; }

        Gizmos.color = Color.red;
        for (int i = 0; i < vertices.Length; i++)
       //     for (int i = 0; i < 84; i++)
            {
                //Gizmos.DrawSphere(vertices[i], 0.1f);
              //  Gizmos.DrawSphere(uve[i], 0.05f);
                //Gizmos.color = Color.yellow;
                //Gizmos.DrawRay(vertices[i], normals[i]);
            }
    }

        // Start is called before the first frame update
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void RecalculateNormals(float angle)
    {   
        //This function is created by: CHARIS MARANGOS
        //Reference: https://schemingdeveloper.com/2014/10/17/better-method-recalculate-normals-unity/
        // I edit a little bit to match with the whole codes.

        var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);

        //var vertices = mesh.vertices;
         normals = new Vector3[vertices.Length];

        // Holds the normal of each triangle in each sub mesh.
        var triNormals = new Vector3[mesh.subMeshCount][];

        var dictionary = new Dictionary<VertexKey, List<VertexEntry>>(vertices.Length);

        for (var subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; ++subMeshIndex)
        {

            var triangles = mesh.GetTriangles(subMeshIndex);

            triNormals[subMeshIndex] = new Vector3[triangles.Length / 3];

            for (var i = 0; i < triangles.Length; i += 3)
            {
                int i1 = triangles[i];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                // Calculate the normal of the triangle
                Vector3 p1 = vertices[i2] - vertices[i1];
                Vector3 p2 = vertices[i3] - vertices[i1];
                Vector3 normal = Vector3.Cross(p1, p2).normalized;
                int triIndex = i / 3;
                triNormals[subMeshIndex][triIndex] = normal;

                List<VertexEntry> entry;
                VertexKey key;

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i1]), out entry))
                {
                    entry = new List<VertexEntry>(4);
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(subMeshIndex, triIndex, i1));

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i2]), out entry))
                {
                    entry = new List<VertexEntry>();
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(subMeshIndex, triIndex, i2));

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i3]), out entry))
                {
                    entry = new List<VertexEntry>();
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(subMeshIndex, triIndex, i3));
            }
        }

        // Each entry in the dictionary represents a unique vertex position.

        foreach (var vertList in dictionary.Values)
        {
            for (var i = 0; i < vertList.Count; ++i)
            {

                var sum = new Vector3();
                var lhsEntry = vertList[i];

                for (var j = 0; j < vertList.Count; ++j)
                {
                    var rhsEntry = vertList[j];

                    if (lhsEntry.VertexIndex == rhsEntry.VertexIndex)
                    {
                        sum += triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex];
                    }
                    else
                    {
                        // The dot product is the cosine of the angle between the two triangles.
                        // A larger cosine means a smaller angle.
                        var dot = Vector3.Dot(
                            triNormals[lhsEntry.MeshIndex][lhsEntry.TriangleIndex],
                            triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex]);
                        if (dot >= cosineThreshold)
                        {
                            sum += triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex];
                        }
                    }
                }

                normals[lhsEntry.VertexIndex] = sum.normalized;
            }
        }

        mesh.normals = normals;
    }

    private struct VertexKey
    {
        //This function is created by: CHARIS MARANGOS
        //Reference: https://schemingdeveloper.com/2014/10/17/better-method-recalculate-normals-unity/

        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        // Change this if you require a different precision.
        private const int Tolerance = 100000;

        // Magic FNV values. Do not change these.
        private const long FNV32Init = 0x811c9dc5;
        private const long FNV32Prime = 0x01000193;

        public VertexKey(Vector3 position)
        {
            _x = (long)(Mathf.Round(position.x * Tolerance));
            _y = (long)(Mathf.Round(position.y * Tolerance));
            _z = (long)(Mathf.Round(position.z * Tolerance));
        }

        public override bool Equals(object obj)
        {
            var key = (VertexKey)obj;
            return _x == key._x && _y == key._y && _z == key._z;
        }

        public override int GetHashCode()
        {
            long rv = FNV32Init;
            rv ^= _x;
            rv *= FNV32Prime;
            rv ^= _y;
            rv *= FNV32Prime;
            rv ^= _z;
            rv *= FNV32Prime;

            return rv.GetHashCode();
        }
    }

    private struct VertexEntry
    {
        //This function is created by: CHARIS MARANGOS
        //Reference: https://schemingdeveloper.com/2014/10/17/better-method-recalculate-normals-unity/

        public int MeshIndex;
        public int TriangleIndex;
        public int VertexIndex;

        public VertexEntry(int meshIndex, int triIndex, int vertIndex)
        {
            MeshIndex = meshIndex;
            TriangleIndex = triIndex;
            VertexIndex = vertIndex;
        }
    }

    /// <summary>
    /// Currently we don't use the program below
    /// </summary>
    /// <param name="uv"></param>
    /*
    private void CubeMapToEquirectangular(Vector2[] uv) // This is not proven yet
    {
        UVs = new Vector2[vertices.Length];

        Vector2[] uvp = new Vector2[vertices.Length];  // UV polar 
        float[] u = new float[vertices.Length];
        float[] v = new float[vertices.Length];
        float cw = 1;   // Cubemap width (dimension)
        float W = 32, H = 16;     // size of equirectangular image

        //Remapping cooridnate [0,1] -->[-1,1]
        for (int i = 0; i < vertices.Length; i++)
        {
            u[i] = 2 * (uv[i].x / cw - 0.5f);
            v[i] = 2 * (uv[i].y / cw - 0.5f);
        }

        // LFRB faces
        for (int i = 0; i <= size; i++) //4x4cube -->//4
        {
            //Left face
            for (int j = 0; j <= 1 * size; j++) //4x4cube -->//16
            {
                int a = i * (4 * size + 1) + j;
                // 4x4cube-- >//uv[i * 17 + j] 

                uvp[a] = GetThetaPhi(u[a], -1, v[a]);   //u[a], -1, v[a]

            }

            //Front face
            for (int j = size + 1; j <= 2 * size; j++)
            {
                int a = i * (4 * size + 1) + j;
                uvp[a] = GetThetaPhi(1, u[a], v[a]);
            }

            //Right face
            for (int j = 2 * size + 1; j <= 3 * size; j++)
            {
                int a = i * (4 * size + 1) + j;
                uvp[a] = GetThetaPhi(-u[a], 1, v[a]);
            }

            //Back face
            for (int j = 3 * size + 1; j <= 4 * size; j++)
            {
                int a = i * (4 * size + 1) + j;
                uvp[a] = GetThetaPhi(-1, -u[a], v[a]);
            }
        }

        //Top & Down faces
        for (int i = 0; i <= size - 1; i++)    //4x4cube -->//3
        {
            for (int j = 0; j <= size; j++)    //4x4cube -->//4
            {
                int a = i * (size + 1) + ((size * 4 + 1) * (size + 1)) + j;     // 4x4cube-- >//uv[i * 5 + 85 + j]
                int b = i * (size + 1) + ((size * 4 + 1) * (size + 1)) + ((size + 1) * size) + j;       //4x4cube -->//uv[i * 5 + 105 + j] 

                //Top faces
                uvp[a] = GetThetaPhi(v[a], u[a], 1);


                //Down faces
                uvp[b] = GetThetaPhi(-v[b], u[b], -1);

            }
        }

        //Remap back [-1,1] --> [0,1]
        for (int i = 0; i < vertices.Length; i++)
        {
            // Debug.Log(i + " : " + uv[i] + " --> " + uvp[i]);
            UVs[i].x = (float)(0.5f + 0.5f * (uvp[i].x / Mathf.PI)) * W;   // uvp.x represents theta
            UVs[i].y = (float)(0.5f + (uvp[i].y / Mathf.PI)) * H;           // uvp.y represents phi           
        }
    }

    private static Vector2 GetThetaPhi(float x, float y, float z)
    {
        Vector2 theta_phi;
        float magnitude, xn, yn, zn;

        magnitude = Mathf.Sqrt(x * x + y * y + z * z);
        //Normalize
        xn = x / magnitude;
        yn = y / magnitude;
        zn = z / magnitude;

        //Convert to polar coordinate
        theta_phi.x = Mathf.Atan2(yn, xn);  // .x represents theta
        theta_phi.y = Mathf.Asin(z);        // .y represents phi

        return theta_phi;
    }


    private void ConvertToEquirectangular() // This is not proven yet
    {   //public static byte[] ConvertToEquirectangular(Texture2D sourceTexture, int outputWidth, int outputHeight)

        //Texture2D equiTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.ARGB32, false);
        float u, v; //Normalised texture coordinates, from 0 to 1, starting at lower left corner
        float phi, theta; //Polar coordinates
        int cubeFaceWidth, cubeFaceHeight;

        // cubeFaceWidth = sourceTexture.width / 4; //4 horizontal faces
        // cubeFaceHeight = sourceTexture.height / 3; //3 vertical faces

        cubeFaceWidth = 4 / 4; //4 horizontal faces
        cubeFaceHeight = 3 / 3; //3 vertical faces

        int height = 8, width = 16;

        for (int j = 0; j < height; j++)    //equiTexture.height
        {
            //Rows start from the bottom
            v = 1 - ((float)j / height);    //equiTexture.height

            theta = v * Mathf.PI;

            for (int i = 0; i < width; i++) //equiTexture.width
            {
                //Columns start from the left
                u = ((float)i / width);     //equiTexture.width
                phi = u * 2 * Mathf.PI;

                float x, y, z; //Unit vector
                x = Mathf.Sin(phi) * Mathf.Sin(theta) * -1;
                y = Mathf.Cos(theta);
                z = Mathf.Cos(phi) * Mathf.Sin(theta) * -1;

                float xa, ya, za;
                float a;

                a = Mathf.Max(new float[3] { Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z) });

                //Vector Parallel to the unit vector that lies on one of the cube faces
                xa = x / a;
                ya = y / a;
                za = z / a;

                Color color;
                int xPixel, yPixel;
                int xOffset, yOffset;

                if (xa == 1)
                {
                    //Right
                    xPixel = (int)((((za + 1f) / 2f) - 1f) * cubeFaceWidth);
                    xOffset = 2 * cubeFaceWidth; //Offset
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight; //Offset
                }
                else if (xa == -1)
                {
                    //Left
                    xPixel = (int)((((za + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = 0;
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight;
                }
                else if (ya == 1)
                {
                    //Up
                    xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = cubeFaceWidth;
                    yPixel = (int)((((za + 1f) / 2f) - 1f) * cubeFaceHeight);
                    yOffset = 2 * cubeFaceHeight;
                }
                else if (ya == -1)
                {
                    //Down
                    xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = cubeFaceWidth;
                    yPixel = (int)((((za + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = 0;
                }
                else if (za == 1)
                {
                    //Front
                    xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = cubeFaceWidth;
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight;
                }
                else if (za == -1)
                {
                    //Back
                    xPixel = (int)((((xa + 1f) / 2f) - 1f) * cubeFaceWidth);
                    xOffset = 3 * cubeFaceWidth;
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight;
                }
                else
                {
                    Debug.LogWarning("Unknown face, something went wrong");
                    xPixel = 0;
                    yPixel = 0;
                    xOffset = 0;
                    yOffset = 0;
                }

                xPixel = Mathf.Abs(xPixel);
                yPixel = Mathf.Abs(yPixel);

                xPixel += xOffset;
                yPixel += yOffset;

                //color = sourceTexture.GetPixel(xPixel, yPixel);
                //equiTexture.SetPixel(i, j, color);
            }
        }

        //equiTexture.Apply();
        //var bytes = equiTexture.EncodeToPNG();
        //Object.DestroyImmediate(equiTexture);


    }

    private Vector3 RayHit(float theta, float phi, float point)
    {
        Vector3 ray;
        ray.x = point * size / 2;

        float rho = ray.x / (Mathf.Cos(theta) * Mathf.Sin(phi));

        ray.y = rho * Mathf.Sin(theta) * Mathf.Sin(phi);
        ray.z = rho * Mathf.Cos(phi);

        return ray;
    }
    */
}
