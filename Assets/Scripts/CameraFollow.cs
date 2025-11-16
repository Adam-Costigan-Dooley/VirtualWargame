using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;

    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        var targetPos = transform.position;
        targetPos.x = Mathf.SmoothDamp(transform.position.x, target.position.x, ref _velocity.x, smoothTime);

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);

        transform.position = targetPos;
    }
}
