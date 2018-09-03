#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - AssetBundle framework for Unity3D
// ===================================
// 
// Filename: AppVersion.cs
// Date:     2015/12/03
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

using System;
using System.Text;

namespace KEngine
{
    /// <summary>
    /// For App Version, version string can with description
    /// 1.2.3.123.release.mi
    /// MAJOR.MINOR.PATCH.BUILD.DESC
    /// </summary>
    public class AppVersion : IComparable, ICloneable
    {
        public uint Major;
        public uint Minor;
        public uint Patch;
        public uint Build;

        public string VersionType { get; set; }
        public string VersionDesc { get; set; }

        public AppVersion(string versionStr)
        {
            var versionArr = versionStr.Split('.');

            if (versionArr.Length >= 1)
                uint.TryParse(versionArr[0], out Major);
            if (versionArr.Length >= 2)
                uint.TryParse(versionArr[1], out Minor);
            if (versionArr.Length >= 3)
                uint.TryParse(versionArr[2], out Patch);
            if (versionArr.Length >= 4)
                uint.TryParse(versionArr[3], out Build);

            if (versionArr.Length >= 5)
            {
                var strVerType = versionArr[5];
                VersionType = strVerType;
            }

            if (versionArr.Length >= 7)
            {
                VersionDesc = string.Join(".", versionArr, 6, versionArr.Length - 6); // 剩余的串起来
            }
        }

        /// <summary>
        /// To Version String
        /// eg. 1.2.1.0.alpha.xxx
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}.{1}.{2}.{3}.{4}", Major, Minor, Patch, Build, VersionType.ToString().ToLower());
            if (!string.IsNullOrEmpty(VersionDesc))
            {
                sb.AppendFormat(".{0}", VersionDesc);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Clone new one
        /// <para>using ToString() to create new</para>
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// eg.  1.2
        /// </summary>
        /// <returns></returns>
        public string ToVersion2()
        {
            return string.Format("{0}.{1}", Major, Minor);
        }

        /// <summary>
        /// eg. 1.2.1
        /// </summary>
        /// <returns></returns>
        public string ToVersion3()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }

        /// <summary>
        /// eg. 1.2.1.0
        /// </summary>
        /// <returns></returns>
        public string ToVersion4()
        {
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Patch, Build);
        }

        /// <summary>
        /// 所有版本数字组成的数组
        /// </summary>
        private uint[] GetVersionNumbers(int limit = 4)
        {
            if (limit == 4)
                return new[] { Major, Minor, Patch, Build};
            if (limit == 3)
                return new[] { Major, Minor, Patch };
            if (limit == 2)
                return new[] { Major, Minor };
            if (limit <= 1)
                return new[] { Major };

            return new[] { Major, Minor, Patch, Build };
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj, 100);
        }

        /// <summary>
        /// 逐字比较，遇不同，数字比较
        /// </summary>
        /// <param name="v2o"></param>
        /// <param name="limitNumber">限制几位数字进行比较？</param>
        /// <returns></returns>
        public int CompareTo(Object v2o, int limitNumber)
        {
            AppVersion v2 = v2o as AppVersion;
            if (v2 == null)
                throw new ArgumentException("Arg_MustBeVersion");

            var nums1 = GetVersionNumbers(limitNumber);
            var nums2 = v2.GetVersionNumbers(limitNumber);
            for (var i = 0; i < nums1.Length; i++)
            {
                var n1 = nums1[i];
                var n2 = nums2[i];
                if (n1 != n2)
                {
                    return n1.CompareTo(n2);
                }
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AppVersion);
        }

        protected bool Equals(AppVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Major;
                hashCode = (hashCode * 397) ^ (int)Minor;
                hashCode = (hashCode * 397) ^ (int)Patch;
                hashCode = (hashCode * 397) ^ (int)Build;
                hashCode = (hashCode * 397) ^ (VersionType != null ? VersionType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VersionDesc != null ? VersionDesc.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(AppVersion v1, AppVersion v2)
        {
            if (Object.ReferenceEquals(v1, null))
            {
                return Object.ReferenceEquals(v2, null);
            }

            return v1.Equals(v2);
        }

        public static bool operator !=(AppVersion v1, AppVersion v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(AppVersion v1, AppVersion v2)
        {
            if ((Object)v1 == null)
                throw new ArgumentNullException("v1");
            return (v1.CompareTo(v2) < 0);
        }

        public static bool operator <=(AppVersion v1, AppVersion v2)
        {
            if ((Object)v1 == null)
                throw new ArgumentNullException("v1");
            return (v1.CompareTo(v2) <= 0);
        }

        public static bool operator >(AppVersion v1, AppVersion v2)
        {
            return (v2 < v1);
        }

        public static bool operator >=(AppVersion v1, AppVersion v2)
        {
            return (v2 <= v1);
        }

    }
}