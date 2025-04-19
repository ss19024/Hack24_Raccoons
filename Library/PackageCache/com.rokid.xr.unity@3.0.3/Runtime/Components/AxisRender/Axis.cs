using UnityEngine;

namespace Rokid.UXR.Components
{
    public class Axis : MonoBehaviour
    {
        [Range(0.2f, 0.5f)]
        public float arrowBottomSize = 0.2f;
        [Range(0.2f, 0.5f)]
        public float arrowHeight = 0.3f;
        [Range(0.2f, 5)]
        public float xLength = 2;
        [Range(0.2f, 5)]
        public float yLength = 2;
        [Range(0.2f, 5)]
        public float zLength = 2;

        [Range(0.05f, 0.1f)]
        public float lineTickness = 0.05f;//轴线的粗细

        [Range(0.1f, 1)]
        public float centerSize = 0.1f;


        void Start()
        {
            transform.Find("X").GetComponent<MeshFilter>().mesh = DrawArrow(arrowHeight, arrowBottomSize, xLength, lineTickness, lineTickness);
            transform.Find("Y").GetComponent<MeshFilter>().mesh = DrawArrow(arrowHeight, arrowBottomSize, yLength, lineTickness, lineTickness);
            transform.Find("Z").GetComponent<MeshFilter>().mesh = DrawArrow(arrowHeight, arrowBottomSize, zLength, lineTickness, lineTickness);
            UpdateArrowTsf();
        }


        /// <summary>
        /// inspector 数据发生改变是调用
        /// </summary>
        void OnValidate()
        {
            // transform.Find("X").GetComponent<MeshFilter>().mesh = DrawArrow(arrowHeight, arrowBottomSize, xLength, lineTickness, lineTickness);
            // transform.Find("Y").GetComponent<MeshFilter>().mesh = DrawArrow(arrowHeight, arrowBottomSize, yLength, lineTickness, lineTickness);
            // transform.Find("Z").GetComponent<MeshFilter>().mesh = DrawArrow(arrowHeight, arrowBottomSize, zLength, lineTickness, lineTickness);
            // UpdateArrowTsf();
        }

        /// <summary>
        /// 根据输入箭头的长度不同更新箭头的位置
        /// </summary>
        void UpdateArrowTsf()
        {
            transform.Find("X").localPosition = new Vector3(xLength, 0, 0);
            transform.Find("Y").localPosition = new Vector3(0, yLength, 0);
            transform.Find("Z").localPosition = new Vector3(0, 0, zLength);
            transform.Find("Center").localScale = Vector3.one * centerSize;
        }


        /// <summary>
        /// 绘制箭头
        /// </summary>
        /// <param name="arrowHeight"></param>
        /// <param name="arrowBottom"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Mesh DrawArrow(float arrowHeight, float arrowBottom, float height, float width, float length)
        {
            var arrow = new Mesh();
            arrow.Clear();
            Vector3[] vertexs = new Vector3[13];
            //箭头
            vertexs[0] = new Vector3(-arrowBottom / 2, 0, -arrowBottom / 2);
            vertexs[1] = new Vector3(arrowBottom / 2, 0, -arrowBottom / 2);
            vertexs[2] = new Vector3(arrowBottom / 2, 0, arrowBottom / 2);
            vertexs[3] = new Vector3(-arrowBottom / 2, 0, arrowBottom / 2);
            vertexs[4] = new Vector3(0, arrowHeight, 0);
            //箭头尾部的线段
            vertexs[5] = new Vector3(-width / 2, 0, -length / 2);
            vertexs[6] = new Vector3(width / 2, 0, -length / 2);
            vertexs[7] = new Vector3(width / 2, 0, length / 2);
            vertexs[8] = new Vector3(-width / 2, 0, length / 2);

            vertexs[9] = new Vector3(-width / 2, -height, -length / 2);
            vertexs[10] = new Vector3(width / 2, -height, -length / 2);
            vertexs[11] = new Vector3(width / 2, -height, length / 2);
            vertexs[12] = new Vector3(-width / 2, -height, length / 2);
            arrow.vertices = vertexs;
            //顶点法线遵循左手螺旋定则
            int[] vertexIndices = new int[] { 
	            //bottom
	            0,1,2,0,2,3,
	            //up01
	            0,4,1,
	            //up02
	            1,4,2,
	            //up03
	            2,4,3,
	            //up04
	            0,3,4,
	            //箭头尾部...
	            5,10,9,5,6,10,
                6,11,10,6,7,11,
                7,12,11,7,8,12,
                5,9,12,5,12,8,
                9,10,12,10,11,12
            };
            arrow.vertices = vertexs;
            //自动计算法线
            arrow.RecalculateNormals();
            arrow.SetIndices(vertexIndices, MeshTopology.Triangles, 0);
            return arrow;
        }
    }
}
