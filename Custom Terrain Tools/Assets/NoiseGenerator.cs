using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel;
using Unity.Jobs;
using System.Threading.Tasks;

public class NoiseGenerator : MonoBehaviour
{
    //https://www.youtube.com/watch?v=TZFv493D7jo
    public Transform terrain01;
    private MeshRenderer rendererA;
    private MeshFilter filterA;
    public MeshFilter filterB;
    public MeshFilter filterC;

    [Header("Brush Settings")]
    public Vector2 initialResolution;
    //Custom brushes are supported
    public Texture2D brush;
    public int brushScale = 2;
    public float intensity = 1f;


    [Header("NoiseProps")]
    public int size;
    public float scale;

    [Header("Edge Fades [Removed]")]
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public int edgeFadeRadius;
    public int neighbourBlend = 3;


    [Range(0,5000)]
    public int vertexSelector;
    
    [Header("Noise Texture Settings")]
    public Vector3 max;
    public Vector3 min;
    public Vector3 bounds;
    public Vector2 offset;

    [Header("Height Params")]
    public float maxHeight;

    [Header("Blending Params")]
    public int blendWidth;

    //Public hidden values
    [HideInInspector]
    public float[] vals;
    [HideInInspector]
    public Texture2D matTex;
    [HideInInspector]
    public List<Point> points;

    //As soon as the Editor Script (VertexPainter.cs) is used to modify the mesh, all Update meshes are called to change the mesh
    public void RegsiterChange()
    {
        //In case editor calls in Edit Mode
        if (rendererA == null)
            Awake();

        UpdateMesh(filterA);
        UpdateMesh(filterB);
        UpdateMesh(filterC);
        
    }
    
    //Incomplete
    public void Save( string path )
    {

    }

    //Incomplete
    public void Load ( string path )
    {

    }

    private void UpdateMesh (MeshFilter aF)
    {
       
        var position = new NativeArray<Vector3>(aF.mesh.vertexCount, Allocator.Persistent);

        Vector3[] verts = aF.mesh.vertices;
        position.CopyFrom(verts);

        var colorVals = new NativeArray<float>(size * size, Allocator.Persistent);

        colorVals.CopyFrom(vals);
        //var byteArr = new NativeArray<Color32>(matTex.GetRawTextureData<Color32>().Length, Allocator.Persistent);
        //byteArr.CopyFrom(matTex.GetRawTextureData<Color32>());
        //SampleMesh();
        //SampleMesh(filterB);
        //SampleMesh(filterC);

        var job = new ApplyNoiseJob()
        {
            verts = position,
            colors = colorVals,
            min = terrain01.InverseTransformPoint(this.min),
            max = terrain01.InverseTransformPoint(this.max),
            maxHeight = this.maxHeight,
            size = this.size
        };

        JobHandle jobHandle = job.Schedule(aF.mesh.vertexCount, 128);
        jobHandle.Complete();

        Vector3[] filteredVerts = new Vector3[aF.mesh.vertexCount];
        filteredVerts = position.ToArray();
        for (int c = 0; c < filteredVerts.Length; c++)
        {
            filteredVerts[c] = (filteredVerts[c]);
        }

        aF.mesh.vertices = filteredVerts;
        aF.mesh.RecalculateBounds();
        aF.mesh.RecalculateNormals();
        aF.transform.GetComponent<MeshCollider>().sharedMesh = filterA.mesh;


        position.Dispose();
        colorVals.Dispose();
    }
    public List<int[]> orders = new List<int[]>();
    private void Awake()
    {

        rendererA = terrain01.GetComponent<MeshRenderer>();
        filterA = terrain01.GetComponent<MeshFilter>();
        matTex = GenerateNoiseTexture(true);
        
        //rendererA.material.mainTexture = matTex;
        vals = new float[size * size];
        int i = 0;
        for (int x = 0; x < matTex.width; x++)
        {

            for (int y = 0; y < matTex.height; y++)
            {
                vals[i] = matTex.GetPixel(x, y).r;
                i++;
                //print(i);
            }
        }
        //rendererA.material.mainTexture = matTex;
        RegsiterChange();

    }


