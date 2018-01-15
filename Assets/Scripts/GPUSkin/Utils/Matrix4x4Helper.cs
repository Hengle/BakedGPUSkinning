using UnityEngine;
using System.Collections;

namespace SDGame.Util
{
    /// <summary>
    /// Unity 的矩阵运算方法太少了，我们给它加几个
    /// </summary>
    public static class Matrix4x4Helper
    {
        public static Matrix4x4 projectiveBias;
        public static Matrix4x4 zero = Matrix4x4.zero;
        public static Matrix4x4 identity = Matrix4x4.identity;

        static Matrix4x4Helper()
        {
            projectiveBias = Matrix4x4.identity;
            projectiveBias.m00 = 0.5f;
            projectiveBias.m03 = 0.5f;
            projectiveBias.m11 = 0.5f;
            projectiveBias.m13 = 0.5f;
            projectiveBias.m22 = 0.5f;
            projectiveBias.m23 = 0.5f;
        }

        
        /// <summary>
        /// 矩阵标量乘
        /// </summary>
        /// <param name="m"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Matrix4x4 MultiplyScalar(Matrix4x4 m, float s)
        {
            Matrix4x4 result = zero;
            MultiplyScalar(m, s, ref result);
            return result;
        }

        public static void MultiplyScalar(Matrix4x4 m, float s, ref Matrix4x4 result)
        {
            result.m00 = m.m00 * s;
            result.m01 = m.m01 * s;
            result.m02 = m.m02 * s;
            result.m03 = m.m03 * s;
            result.m10 = m.m10 * s;
            result.m11 = m.m11 * s;
            result.m12 = m.m12 * s;
            result.m13 = m.m13 * s;
            result.m20 = m.m20 * s;
            result.m21 = m.m21 * s;
            result.m22 = m.m22 * s;
            result.m23 = m.m23 * s;
            result.m30 = m.m30 * s;
            result.m31 = m.m31 * s;
            result.m32 = m.m32 * s;
            result.m33 = m.m33 * s;
        }

        /// <summary>
        /// 矩阵加
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix4x4 Add(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result = a;
            for (int i = 0; i < 16; i++)
                result[i] += b[i];
            return result;
        }

        /// <summary>
        /// 矩阵加
        /// </summary>
        public static void Add(Matrix4x4 a, Matrix4x4 b, ref Matrix4x4 r)
        {
            r.m00 = a.m00 + b.m00;
            r.m01 = a.m01 + b.m01;
            r.m02 = a.m02 + b.m02;
            r.m03 = a.m03 + b.m03;
            r.m10 = a.m10 + b.m10;
            r.m11 = a.m11 + b.m11;
            r.m12 = a.m12 + b.m12;
            r.m13 = a.m13 + b.m13;
            r.m20 = a.m20 + b.m20;
            r.m21 = a.m21 + b.m21;
            r.m22 = a.m22 + b.m22;
            r.m23 = a.m23 + b.m23;
            r.m30 = a.m30 + b.m30;
            r.m31 = a.m31 + b.m31;
            r.m32 = a.m32 + b.m32;
            r.m33 = a.m33 + b.m33;
        }

        /// <summary>
        /// 分解出矩阵里的缩放、旋转、平移分量
        /// </summary>
        /// <param name="matrix">被分解的矩阵[this]</param>
        /// <param name="scale">缩放分量</param>
        /// <param name="rotation">旋转分量</param>
        /// <param name="translation">平移分量</param>
        public static void Decompose(Matrix4x4 matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            Vector4 col0 = matrix.GetColumn(0);
            Vector4 col1 = matrix.GetColumn(1);
            Vector4 col2 = matrix.GetColumn(2);

            scale.x = Mathf.Sqrt(col0.x * col0.x + col0.y * col0.y + col0.z * col0.z);
            scale.y = Mathf.Sqrt(col1.x * col1.x + col1.y * col1.y + col1.z * col1.z);
            scale.z = Mathf.Sqrt(col2.x * col2.x + col2.y * col2.y + col2.z * col2.z);
            rotation = Quaternion.LookRotation(new Vector3(col2.x, col2.y, col2.z), new Vector3(col1.x, col1.y, col1.z));
            translation = matrix.GetColumn(3);
        }

