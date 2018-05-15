//#define OOTII_PROFILE

using System;
using UnityEngine;

namespace com.ootii.Actors
{
    /// <summary>
    /// Mount Points provide a way of attaching one object to another
    /// in a pre-defined spot. While we can use transforms to connect objects
    /// together, we want a systematic way for determing where these
    /// connection points are.
    /// 
    /// This monobehaviour represents a single (simple) mount point.
    /// </summary>
    [Serializable]
    [AddComponentMenu("ootii/Mount Points/Mount")]
    public class Mount : MonoBehaviour
    {
        /// <summary>
        /// Holds the mount point that we'll use to attract and mount
        /// </summary>
        public MountPoint Point = null;

        /// <summary>
        /// Called when the object is instanciated, but before Update()
        /// is called for the first time
        /// </summary>
        void Start()
        {
            // Ensure the mount points are initialized
            if (Point != null)
            {
                Point.OriginalScale = gameObject.transform.lossyScale;
            }
        }

        /// <summary>
        /// Creates a mount point at run-time. This won't persist unless you
        /// serialize the data yourself during run-time.
        /// </summary>
        /// <param name="rName">Name of the mount point</param>
        /// <param name="rBoneName">Bone name to attach it to or empty string to attach to root</param>
        /// <param name="rIgnoreParentScale">Determines if we should ignore the parent object's scale value</param>
        /// <returns></returns>
        public MountPoint CreateMountPoint(string rName, string rBoneName, bool rIgnoreParentScale)
        {
            // Create the mount point
            Point = new MountPoint();
            Point.Owner = gameObject;
            Point.Name = rName;
            Point.BoneName = rBoneName;
            Point.AllowChildren = false;
            Point.ForceChildOrientation = false;
            Point.IgnoreParentScale = false;

            // Attach it to the right bone
            Transform lParentTransform = gameObject.transform;

            Point.Anchor = new GameObject();
            Point.Anchor.name = "MP_" + Point.Anchor.GetInstanceID();

            if (Point.BoneName.Length > 0)
            {
                Animator lAnimator = gameObject.GetComponent<Animator>();
                if (lAnimator != null)
                {
                    int lBoneIndex = MountPoints.GetHumanBodyBoneID(Point.BoneName);
                    if (lBoneIndex >= 0 && lBoneIndex <= (int)HumanBodyBones.LastBone)
                    {
                        lParentTransform = lAnimator.GetBoneTransform((HumanBodyBones)lBoneIndex);
                    }
                    else
                    {
                        Transform lBoneTransform = MountPoints.FindBone(transform, Point.BoneName);
                        if (lBoneTransform != null) { lParentTransform = lBoneTransform; }
                    }
                }
            }

            Point.Anchor.transform.position = lParentTransform.position;
            Point.Anchor.transform.rotation = lParentTransform.rotation;
            Point.Anchor.transform.parent = lParentTransform;

            // Initialize by ignoring the scale
            Point.IgnoreParentScale = rIgnoreParentScale;

            // Return the point
            return Point;
        }

        /// <summary>
        /// Connects the child mount point to the parent
        /// </summary>
        /// <param name="rParent">Parent mount point we are connecting to</param>
        /// <param name="rChild">Child mount point being connected</param>
        /// <returns>Boolean used to determine if the connection was made</returns>
        public bool ConnectMountPoints(MountPoint rParentPoint)
        {
            if (rParentPoint == null) { return false; }
            if (!rParentPoint.AllowChildren) { return false; }

            Point.ChildTo(rParentPoint);

            return true;
        }

        /// <summary>
        /// Conntects the child mount point to the parent
        /// </summary>
        /// <param name="rParent">String representing the parents mount point name</param>
        /// <param name="rChildItemPath">Resource path to the object we'll instanciate</param>
        /// <param name="rChildPointName">String representing the child's mount point name</param>
        /// <returns>GameObject that is the child that is instanciated</returns>
        public bool ConnectMountPoints(GameObject rParent, string rParentPoint)
        {
            if (rParent == null) { return false; }

            MountPoints lParentMPs = rParent.GetComponent<MountPoints>();
            if (lParentMPs == null) { return false; }

            return lParentMPs.ConnectMountPoints(rParentPoint, Point);
        }

        /// <summary>
        /// Conntects the child mount point to the parent
        /// </summary>
        /// <param name="rParent">String representing the parents mount point name</param>
        /// <param name="rChildItemPath">Resource path to the object we'll instanciate</param>
        /// <param name="rChildPointName">String representing the child's mount point name</param>
        /// <returns>GameObject that is the child that is instanciated</returns>
        public bool ConnectMountPoints(string rParentName, string rParentPoint)
        {
            GameObject lParent = GameObject.Find(rParentName);
            if (lParent == null) { return false; }

            MountPoints lParentMPs = lParent.GetComponent<MountPoints>();
            if (lParentMPs == null) { return false; }

            return lParentMPs.ConnectMountPoints(rParentPoint, Point);
        }

        /// <summary>
        /// Disconnects the child mount point from the parent
        /// </summary>
        /// <param name="rParent">Parent mount point who owns the child</param>
        /// <param name="rChild">Child mount point to disconnect</param>
        public void DisconnectMountPoints()
        {
            Point.ChildTo(null);
        }
    }
}
