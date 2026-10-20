using UnityEngine;

public sealed class BRHumanoidAnimatorDriver : MonoBehaviour
{
    public Animator targetAnimator;
    private Vector3 previousPosition;
    private CharacterController controller;

    private void Awake()
    {
        if (targetAnimator == null) targetAnimator = GetComponentInChildren<Animator>(true);
        controller = GetComponent<CharacterController>();
        previousPosition = transform.position;
        if (targetAnimator != null) targetAnimator.applyRootMotion = false;
    }

    private void Update()
    {
        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null) return;
        float dt = Mathf.Max(Time.deltaTime, .0001f);
        Vector3 velocity = (transform.position - previousPosition) / dt;
        previousPosition = transform.position;
        velocity.y = 0f;
        float speed = Mathf.Clamp01(velocity.magnitude / 7.5f);
        bool grounded = controller == null || controller.isGrounded;
        SetFloat("Speed", speed); SetFloat("speed", speed); SetFloat("MotionSpeed", speed);
        SetBool("Grounded", grounded); SetBool("IsGrounded", grounded);
    }

    private void SetFloat(string name, float value)
    {
        foreach (AnimatorControllerParameter p in targetAnimator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Float)
            {
                targetAnimator.SetFloat(name, value, .08f, Time.deltaTime);
                return;
            }
    }

    private void SetBool(string name, bool value)
    {
        foreach (AnimatorControllerParameter p in targetAnimator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Bool)
            {
                targetAnimator.SetBool(name, value);
                return;
            }
    }
}
