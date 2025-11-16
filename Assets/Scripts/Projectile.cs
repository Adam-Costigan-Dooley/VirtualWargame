using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private Transform _target;
    private int _damage;

    public void Launch(Transform target, int damage)
    {
        _target = target;
        _damage = damage;
        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            _target.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, _target.position) < 0.1f)
        {
            Enemy enemy = _target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }
            Destroy(gameObject);
        }
    }
}