        /// <summary>
        /// 从一个矩阵中提取旋转
        /// </summary>
        /// <param name="matrix">要提取的矩阵</param>
        /// <returns>旋转</returns>
        public static Quaternion ExtractRotation(Matrix4x4 matrix)
        {
            Vector4 col1 = matrix.GetColumn(1);
            Vector4 col2 = matrix.GetColumn(2);
            Quaternion result = Quaternion.LookRotation(new Vector3(col2.x, col2.y, col2.z), new Vector3(col1.x, col1.y, col1.z));
            return result;
        }

        /// <summary>
        /// 建立一个平面反射矩阵
        /// </summary>
        /// <param name="p">平面</param>
        /// <returns></returns>
        public static Matrix4x4 MakePlaneReflection(Plane p)
        {
            Matrix4x4 result = new Matrix4x4();
            Vector3 n = p.normal;
            float d = p.distance;

            result.m00 = -2.0f * n.x * n.x + 1.0f;
            result.m01 = -2.0f * n.x * n.y;
            result.m02 = -2.0f * n.x * n.z;
            result.m03 = -2.0f * n.x * d;

            result.m10 = -2.0f * n.y * n.x;
            result.m11 = -2.0f * n.y * n.y + 1.0f;
            result.m12 = -2.0f * n.y * n.z;
            result.m13 = -2.0f * n.y * d;

            result.m20 = -2.0f * n.z * n.x;
            result.m21 = -2.0f * n.z * n.y;
            result.m22 = -2.0f * n.z * n.z + 1.0f;
            result.m23 = -2.0f * n.z * d;

            result.m30 = 0.0f;
            result.m31 = 0.0f;
            result.m32 = 0.0f;
            result.m33 = 1.0f;

            return result;
        }

        /// <summary>
        /// 建立照相机矩阵
        /// </summary>
        /// <param name="eye">照相机位置</param>
        /// <param name="dir">照相机查看方向</param>
        /// <param name="up">照相机正上方方向</param>
        /// <returns>照相机矩阵</returns>
        public static Matrix4x4 MakeViewMatrix(Vector3 eye, Vector3 dir, Vector3 up)
        {
            float dot = Vector3.Dot(dir, up);
            up = up - dot * dir;
            up.Normalize();

            Vector3 right = Vector3.Cross(dir, up);
            right.Normalize();

            Matrix4x4 result = new Matrix4x4();

            result.m00 =  right.x;
            result.m10 =  up.x;
            result.m20 = -dir.x;
            result.m30 =  0.0f;

            result.m01 =  right.y;
            result.m11 =  up.y;
            result.m21 = -dir.y;
            result.m31 =  0.0f;

            result.m02 =  right.z;
            result.m12 =  up.z;
            result.m22 = -dir.z;
            result.m32 =  0.0f;

            result.m03 = -Vector3.Dot(right, eye);
            result.m13 = -Vector3.Dot(up, eye);
            result.m23 =  Vector3.Dot(dir, eye);
            result.m33 =  1.0f;

            return result;
        }

        /// <summary>
        /// 修改投影矩阵的近裁剪面，一般用于平面反射
        /// </summary>
        /// <param name="projection">当前的投影矩阵</param>
        /// <param name="view">当前的视图矩阵</param>
        /// <param name="plane">世界空间裁剪面</param>
        /// <returns>修改后的投影矩阵</returns>
        //public static Matrix4x4 MakeObliqueFrustum(Matrix4x4 projection, Matrix4x4 view, Plane clipPlane)
        //{
        //    Matrix4x4 viewIT = view.inverse.transpose;
        //    Plane planeInViewSpace = GeometryUtils.TransformPlane(clipPlane, viewIT);
        //    Vector4 p = new Vector4(planeInViewSpace.normal.x, planeInViewSpace.normal.y, planeInViewSpace.normal.z, planeInViewSpace.distance);
        //    Vector4 q = new Vector4();
        //    q.x = (Mathf.Sign(p.x) + projection.m02) / projection.m00;
        //    q.y = (Mathf.Sign(p.y) + projection.m12) / projection.m11;
        //    q.z = -1.0f;
        //    q.w = (1.0f + projection.m22) / projection.m23;
        //    Vector4 c = p * (2.0f / Vector4.Dot(p, q));
        //    Matrix4x4 result = projection;
        //    result.m20 = c.x;
        //    result.m21 = c.y;
        //    result.m22 = c.z + 1.0f;
        //    result.m23 = c.w;
        //    return result;
        //}

