using UnityEngine;

public class MoveController : MonoBehaviour
{
    [SerializeField] private RayController _rayController;
    [SerializeField] private GameObject _target;
    
    [SerializeField] [Range(0, 9)] private float speed = 5;
    [SerializeField] [Range(1, 360)] private float fov;
    [SerializeField] [Range(1, 10)] private float radius = 5;

    [SerializeField] private Color _defaultColor = Color.white;
    [SerializeField] private Color _triggerColor = Color.green;
    [SerializeField] private LayerMask _layerMask;

    SpriteRenderer sr;
    Rigidbody2D rb;
    Vector2 lastVelocity;

    private const float _xSize = 32;
    private const float _ySize = 18;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        SetDirection(GetRandomVector2());
        sr.color = _defaultColor;
    }

    void Update()
    {
        Vector2 position = transform.position;
        _rayController.SetDirection(lastVelocity);
        _rayController.SetOrigin(position);
        _rayController.SetFovAndRadius(fov, radius);
        FieldOfView(lastVelocity);
    }

    public void FixedUpdate()
    {
        lastVelocity = rb.velocity;
        CheckForFlipping();
        SlowdownDetector();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 reflection;

        if (collision.collider.CompareTag("Player") && collision.otherCollider.CompareTag("Player"))
        {
            reflection = Vector2.Reflect(lastVelocity.normalized, collision.GetContact(0).normal);
        }
        else
        {
            var wall = collision.collider.name == transform.name ? collision.otherCollider : collision.collider;
            reflection = GetPseudoRandomBorderReflection(wall.attachedRigidbody.transform.position);
        }
        SetDirection(reflection);
    }

    void SlowdownDetector()
    {
        if (rb.velocity.magnitude != speed)
        {
            {
                // To detect that speed slowed after collision
                Debug.LogWarning("Slowdown detected");
                SetDirection(lastVelocity);

                // To prevent cats stuck on objects
                if (rb.velocity.magnitude == 0)
                {
                    rb.AddForce(GetRandomVector2(), ForceMode2D.Impulse);
                    Debug.LogWarning("Added force!");
                }
            }
        }
    }

    Vector2 GetRandomVector2()
    {
        return new Vector2(Random.Range(-_xSize, _xSize), Random.Range(-_ySize, _ySize));
    }

    void SetDirection(Vector2 direction)
    {
        direction = direction.normalized;
        rb.velocity = direction * speed;
        lastVelocity = rb.velocity;
    }

    public Vector2 GetPseudoRandomBorderReflection(Vector2 positionBorder)
    {
        Vector2 vector;
        float minSize = 0.1f;

        //Top
        if (positionBorder.x == 0 && positionBorder.y > 0)
        {
            vector = new Vector2(Random.Range(-_xSize, _xSize), Random.Range(-_ySize, minSize)).normalized;

        }
        //Bot
        else if (positionBorder.x == 0 && positionBorder.y < 0)
        {
            vector = new Vector2(Random.Range(-_xSize, _xSize), Random.Range(minSize, _ySize)).normalized;

        }
        //Right
        else if (positionBorder.x > 0 && positionBorder.y == 0)
        {
            vector = new Vector2(Random.Range(-_xSize, minSize), Random.Range(-_ySize, _ySize)).normalized;
        }
        //Left
        else
        {
            vector = new Vector2(Random.Range(minSize, _xSize), Random.Range(-_ySize, _ySize)).normalized;
        }
        return vector;
    }

    private void CheckForFlipping()
    {
        bool movingLeft = lastVelocity.x < 0;
        bool movingRight = lastVelocity.x > 0;

        if (movingLeft)
        {
            transform.localScale = new Vector3(-2f, transform.localScale.y);
        }
        if (movingRight)
        {
            transform.localScale = new Vector3(2f, transform.localScale.y);
        }
    }

    public void FieldOfView(Vector2 direction)
    {
        var hitObstacle = Physics2D.CircleCast(transform.position, radius, direction, 0f, _layerMask);
         
        if (hitObstacle.collider != null)
        {
            Vector2 targetPosition = _target.transform.position;
            Vector2 position = transform.position;
            Vector2 directionToTarget = (targetPosition - position).normalized;

            float foundTarget = Vector2.Angle(direction.normalized, directionToTarget);
            sr.color = foundTarget <= fov / 2 ? _triggerColor : _defaultColor;
        }
        else
        {
            sr.color = _defaultColor;
        }
    }
}
