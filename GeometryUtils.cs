using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Nesting.Geometry
{
    using UnityEngine;

    public static class GeometryUtils
    {

        static double TOL = Math.Pow(10d, -9d);

        public static bool AlmostEqual(double a, double b, double? tolerance = 0)
        {
            if (tolerance == null || tolerance.Value == 0)
            {
                tolerance = TOL;
            }
            return Math.Abs(a - b) < tolerance;
        }

        public static float PolygonArea(this IEnumerable<Vector2> vertices)
        {
            if (vertices.Count() < 3)
                return 0;
            var area = 0f;
            var last = vertices.Last();
            foreach (var current in vertices)
            {
                area += (last.x + current.x) * (last.y - current.y);
                last = current;
            }

            return 0.5f * area;
        }

        public static IEnumerable<Vector2> RotatePolygon(this IEnumerable<Vector2> vertices, float angle)
        {
            angle = angle * (float)Math.PI / 180f;

            return vertices.Select(p => new Vector2((float)(p.x * Math.Cos(angle) - p.y * Math.Sin(angle)), (float)(p.x * Math.Sin(angle) + p.y * Math.Cos(angle))));//.ToList();

        }

        ////TRS矩阵执行平移translate，旋转angle角度和缩放scale比例
        public static IEnumerable<Vector2> TRS(this IEnumerable<Vector2> vertices, Vector2 translate, float angle, Vector2 scale)
        {
            angle = angle * (float)Math.PI / 180f;

            return vertices.Select(p => new Vector2(((float)(p.x * Math.Cos(angle) - p.y * Math.Sin(angle)) + translate.x) * scale.x, ((float)(p.x * Math.Sin(angle) + p.y * Math.Cos(angle)) + translate.y) * scale.y));//.ToList();

        }

        //new-----------
        public static List<Vector2> TRS(this List<Vector2> vertices, Vector2 translate, float angle, Vector2 scale)
        {
            angle = angle * (float)Math.PI / 180f;

            return vertices.Select(p => new Vector2(((float)(p.x * Math.Cos(angle) - p.y * Math.Sin(angle)) + translate.x) * scale.x, ((float)(p.x * Math.Sin(angle) + p.y * Math.Cos(angle)) + translate.y) * scale.y)).ToList();


        }
        //new-----------


        public static List<List<Vector2>> TRS(this List<List<Vector2>> vertices, Vector2 translate, float angle, Vector2 scale)
        {
            angle = angle * (float)Math.PI / 180f;

            return vertices.Select(path => path.Select(p => new Vector2(((float)(p.x * Math.Cos(angle) - p.y * Math.Sin(angle)) + translate.x) * scale.x, ((float)(p.x * Math.Sin(angle) + p.y * Math.Cos(angle)) + translate.y) * scale.y)).ToList()).ToList();

        }
    }
}