        public static void MakeIdentity(ref Matrix4x4 m)
        {
            m.m00 = 1.0f; m.m01 = 0.0f; m.m02 = 0.0f; m.m03 = 0.0f;
            m.m10 = 0.0f; m.m11 = 1.0f; m.m12 = 0.0f; m.m13 = 0.0f;
            m.m20 = 0.0f; m.m21 = 0.0f; m.m22 = 1.0f; m.m23 = 0.0f;
            m.m30 = 0.0f; m.m31 = 0.0f; m.m32 = 0.0f; m.m33 = 1.0f;
        }

        /// <summary>
        /// 建立一个平移矩阵
        /// </summary>
        /// <param name="dx">x轴平移距离</param>
        /// <param name="dy">y轴平移距离</param>
        /// <param name="dz">z轴平移距离</param>
        /// <returns>平移矩阵</returns>
        public static Matrix4x4 MakeTranslate(float dx, float dy, float dz)
        {
            Matrix4x4 result = Matrix4x4.identity;
            MakeTranslate(dx, dy, dz, ref result);
            return result;
        }

        /// <summary>
        /// 建立一个平移矩阵
        /// </summary>
        /// <param name="dx">x轴平移距离</param>
        /// <param name="dy">y轴平移距离</param>
        /// <param name="dz">z轴平移距离</param>
        /// <param name="result">用于返回结果</param>
        public static void MakeTranslate(float dx, float dy, float dz, ref Matrix4x4 result)
        {
            result = Matrix4x4.identity;
            result.m03 = dx;
            result.m13 = dy;
            result.m23 = dz;
        }

        /// <summary>
        /// 建立一个缩放矩阵
        /// </summary>
        /// <param name="sx">x轴缩放比例</param>
        /// <param name="sy">y轴缩放比例</param>
        /// <param name="sz">z轴缩放比例</param>
        /// <returns>缩放矩阵</returns>
        public static Matrix4x4 MakeScaling(float sx, float sy, float sz)
        {
            Matrix4x4 result = Matrix4x4.identity;
            MakeScaling(sx, sy, sz, ref result);
            return result;
        }

        /// <summary>
        /// 建立一个缩放矩阵
        /// </summary>
        /// <param name="sx">x轴缩放比例</param>
        /// <param name="sy">y轴缩放比例</param>
        /// <param name="sz">z轴缩放比例</param>
        /// <param name="result">用于返回缩放矩阵</param>
        public static void MakeScaling(float sx, float sy, float sz, ref Matrix4x4 result)
        {
            result = Matrix4x4.identity;
            result.m00 = sx;
            result.m11 = sy;
            result.m22 = sz;
        }

        /// <summary>
        /// 从四元数构造一个旋转矩阵
        /// </summary>
        /// <param name="q">四元数</param>
        /// <returns>旋转矩阵</returns>
        public static Matrix4x4 MakeRotationQuaternion(Quaternion q)
        {
            Matrix4x4 result = Matrix4x4.identity;
            MakeRotationQuaternion(q, ref result);
            return result;
        }

        /// <summary>
        /// 从四元数构造一个旋转矩阵
        /// </summary>
        /// <param name="q">四元数</param>
        /// <param name="result">用于返回结果</param>
        public static void MakeRotationQuaternion(Quaternion q, ref Matrix4x4 result)
        {
            float xx = q.x * q.x;
            float yy = q.y * q.y;
            float zz = q.z * q.z;
            float xy = q.x * q.y;
            float xz = q.x * q.z;
            float xw = q.x * q.w;
            float yz = q.y * q.z;
            float yw = q.y * q.w;
            float zw = q.z * q.w;

            result = Matrix4x4.identity;
            result.m00 = 1.0f - 2.0f * (yy + zz);
            result.m10 = 2.0f * (xy + zw);
            result.m20 = 2.0f * (xz - yw);

            result.m01 = 2.0f * (xy - zw);
            result.m11 = 1.0f - 2.0f * (xx + zz);
            result.m21 = 2.0f * (yz + xw);

            result.m02 = 2.0f * (xz + yw);
            result.m12 = 2.0f * (yz - xw);
            result.m22 = 1.0f - 2.0f * (xx + yy);
        }

        /// <summary>
        /// 构造一个x轴旋转矩阵
        /// </summary>
        /// <param name="degree">旋转度数</param>
        /// <returns>旋转矩阵</returns>
        //public static Matrix4x4 MakeRotationX(float degree)
        //{
        //    Matrix4x4 result = Matrix4x4.identity;
        //    MakeRotationX(degree, ref result);
        //    return result;
        //}