    private void Blend (NoiseGenerator gen, string side)
    {

        if (side == "Left")
        {

            //Left Noise Map
            Texture2D lNoise = gen.matTex;

            float[] yAverages = new float[lNoise.height];

            for (int y = 0; y < lNoise.width; y++)
            {
                float averageX = 0f;
                int xCount = 0;
                for (int x = 0; x < lNoise.height; x++)
                {

                    //Past the blending area
                    if (x >= lNoise.width-blendWidth)
                    {
                        //Average the x values.
                        averageX += lNoise.GetPixel(x, y).r;
                        xCount++;
                    }

                }

                averageX /= xCount;
                yAverages[y] = averageX;
            }

            //Now affect the current noise texture (coming from the left -> end)
            for (int y = 0; y < matTex.width; y++)
            {
                for (int x = 0; x < matTex.height; x++)
                {

                    //Past the blending area
                    if (x >= blendWidth)
                    {

                        float current = matTex.GetPixel(x, y).r * yAverages[y];
                        matTex.SetPixel(x, y, new Color(current, current, current));
                    }

                }
            }

            matTex.Apply();


        }

        SampleMesh();
        SampleMesh(filterB);
        SampleMesh(filterC);


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, bounds/2);
        Gizmos.DrawWireCube(min, Vector3.one);
        Gizmos.DrawWireCube(max, Vector3.one);
        if (filterA != null)
        {
            Vector3[] verts = filterA.mesh.vertices;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(terrain01.TransformPoint(verts[vertexSelector]),0.2f);
             
        }
    }

    
    
    private void Update()
    {
        
        if (Input.GetKey(KeyCode.Space))
        {
            matTex = GenerateNoiseTexture(true);
            rendererA.material.mainTexture = matTex;
            UpdateMesh(filterA);
            UpdateMesh(filterB);
            UpdateMesh(filterC); 
        }
    }

    //Creating a Job using Unity's Job System to manipulate the mesh quickly [100x faster]
    struct ApplyNoiseJob : IJobParallelFor {

        //Native array of Vector3 to store all the verts
        public NativeArray<Vector3> verts;
        
        public float rXPos, rYPos;

        [NativeDisableParallelForRestriction] //Stores the float[] of the heights that are created in the Texture2D (Texture2D's aren't supported in Job System)
        public NativeArray<float> colors;
        
        //Parameters
        public Vector3 min, max;
        public float maxHeight;
        public int size;

        //Execute Method
        public void Execute (int i)
        {

            Vector3 localPos = verts[i];
            //float realXPos = trans.TransformPoint(localPos).x;
            //float realZPos = trans.TransformPoint(localPos).z;

            float realXPos = localPos.x;
            float realZPos = localPos.z;

            float percX = ((realXPos - min.x)*100) / (max.x - min.x);
            float percY = ((realZPos - min.z)*100) / (max.z - min.z);

            int pixelX = Mathf.Clamp(Mathf.RoundToInt(percX/100 * size),0,size-1);
            int pixelY = Mathf.Clamp(Mathf.RoundToInt(percY/100 * size),0, size-1);

            float r = colors[pixelY * size + pixelX];
   
            float val = r;
            float Fy = (val * maxHeight);

            localPos = new Vector3(realXPos, Fy, realZPos);

            verts[i] = localPos;


        }

    }

    
    //Base mesh sampling [redundant]
    private void SampleMesh()
    {
        Vector3[] verts = filterA.mesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 localPos = filterA.mesh.vertices[i];
            float realXPos = terrain01.TransformPoint(localPos).x;
            float realZPos = terrain01.TransformPoint(localPos).z;

            float percX = (realXPos - min.x) / (max.x - min.x);
            float percY = (realZPos - min.z) / (max.z - min.z);

            int pixelX = Mathf.RoundToInt(percX * size);
            int pixelY = Mathf.RoundToInt(percY * size);

            Color r = matTex.GetPixel(pixelX, pixelY);
            float val = r.r;

            verts[i].y = val * maxHeight;

        }

        filterA.mesh.vertices = verts;
        filterA.mesh.RecalculateBounds();
        filterA.mesh.RecalculateNormals();
        terrain01.GetComponent<MeshCollider>().sharedMesh = filterA.mesh;
       
    }

    //Sampling any mesh from the current noise texture
    private void SampleMesh(MeshFilter filterA)
    {
        Vector3[] verts = filterA.mesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            //Finding what vertext maps to what coordinates in the texture 

            Vector3 localPos = filterA.mesh.vertices[i];
            float realXPos = terrain01.TransformPoint(localPos).x;
            float realZPos = terrain01.TransformPoint(localPos).z;

            float percX = (realXPos - min.x) / (max.x - min.x);
            float percY = (realZPos - min.z) / (max.z - min.z);

            int pixelX = Mathf.RoundToInt(percX * size);
            int pixelY = Mathf.RoundToInt(percY * size);

            Color r = matTex.GetPixel(pixelX, pixelY);
            float val = r.r;

            verts[i].y = val * maxHeight;

        }

        filterA.mesh.vertices = verts;
        filterA.mesh.RecalculateBounds();
        filterA.mesh.RecalculateNormals();
        filterA.transform.GetComponent<MeshCollider>().sharedMesh = filterA.mesh;

    }

    //Redundant
    public Texture2D GenerateNoiseTexture ()
    {
        Texture2D noise = new Texture2D(size, size);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float xCoord = (float)x / size*scale + offset.x;
                float yCoord = (float)y / size * scale + offset.y;


                float noiseValue = Noise(xCoord, yCoord);
                float fade = 1f;

                Color newColor = new Color(noiseValue*fade, noiseValue * fade, noiseValue * fade);

                noise.SetPixel(x, y, newColor);

            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float finalFade = 1f;
                int inc = 0;
                float getAverageNeighbours =1f;

                /*if (x > size - edgeFadeRadius || x < edgeFadeRadius || y > size - edgeFadeRadius || y < edgeFadeRadius)
                {

                    float fadeUp = 1f;
                    float fadeRight = 1f;

                    if (right && x > size - edgeFadeRadius)
                    {
                        fadeRight = Mathf.Lerp(0, 1, ((float)size - x) / (edgeFadeRadius));
                    }
                    
                    else if (left && x < edgeFadeRadius)
                    {
                        fadeRight = Mathf.Lerp(0, 1, ((float)x) / (edgeFadeRadius));
                    }

                    if (up && y > size - edgeFadeRadius)
                    {
                        fadeUp = Mathf.Lerp(0, 1, ((float)size - y) / (edgeFadeRadius));
                    }

                    else if (down && y < edgeFadeRadius)
                    {
                        fadeUp = Mathf.Lerp(0, 1, ((float)y) / (edgeFadeRadius));
                    }

                    finalFade = fadeRight * fadeUp;

                }
                */
                /*
                 for (int cX = x-neighbourBlend; cX < x+1+neighbourBlend; cX++)
                 {
                     for (int cY = y - neighbourBlend; cY < y + neighbourBlend + 1; cY++)
                     {
                         if (cX == x || cY == y)
                             continue;

                         if (cX > size - 1 || cY > size - 1 || cX < 0 || cY < 0)
                             continue;

                         inc++;
                         getAverageNeighbours += noise.GetPixel(cX, cY).r;

                     }
                 getAverageNeighbours /= inc;
                 }*/


                float v = noise.GetPixel(x, y).r * finalFade * getAverageNeighbours;

                Color newColor = new Color(v,v,v);

                noise.SetPixel(x, y, newColor);

            }
        }


        noise.Apply();

        return noise;

    }

    //Generating a noise texture in parallel (quicker)
    public Texture2D GenerateNoiseTexture(bool parallelT)
    {
        Texture2D noise = new Texture2D(size, size);

        //Keeping an empty array the size of the noise texture's dimensions
        float[] vals2 = new float[size*size];

        //Run a Parallel.For
        Parallel.For(0, size,
            x =>
            {
                //Double loop
                Parallel.For(0, size, y =>
                {
                    //Coordinate creation based on parameters
                    float xCoord = (float)x / size * scale + offset.x;
                    float yCoord = (float)y / size * scale + offset.y;

                    //Getting perlin noise
                    float noiseValue = Noise(xCoord, yCoord);
                    
                    //Fading not supported yet
                    float fade = 1f;

                    //Generating color from noiseValue
                    float newColor = (noiseValue * fade)*1f;

                    //Setting the value of the pixel in the float[] array as Texture2D's aren't supported in threads other than the main
                    vals2[x * size + y] = newColor;

                });

            });

        //Transporting values from float[] to Texture -- find a better way to bind these
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float a = vals2[x * size + y];
                noise.SetPixel(x, y, new Color(a,a,a));
            }
        }

        vals = vals2;

        //Applying texture
        noise.Apply();


        return noise;
    }

    //Create perlin noise using x & y input
    public float Noise (float x, float y)
    {

        return Mathf.PerlinNoise(x, y);
       
    }

    //Finding a point on the grid given X & Y
    public Point FindPoint (int x, int y)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].x == x && points[i].y == y)
            {
                return points[i];
            }
        }

        return new Point();
    }

    //Finding the texture coordinates + returning texture darkness float[] based on worldPos parameter
    public (int, int, float[] vals) FindTextureIndex ( Vector3 worldPos )
    {

        Vector3 localPos = worldPos;
        //float realXPos = trans.TransformPoint(localPos).x;
        //float realZPos = trans.TransformPoint(localPos).z;

        float realXPos = localPos.x;
        float realZPos = localPos.z;

        float percX = ((realXPos - min.x) * 100) / (max.x - min.x);
        float percY = ((realZPos - min.z) * 100) / (max.z - min.z);



        int pixelX = Mathf.Clamp(Mathf.RoundToInt((percX) / 100 * size), 0, size - 1);
        int pixelY = Mathf.Clamp(Mathf.RoundToInt((percY) / 100 * size), 0, size - 1);

        return (pixelY, pixelX, vals);
    }

   

}

public struct Point
{
    public int x;
    public int y;
    public int vertexIndex;
}
 
