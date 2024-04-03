using System;
using UnityEngine;
using DG.Tweening;

public class ProjectileMover : MonoBehaviour
{
    public AnimationCurve trajectoryCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    private Transform target;
    private Tween tween;

    public void MoveToTarget(Transform target, float duration)
    {
        this.target = target;
        if (target == null)
        {
            Debug.LogError("Target is not set.");
            return;
        }

        Invoke("Destroy", duration + 0.15f);

        // Cancel the existing tween if it's still running
        if (tween != null && tween.IsPlaying())
        {
            tween.Kill();
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = target.position;

        tween = DOTween.To(() => 0f, value =>
        {
            float height = trajectoryCurve.Evaluate(value);
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, value);
            currentPosition.y += height;
            transform.position = currentPosition;
        }, 1f, duration).SetEase(Ease.Linear);
    }

    public void MoveToTarget(Vector3 target, float duration)
    {
        Invoke("Destroy", duration + 0.15f);

        // Cancel the existing tween if it's still running
        if (tween != null && tween.IsPlaying())
        {
            tween.Kill();
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = target;

        tween = DOTween.To(() => 0f, value =>
        {
            float height = trajectoryCurve.Evaluate(value);
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, value);
            currentPosition.y += height;
            transform.position = currentPosition;
        }, 1f, duration).SetEase(Ease.Linear);
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}