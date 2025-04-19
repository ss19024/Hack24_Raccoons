using System;
using UnityEngine;

namespace Rokid.UXR.Module
{

    public class ARTrackedImage
    {
        public int index;
        public Pose pose;
        public Vector2 size;
        public float sizeScale;
        public Bounds bounds;

        public ARTrackedImage()
        {

        }


        public ARTrackedImage(int index)
        {
            this.index = index;
        }

        public ARTrackedImage(string index, Pose pose, Vector2 size)
        {
            this.index = Convert.ToInt32(index);
            this.pose = pose;
            this.size = size;
            this.sizeScale = Mathf.Sqrt(this.size.x * this.size.x + this.size.y * this.size.y);
            this.bounds = new Bounds(this.pose.position, new Vector3(size.x, size.y, 0));
        }

        public override string ToString()
        {
            return $"\r\nIndex:{index}\r\nPose:{pose.position},{pose.rotation.eulerAngles}\r\nSize:{size}";
        }
    }
}