        /// <summary>
        /// 构造一个x轴旋转矩阵
        /// </summary>
        /// <param name="degree">旋转度数</param>
        /// <param name="result">用于返回结果</param>
        //public static void MakeRotationX(float degree, ref Matrix4x4 result)
        //{
        //    float r = MathUtils.DegToRad(degree);
        //    float s = Mathf.Sin(r);
        //    float c = Mathf.Cos(r);
        //    result = Matrix4x4.identity;
        //    result.m11 =  c;
        //    result.m21 =  s;
        //    result.m12 = -s;
        //    result.m22 =  c;
        //}

        /// <summary>
        /// 构造一个y轴旋转矩阵
        /// </summary>
        /// <param name="degree">旋转度数</param>
        /// <returns>旋转矩阵</returns>
        //public static Matrix4x4 MakeRotationY(float degree)
        //{
        //    Matrix4x4 result = Matrix4x4.identity;
        //    MakeRotationY(degree, ref result);
        //    return result;
        //}

        /// <summary>
        /// 构造一个y轴旋转矩阵
        /// </summary>
        /// <param name="degree">旋转度数</param>
        /// <param name="result">用于返回结果</param>
        //public static void MakeRotationY(float degree, ref Matrix4x4 result)
        //{
        //    float r = MathUtils.DegToRad(degree);
        //    float s = Mathf.Sin(r);
        //    float c = Mathf.Cos(r);
        //    result = Matrix4x4.identity;
        //    result.m00 =  c;
        //    result.m20 = -s;
        //    result.m02 =  s;
        //    result.m22 =  c;
        //}

        /// <summary>
        /// 构造一个z轴旋转矩阵
        /// </summary>
        /// <param name="degree">旋转度数</param>
        /// <returns>旋转矩阵</returns>
        //public static Matrix4x4 MakeRotationZ(float degree)
        //{
        //    Matrix4x4 result = Matrix4x4.identity;
        //    MakeRotationZ(degree, ref result);
        //    return result;
        //}

        /// <summary>
        /// 构造一个z轴旋转矩阵
        /// </summary>
        /// <param name="degree">旋转度数</param>
        /// <param name="result">用于返回结果</param>
        //public static void MakeRotationZ(float degree, ref Matrix4x4 result)
        //{
        //    float r = MathUtils.DegToRad(degree);
        //    float s = Mathf.Sin(r);
        //    float c = Mathf.Cos(r);
        //    result = Matrix4x4.identity;
        //    result.m00 =  c;
        //    result.m10 =  s;
        //    result.m01 = -s;
        //    result.m11 =  c;
        //}

        /// <summary>
        /// 根据三个互相垂直的坐标轴建立一个旋转矩阵
        /// </summary>
        /// <param name="dir">Z轴</param>
        /// <param name="up">Y轴</param>
        /// <param name="right">X轴</param>
        /// <returns></returns>
        public static Matrix4x4 MakeRotation(Vector3 dir, Vector3 up, Vector3 right)
        {
            Matrix4x4 result = Matrix4x4.identity;
            result.SetColumn(0, right);
            result.SetColumn(1, up);
            result.SetColumn(2, dir);
            return result;
        }

        /// <summary>
        /// 将一个旋转矩阵转换为四元数
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Quaternion RotationMatrixToQuaternion(Matrix4x4 m)
        {
            Quaternion result = Quaternion.identity;
            float trace = m[0] + m[5] + m[10];
            float s;
            float invS;
            if (trace > 0.0f)
            {
                s = Mathf.Sqrt(trace + 1.0f) * 2.0f;
                invS = 1.0f / s;
                result.w = 0.25f * s;
                result.x = (m[6] - m[9]) * invS;
                result.y = (m[8] - m[2]) * invS;
                result.z = (m[1] - m[4]) * invS;
            }
            else if ((m[0] > m[5]) && (m[0] > m[10]))
            {
                s = Mathf.Sqrt(1.0f + m[0] - m[5] - m[10]) * 2.0f;
                invS = 1.0f / s;
                result.w = (m[6] - m[9]) * invS;
                result.x = 0.25f * s;
                result.y = (m[4] + m[1]) * invS;
                result.y = (m[8] + m[2]) * invS;
            }
            else if (m[5] > m[8])
            {
                s = Mathf.Sqrt(1.0f + m[5] - m[0] - m[8]) * 2.0f;
                invS = 1.0f / s;
                result.w = (m[8] - m[2]) * invS;
                result.x = (m[4] + m[1]) * invS;
                result.y = 0.25f * s;
                result.z = (m[9] + m[6]) * invS;
            }
            else
            {
                s = Mathf.Sqrt(1.0f + m[10] - m[0] - m[5]) * 2.0f;
                invS = 1.0f / s;
                result.w = (m[1] - m[4]) * invS;
                result.x = (m[8] + m[2]) * invS;
                result.y = (m[9] + m[6]) * invS;
                result.z = 0.25f * s;
            }

            return result;
        }

