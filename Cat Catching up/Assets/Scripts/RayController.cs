using UnityEngine;

public class RayController : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;

    private Mesh _mesh;

    private float _fov;
    private float _radius;

    private Vector3 _origin;
    private float _startingAngle = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        _origin = Vector3.zero;
    }

    public void SetFovAndRadius(float fov, float radius)
    {
        _fov = fov;
        _radius = radius;
    }

    private void LateUpdate()
    {
        int rayCount = 50;
        float angle = _startingAngle;
        float angleIncrease = _fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = _origin;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;

            RaycastHit2D raycastHit2D = Physics2D.Raycast(_origin, GetVectorFromAngle(angle), _radius, _layerMask);

            if (raycastHit2D.collider == null)
            {
                vertex = _origin + GetVectorFromAngle(angle) * _radius;
            }
            else
            {
                vertex = raycastHit2D.point;
            }
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }
            vertexIndex++;
            angle -= angleIncrease;
        }

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
    }

    public static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public void SetOrigin(Vector3 origin)
    {
        _origin = origin;
    }

    public void SetDirection(Vector3 direction)
    {
        _startingAngle = GetAngleFromVectorFloat(direction) + _fov / 2f;
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;        
        return n;
    }
}


