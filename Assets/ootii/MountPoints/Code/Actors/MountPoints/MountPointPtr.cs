using System;
using UnityEngine;

namespace com.ootii.Actors
{
    /// <summary>
    /// Unity doesn't seem to serialize class references. So when
    /// deserializing, we get two new instances when really we should
    /// have one instance and a reference. We'll use this class to
    /// allow us to use a reference to the mount points in the
    /// MountPoints.
    /// </summary>
    [Serializable]
    public class MountPointPtr
    {
        /// <summary>
        /// GameObject that owns the MountPoints that owns the MountPoint.
        /// Note that GameObject references are kept.
        /// </summary>
        [SerializeField]
        public GameObject Owner = null;

        /// <summary>
        /// GUID representing the unique MountPoint
        /// </summary>
        [SerializeField]
        public string GUID = "";

        /// <summary>
        /// Cache and accessor to grabbing the actual mount point
        /// </summary>
        [NonSerialized]
        private MountPoint mMountPoint = null;
        public MountPoint MountPoint
        {
            set
            {
                mMountPoint = value;

                // Store the data that gets us to the real value
                if (mMountPoint != null)
                {
                    Owner = mMountPoint.Owner;
                    GUID = mMountPoint._GUID;
                }
                // Clear out the data if needed
                else
                {
                    Owner = null;
                    GUID = "";
                }
            }

            get
            {
                // If the cache is null, try to grab the real value
                if (mMountPoint == null)
                {
                    if (Owner != null)
                    {
                        MountPoints lParentMountPointList = Owner.GetComponent<MountPoints>();
                        if (lParentMountPointList != null)
                        {
                            mMountPoint = lParentMountPointList.GetMountPointFromGUID(GUID);
                        }
                        else
                        {
                            Mount lParentMount = Owner.GetComponent<Mount>();
                            if (lParentMount != null) { mMountPoint = lParentMount.Point; }
                        }
                    }
                }

                // Return the mount point
                return mMountPoint;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MountPointPtr()
        {
        }

        /// <summary>
        /// Mount Point constructor
        /// </summary>
        /// <param name="rSource">Mount Point to fill the reference with</param>
        public MountPointPtr(MountPoint rSource)
        {
            MountPoint = rSource;
        }

        /// <summary>
        /// We have to override or we get a warning
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Override equals to use our equality tests
        /// </summary>
        /// <param name="rObject"></param>
        /// <returns></returns>
        public override bool Equals(object rObject)
        {
            if (rObject is MountPointPtr)
            {
                return (this == (MountPointPtr)rObject);
            }
            else if (rObject is MountPoint)
            {
                return (this == (MountPoint)rObject);
            }
            else
            {
                return base.Equals(rObject);
            }
        }

        /// <summary>
        /// Hijack the equality operator for comparing pointer values
        /// </summary>
        /// <returns>True if the values are considered equal, false if not</returns>
        public static bool operator ==(MountPointPtr rLeft, MountPointPtr rRight)
        {
            if (ReferenceEquals(rLeft, rRight)) { return true; }
            if (ReferenceEquals(rLeft, null) && ReferenceEquals(rRight, null)) { return true; }
            if (!ReferenceEquals(rLeft, null) && ReferenceEquals(rRight, null)) { return false; }
            if (ReferenceEquals(rLeft, null) && !ReferenceEquals(rRight, null)) { return false; }
            
            if (rLeft.Owner == null || rRight.Owner == null) { return false; }
            if (rLeft.GUID.Length == 0 || rRight.GUID.Length == 0) { return false; }

            return (rLeft.Owner == rRight.Owner && rLeft.GUID == rRight.GUID);
        }

        /// <summary>
        /// Hijack the inequality operator for comparing pointer values
        /// </summary>
        /// <returns>True if the values are considered equal, false if not</returns>
        public static bool operator !=(MountPointPtr rLeft, MountPointPtr rRight)
        {
            return !(rLeft == rRight);
        }

        /// <summary>
        /// Hijack the equality operator for comparing values
        /// </summary>
        /// <returns>True if the values are considered equal, false if not</returns>
        public static bool operator ==(MountPointPtr rLeft, MountPoint rRight)
        {
            if (ReferenceEquals(rLeft, null) && ReferenceEquals(rRight, null)) { return true; }
            if (!ReferenceEquals(rLeft, null) && ReferenceEquals(rRight, null)) { return false; }
            if (ReferenceEquals(rLeft, null) && !ReferenceEquals(rRight, null)) { return false; }
            
            if (rLeft.Owner == null || rRight.Owner == null) { return false; }
            if (rLeft.GUID.Length == 0 || rRight._GUID.Length == 0) { return false; }

            return (rLeft.Owner == rRight.Owner && rLeft.GUID == rRight._GUID);
        }

        /// <summary>
        /// Hijack the equality operator for comparing values
        /// </summary>
        /// <returns>True if the values are considered equal, false if not</returns>
        public static bool operator !=(MountPointPtr rLeft, MountPoint rRight)
        {
            return !(rLeft == rRight);
        }
    }
}
