using Rokid.UXR.Module;
using UnityEngine;

public struct BoundedPlane
{
    public long planeHandle;
    public Vector2[] boundary;//局部坐标
    public Vector3[] boundary3D;//世界坐标
    public Pose pose;

    public PlaneType planeType;
    public override string ToString()
    {
        string boundaryStr = "\r\n";
        string boundary3DStr = "\r\n";
        if (boundary?.Length > 0)
        {
            for (int i = 0; i < boundary.Length; i++)
            {
                boundaryStr += $"({boundary[i].x},{boundary[i].y})\r\n";
                boundary3DStr += $"({boundary3D[i].x},{boundary3D[i].y},{boundary3D[i].z})\r\n";
            }
        }
        return $"planeId:{planeHandle} \r\nplaneType:{planeType} \r\npose:\r\n{pose.position.ToString("0.0000")}\r\n{pose.rotation.eulerAngles.ToString("0.0000")} \r\nboundary3D:{boundary3DStr}\r\nboundary:{boundaryStr} ";
    }


    public void release()
    {
        boundary = null;
        boundary3D = null;
    }
}
