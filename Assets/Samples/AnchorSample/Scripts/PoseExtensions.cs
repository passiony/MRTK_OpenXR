using UnityEngine;

public static class TransformExtensions
{
    public static Pose GetLocalPose(this Transform transform)
    {
        return new Pose(transform.localPosition, transform.localRotation);
    }

    public static Pose GetGlobalPose(this Transform transform)
    {
        return new Pose(transform.position, transform.rotation);
    }

    public static void SetLocalPose(this Transform transform, Pose pose)
    {
        transform.localPosition = pose.position;
        transform.localRotation = pose.rotation;
    }

    public static void SetGlobalPose(this Transform transform, Pose pose)
    {
        transform.position = pose.position;
        transform.rotation = pose.rotation;
    }
}

/// <summary>
/// Extensions for Poses to enable basic transform math.
/// </summary>
public static class PoseExtensions
{
    /*
     * application of a transform on a position, defined such that:
     * transform.position == FromGlobal(parent.transform) * localPosition
     */
    public static Vector3 Multiply(this Pose pose, Vector3 position)
    {
        return pose.position + pose.rotation * position;
    }

    /*
     * chaining of transforms, defined such that
     * V' = lhs * (rhs * V)
     *    = (lhs.pos,lhs.rot) * (rhs.pos + rhs.rot * V)
     *    = lhs.pos + lhs.rot * (rhs.pos + rhs.rot * V)
     *    = lhs.pos + lhs.rot * rhs.pos + lhs.rot * rhs.rot * V
     *    = (lhs.pos + lhs.rot * rhs.pos , lhs.rot * rhs.rot) * V
     *    = (lhs * rhs) * V
     */

    public static Pose Multiply(this Pose lhs, Pose rhs)
    {
        return new Pose(lhs.position + lhs.rotation * rhs.position, lhs.rotation * rhs.rotation);
    }

    /*
     * inverse of transform, defined such that
     * 1 == inv(t) * t == t * inv(t)
     * 
     *   inv(t) * t
     * = (-inv(t.rot)*t.pos , inv(t.rot)) * (t.pos, t.rot)
     * = (-inv(t.rot)*t.pos + inv(t.rot)  * t.pos , inv(t.rot) * t.rot)
     * = 1
     * 
     *   t * inv(t)
     * = (t.pos, t.rot) * (-inv(t.rot)*t.pos , inv(t.rot))
     * = (t.pos + t.rot * (-inv(t.rot)*t.pos) , t.rot * inv(t.rot))
     * = 1
     */
    public static Pose Inverse(this Pose t)
    {
        var inv_t_rotation = Quaternion.Inverse(t.rotation);
        return new Pose(-(inv_t_rotation * t.position), inv_t_rotation);
    }
}