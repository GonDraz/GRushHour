using UnityEngine;

namespace GonDraz.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        ///     Returns a new Vector3 with the X value changed.
        ///     (Trả về Vector3 mới với giá trị X thay đổi)
        /// </summary>
        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        /// <summary>
        ///     Returns a new Vector3 with the Y value changed.
        ///     (Trả về Vector3 mới với giá trị Y thay đổi)
        /// </summary>
        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        /// <summary>
        ///     Returns a new Vector3 with the Z value changed.
        ///     (Trả về Vector3 mới với giá trị Z thay đổi)
        /// </summary>
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        ///     Converts a Vector3 to Vector2 (drops Z).
        ///     (Chuyển Vector3 sang Vector2, bỏ trục Z)
        /// </summary>
        public static Vector2 To2D(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        ///     Converts a Vector2 to Vector3 (adds Z).
        ///     (Chuyển Vector2 sang Vector3, thêm trục Z)
        /// </summary>
        public static Vector3 To3D(this Vector2 v, float z = 0)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        ///     Checks if the distance between two vectors is less than a threshold.
        ///     (Kiểm tra khoảng cách giữa hai vector nhỏ hơn ngưỡng cho trước)
        /// </summary>
        public static bool IsCloseTo(this Vector3 v, Vector3 other, float threshold = 0.01f)
        {
            return Vector3.Distance(v, other) < threshold;
        }

        /// <summary>
        ///     Returns the normalized direction from one vector to another.
        ///     (Trả về hướng chuẩn hoá từ một vector đến vector khác)
        /// </summary>
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }

        /// <summary>
        ///     Returns a random Vector2 inside a circle with given radius.
        ///     (Trả về Vector2 ngẫu nhiên trong hình tròn bán kính radius)
        /// </summary>
        public static Vector2 RandomInsideCircle(float radius = 1f)
        {
            return Random.insideUnitCircle * radius;
        }

        /// <summary>
        ///     Returns a random Vector3 inside a sphere with given radius.
        ///     (Trả về Vector3 ngẫu nhiên trong hình cầu bán kính radius)
        /// </summary>
        public static Vector3 RandomInsideSphere(float radius = 1f)
        {
            return Random.insideUnitSphere * radius;
        }

        /// <summary>
        ///     Returns the squared distance between two vectors.
        ///     (Trả về bình phương khoảng cách giữa hai vector)
        /// </summary>
        public static float SqrDistance(this Vector3 v, Vector3 other)
        {
            return (v - other).sqrMagnitude;
        }

        /// <summary>
        ///     Returns the angle in degrees between two vectors.
        ///     (Trả về góc giữa hai vector, đơn vị độ)
        /// </summary>
        public static float AngleTo(this Vector3 from, Vector3 to)
        {
            return Vector3.Angle(from, to);
        }

        /// <summary>
        ///     Returns a vector with each component clamped between min and max.
        ///     (Trả về vector với từng thành phần bị giới hạn trong khoảng min-max)
        /// </summary>
        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(v.x, min.x, max.x),
                Mathf.Clamp(v.y, min.y, max.y),
                Mathf.Clamp(v.z, min.z, max.z)
            );
        }

        /// <summary>
        ///     Returns a vector with the absolute value of each component.
        ///     (Trả về vector với giá trị tuyệt đối từng thành phần)
        /// </summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        /// <summary>
        ///     Returns a vector with each component rounded to the nearest integer.
        ///     (Trả về vector với từng thành phần được làm tròn)
        /// </summary>
        public static Vector3 Round(this Vector3 v)
        {
            return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }

        /// <summary>
        ///     Returns a vector with each component floored.
        ///     (Trả về vector với từng thành phần làm tròn xuống)
        /// </summary>
        public static Vector3 Floor(this Vector3 v)
        {
            return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
        }

        /// <summary>
        ///     Returns a vector with each component ceiled.
        ///     (Trả về vector với từng thành phần làm tròn lên)
        /// </summary>
        public static Vector3 Ceil(this Vector3 v)
        {
            return new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));
        }

        /// <summary>
        ///     Returns true if all components of the vector are approximately zero.
        ///     (Trả về true nếu tất cả thành phần của vector gần bằng 0)
        /// </summary>
        public static bool IsZero(this Vector3 v, float epsilon = 1e-6f)
        {
            return Mathf.Abs(v.x) < epsilon && Mathf.Abs(v.y) < epsilon && Mathf.Abs(v.z) < epsilon;
        }

        /// <summary>
        ///     Returns a vector with the minimum of each component from two vectors.
        ///     (Trả về vector với từng thành phần là giá trị nhỏ nhất giữa hai vector)
        /// </summary>
        public static Vector3 Min(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        /// <summary>
        ///     Returns a vector with the maximum of each component from two vectors.
        ///     (Trả về vector với từng thành phần là giá trị lớn nhất giữa hai vector)
        /// </summary>
        public static Vector3 Max(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        /// <summary>
        ///     Returns a vector with the sign of each component.
        ///     (Trả về vector với dấu của từng thành phần)
        /// </summary>
        public static Vector3 Sign(this Vector3 v)
        {
            return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));
        }

        /// <summary>
        ///     Returns a vector with each component normalized to [0,1] given min and max.
        ///     (Chuẩn hoá từng thành phần vector về [0,1] dựa trên min/max)
        /// </summary>
        public static Vector3 Normalize01(this Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.InverseLerp(min.x, max.x, v.x),
                Mathf.InverseLerp(min.y, max.y, v.y),
                Mathf.InverseLerp(min.z, max.z, v.z)
            );
        }

        // Snaps the Z-coordinate of a Vector3 to 0
        public static Vector3 SnapZ(this Vector3 v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        // Snaps the Z-coordinate of a Vector3 to -10 (suitable for cameras)
        public static Vector3 SnapZCamera(this Vector3 v)
        {
            return new Vector3(v.x, v.y, -10);
        }

        // Adjusts the x-coordinate of a Vector2Int if its y-coordinate is even
        public static Vector2Int SnapRow(this Vector2Int v)
        {
            if (v.y % 2 == 0) v.x -= 1;

            return v;
        }

        // Returns the next Vector2Int in a specified direction from the current Vector2Int
        private static Vector2Int GetNext(this Vector2Int v, Vector2Int dir)
        {
            if (dir.y != 0)
            {
                if (v.y % 2 == 0 && dir.x < 0)
                    dir.x = 0;
                else if (v.y % 2 == 1 && dir.x > 0)
                    dir.x = 0;
            }

            return v + dir;
        }

        // Overload of GetNext that accepts two integers as direction
        public static Vector2Int GetNext(this Vector2Int v, int x, int y)
        {
            return v.GetNext(new Vector2Int(x, y));
        }

        public static Vector2Int ToV2Int(this Vector2 v)
        {
            return new Vector2Int((int)v.x, (int)v.y);
        }

        public static Vector2Int ToV2Int(this Vector3 v)
        {
            return new Vector2Int((int)v.x, (int)v.y);
        }
    }
}