        /// <summary>
        /// 建立一个右手照相机矩阵
        /// </summary>
        /// <returns></returns>
        public static Matrix4x4 MakeCameraLookAtRH(Vector3 eye, Vector3 lookAt, Vector3 worldUp)
        {
            Vector3 dir = lookAt - eye;
            dir.Normalize();

            float dot = Vector3.Dot(worldUp, dir);
            Vector3 up = worldUp - dir * dot;
            up.Normalize();

            Vector3 right = Vector3.Cross(dir, up).normalized;

            Matrix4x4 result = Matrix4x4.identity;
            result[0]  = right.x;
            result[1]  = up.x;
            result[2]  = -dir.x;
            result[3]  = 0.0f;

            result[4]  = right.y;
            result[5]  = up.y;
            result[6]  = -dir.y;
            result[7]  = 0.0f;

            result[8]  = right.z;
            result[9]  = up.z;
            result[10] = -dir.z;
            result[11] = 0.0f;

            result[12] = -Vector3.Dot(right, eye);
            result[13] = -Vector3.Dot(up, eye);
            result[14] =  Vector3.Dot(dir, eye);
            result[15] = 1.0f;

            return result;
        }

        /// <summary>
        /// 构造一个OpenGL风格的正交投影矩阵
        /// </summary>
        /// <returns></returns>
        public static Matrix4x4 MakeOrthoOpenGL(float left, float right, float top, float bottom, float zNear, float zFar)
        {
            Matrix4x4 result = Matrix4x4.identity;
            result[0]  = 2.0f / (right - left);
            result[5]  = 2.0f / (top - bottom);
            result[10] = -2.0f / (zFar - zNear);
            result[12] = -(right + left) / (right - left);
            result[13] = -(top + bottom) / (top - bottom);
            result[14] = -(zFar + zNear) / (zFar - zNear);
            return result;
        }

        /// <summary>
        /// 构造一个OpenGL风格的透视投影矩阵
        /// </summary>
        /// <param name="fovY"></param>
        /// <param name="aspect"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        /// <returns></returns>
        //public static Matrix4x4 MakePerspectiveOpenGL(float fovY, float aspect, float zNear, float zFar)
        //{
        //    float d = MathUtils.DegToRad(fovY * 0.5f);
        //    float top = zNear * Mathf.Tan(d);
        //    float bottom = -top;
        //    float left= -(top - bottom) * aspect * 0.5f;
        //    float right = -left;

        //    Matrix4x4 result = Matrix4x4.identity;
        //    result[0]  = 2.0f * zNear / (right - left);
        //    result[5]  = 2.0f * zNear / (top - bottom);
        //    result[8]  = (right + left) / (right - left);
        //    result[9]  = (top + bottom) / (top - bottom);
        //    result[10] = -(zFar + zNear) / (zFar - zNear);
        //    result[11] = -1.0f;
        //    result[14] = -2.0f * zFar * zNear / (zFar - zNear);
        //    result[15] = 0.0f;

        //    return result;
        //}

        /// <summary>
        /// 构造一个Direct3D风格的正交投影矩阵，使用右手坐标系
        /// </summary>
        /// <returns></returns>
        public static Matrix4x4 MakeOrthoD3DRH(float left, float right, float top, float bottom, float zNear, float zFar)
        {
            Matrix4x4 result = Matrix4x4.identity;
            result[0]  =  2.0f / (right - left);
            result[5]  =  2.0f / (top - bottom);
            result[10] = -1.0f / (zFar - zNear);
            result[12] = -(right + left) / (right - left);
            result[13] = -(top + bottom) / (top - bottom);
            result[14] = -zNear / (zFar - zNear);
            return result;
        }
    }
}
