using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Threading.Tasks;


public class VertexPainter : EditorWindow
{
    [MenuItem("Window/UIElements/VertexPainter")]
    public static void ShowExample()
    {
        VertexPainter wnd = GetWindow<VertexPainter>();
        wnd.titleContent = new GUIContent("VertexPainter");
    }

    private SerializedObject presetManagerSerialized;
    private ObjectField presetObjectField;
    Texture2D temporaryImage;
    public void OnEnable()
    {
    }

    NoiseGenerator editorTemporaryNoiseGenerator;
    private void OnGUI()
    {   
        //Incomplete
        if (GUILayout.Button("Save Terrain"))
        {

        }

        if (GUILayout.Button("Load Terrain"))
        {

        }
    }
    float scale = 1;
    //Vertex Painting Code?

    private Texture2D Conv(Texture2D tex, int newWidth, int newHeight)
    {
        Texture2D result = new Texture2D(tex.width, tex.height, tex.format, false);
        float incX = (1.0f / (float)newWidth);
        float incY = (1.0f / (float)newHeight);
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = tex.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        return result;
    }
    private void Update()
    {
        Vector3 mP = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mP);
        RaycastHit hit;

        Vector3 centerPoint = Vector3.zero;

        //This code manages finding the points that you have hovered over when the game is playing
        if (Physics.Raycast(ray, out hit))
        {

            editorTemporaryNoiseGenerator = hit.transform.root.root.GetComponent<NoiseGenerator>();

            Vector3[] vertices = hit.transform.GetComponent<MeshFilter>().mesh.vertices;
            int[] triangles = hit.transform.GetComponent<MeshFilter>().mesh.triangles;

            Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
            Transform hitTransform = hit.collider.transform;
            p0 = hitTransform.TransformPoint(p0);
            p1 = hitTransform.TransformPoint(p1);
            p2 = hitTransform.TransformPoint(p2);

            Debug.DrawLine(p0, p1, Color.red, 0.2f);
            Debug.DrawLine(p1, p2, Color.red, 0.2f);
            Debug.DrawLine(p2, p0, Color.red, 0.2f);

            //Find the average point on the mesh where your mouse is
            centerPoint = (p0 + p1 + p2) / 3;
            Debug.DrawLine(centerPoint, centerPoint + Vector3.up * 2, Color.green, 0.2f);

        }
        else
            editorTemporaryNoiseGenerator = null;

        if (editorTemporaryNoiseGenerator == null)
            return;
       
        Texture2D brush = new Texture2D(0, 0);

        editorTemporaryNoiseGenerator.brushScale = Mathf.Clamp(editorTemporaryNoiseGenerator.brushScale+Mathf.RoundToInt(Input.GetAxis("Mouse ScrollWheel")),1, 6);
       
        float intensity2 = 0f;
        if (editorTemporaryNoiseGenerator != null && editorTemporaryNoiseGenerator.brush != null)
        {

            //Intensity of the brsuh
            intensity2 = editorTemporaryNoiseGenerator.intensity;

            if (scale != editorTemporaryNoiseGenerator.brushScale)
            {
                //Supporting the scaling of the brush (settings can be changed in NoiseGenerator.cs)
                TextureScale.Bilinear(editorTemporaryNoiseGenerator.brush, Mathf.RoundToInt(editorTemporaryNoiseGenerator.initialResolution.x * editorTemporaryNoiseGenerator.brushScale), Mathf.RoundToInt(editorTemporaryNoiseGenerator.initialResolution.y * editorTemporaryNoiseGenerator.brushScale));
            }

            //Setting the current brush to the noise generator's brush (can change per terrain)
            brush = editorTemporaryNoiseGenerator.brush;
            scale = editorTemporaryNoiseGenerator.brushScale;
            
        }
        

        if (Input.GetMouseButton(0))
        {
            NoiseGenerator g = editorTemporaryNoiseGenerator;
            if (g == null)
                return;
            (int a, int b, float[] vals) = g.FindTextureIndex(centerPoint);

            int size = g.size;

            int xtemp = 0;
            for (int x = a - (brush.width)/2; x < a + (brush.width) / 2; x++)
            {
                
                int ytemp = 0;
                for (int y = b - (brush.height) / 2; y < b + (brush.height) / 2; y++)
                {

                    if (x < 0 || y < 0)
                    {
                        //Here is where neighbour blending will occur
                        continue;
                    }
                    if (x > size - 1 || y > size - 1)
                    {
                        //Here is where neighbour blending will occur
                        continue;
                    }
                    bool smoothing = false;
                    int mult = 1;
                    if (Input.GetKey(KeyCode.LeftShift))
                        mult = -1;

                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        smoothing = true;
                    }

                    float ar = 0;

                    if (!smoothing)
                    {
                        //Convert the intensity to the local coordinates of the texture (rgba).
                        float intensity = brush.GetPixel(xtemp, ytemp).a * intensity2 * mult;

                        ar = vals[x * size + y] + (0.03f * intensity);
                    }
                    else
                    {
                        float intensity = brush.GetPixel(xtemp, ytemp).a * intensity2 * mult;
                        ar = Mathf.Lerp(vals[x * size + y], 0, 0.2f* intensity);
                    }
                    g.matTex.SetPixel(x, y, new Color(ar, ar, ar));
                    vals[x * size + y] = ar;

                    ytemp++;
                }

                xtemp++;
            }

            g.vals = vals;
            g.matTex.Apply();
            
            //Registering the change in the texture -> updates the mesh
            g.RegsiterChange();
        }
    }
}