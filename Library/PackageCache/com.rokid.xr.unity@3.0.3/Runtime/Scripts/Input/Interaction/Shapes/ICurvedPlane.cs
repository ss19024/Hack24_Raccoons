
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Represents a curved rectangular section of a
    /// cylinder wall.
    /// </summary>
    public interface ICurvedPlane
    {
        /// <summary>
        /// The cylinder the curved plane lies on
        /// 曲面所在的圆柱体
        /// </summary>
        Cylinder Cylinder { get; }

        /// <summary>
        /// The horizontal size of the plane, in degrees
        /// 平面的水平尺寸，以度为单位
        /// </summary>
        float ArcDegrees { get; }

        /// <summary>
        /// The rotation of the center of the plane relative
        /// to the Cylinder's forward Z axis, in degrees
        /// 平面中心相对于圆柱体向前 Z 轴的旋转，以度为单位
        /// </summary>
        float Rotation { get; }

        /// <summary>
        /// The bottom of the plane relative to the
        /// Cylinder Y position, in Cylinder local space
        /// 平面的底部相对于
        /// 圆柱体Y位置，在圆柱体局部空间
        /// </summary>
        float Bottom { get; }

        /// <summary>
        /// The top of the plane relative to the
        /// Cylinder Y position, in Cylinder local space
        /// 平面的顶部相对于
        /// 圆柱体Y位置，在圆柱体局部空间
        /// </summary>
        float Top { get; }
    }

}
