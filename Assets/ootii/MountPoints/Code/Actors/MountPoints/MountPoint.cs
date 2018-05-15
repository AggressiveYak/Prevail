using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Base;

namespace com.ootii.Actors
{
    /// <summary>
    /// The Mount Point acts as both a parent and a child
    /// for other mount points. They are attached to objects and
    /// 'snap' together to connect the objects.
    /// </summary>
    [Serializable]
    public class MountPoint : BaseObject
    {
        /// <summary>
        /// Friendly name of the mount point
        /// </summary>
        /// <summary>
        /// Friendly name of the mount point
        /// </summary>
        public override string Name
        {
            get { return _Name; }

            set
            {
                _Name = value;
                if (_Anchor != null) { _Anchor.name = "MP_" + _Name; }
            }
        }
        
        /// <summary>
        /// Bone we want to attach the link to
        /// </summary>
        public string _BoneName = "";
        public string BoneName
        {
            get { return _BoneName; }
            set { _BoneName = value; }
        }

        /// <summary>
        /// High level game object that owns the mount point. This 
        /// is the game object that owns the mount point list and is
        /// typically the avatar, a weapon, etc
        /// </summary>
        public GameObject _Owner = null;
        public GameObject Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }

        /// <summary>
        /// Resource path to the prefab for the item
        /// </summary>
        public string _OwnerResourcePath = "";
        public string OwnerResourcePath
        {
            get { return _OwnerResourcePath; }
            set { _OwnerResourcePath = value; }
        }

        /// <summary>
        /// Game object that represents the mount point
        /// </summary>
        public GameObject _Anchor = null;
        public GameObject Anchor
        {
            get { return _Anchor; }
            set { _Anchor = value; }
        }

        /// <summary>
        /// Short cut for grabbing the parent mount point. Since
        /// Unity can't serialize a reference to a class and share it across
        /// objects, we need to create this work around
        /// </summary>
        public MountPointPtr _ParentMountPoint = null;
        public MountPoint ParentMountPoint
        {
            set { _ParentMountPoint.MountPoint = value; }
            get { return _ParentMountPoint.MountPoint; }
        }

        /// <summary>
        /// Child mount points that are attached to this mount point
        /// </summary>
        public List<MountPointPtr> ChildMountPoints = new List<MountPointPtr>();

        /// <summary>
        /// Determines if the mount point is locked into the
        /// position on the model. If so, then moving the mount
        /// point actually moves the model
        /// </summary>
        [SerializeField]
        protected bool mIsLocked = false;
        public bool IsLocked
        {
            get { return mIsLocked; }
            set { mIsLocked = value; }
        }

        /// <summary>
        /// Determines if this mount point is able to accept
        /// children being dropped on it.
        /// </summary>
        public bool _AllowChildren = true;
        public bool AllowChildren
        {
            get { return _AllowChildren; }
            set { _AllowChildren = value; }
        }

        /// <summary>
        /// Determines if the child object will rotate to
        /// match this mount points orientation when connected.
        /// </summary>
        public bool _ForceChildOrientation = true;
        public bool ForceChildOrientation
        {
            get { return _ForceChildOrientation; }
            set { _ForceChildOrientation = value; }
        }

        /// <summary>
        /// If we rotate the child, invert the rotation so z-axis is not aligned, but opposite
        /// </summary>
        public bool _InvertOrientation = false;
        public bool InvertOrientation
        {
            get { return _InvertOrientation; }
            set { _InvertOrientation = value; }
        }

        /// <summary>
        /// Allows us to ignore the owner scale when attaching
        /// sub objects.
        /// </summary>
        public bool _IgnoreParentScale = true;
        public bool IgnoreParentScale
        {
            get { return _IgnoreParentScale; }

            set
            {
                _IgnoreParentScale = value;

                if (_Owner != null && _Anchor != null)
                {
                    if (Mathf.Abs(_Owner.transform.lossyScale.x) > Mathf.Epsilon &&
                        Mathf.Abs(_Owner.transform.lossyScale.y) > Mathf.Epsilon && 
                        Mathf.Abs(_Owner.transform.lossyScale.z) > Mathf.Epsilon)
                    {
                        Vector3 lMPScale = Vector3.one;
                        if (_IgnoreParentScale && _Owner.transform.lossyScale != Vector3.one)
                        {
                            lMPScale = _Owner.transform.lossyScale;
                            lMPScale.x = 1f / Mathf.Max(lMPScale.x, 0.0001f);
                            lMPScale.y = 1f / Mathf.Max(lMPScale.y, 0.0001f);
                            lMPScale.z = 1f / Mathf.Max(lMPScale.z, 0.0001f);
                        }

                        if (_Anchor.transform.localScale != lMPScale)
                        {
                            _Anchor.transform.localScale = lMPScale;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Store the scale before we change it
        /// </summary>
        public Vector3 _OriginalScale = Vector3.one;
        public Vector3 OriginalScale
        {
            get { return _OriginalScale; }
            set { _OriginalScale = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MountPoint()
            : base()
        {
            // Since Unity won't serialize the same class across objects,
            // we'll use the GUID to help us track unique mount points
            GenerateGUID();

            // Create the parent mount point pointer
            _ParentMountPoint = new MountPointPtr();
        }

        /// <summary>
        /// Anchors the mount point to the specified bone, essencially making
        /// this mount point a child of the bone.
        /// </summary>
        /// <param name="rBoneName">Bone name to anchor to or empty string to anchor to the root</param>
        public void AnchorTo(string rBoneName)
        {
            Transform lParentTransform = _Owner.transform;

            if (_Anchor == null) 
            { 
                _Anchor = new GameObject();
                _Anchor.name = "MP_" + _Anchor.GetInstanceID();
            }

            _BoneName = rBoneName;
            if (_BoneName.Length > 0)
            {
                Animator lAnimator = _Owner.GetComponent<Animator>();
                if (lAnimator != null)
                {
                    int lBoneIndex = MountPoints.GetHumanBodyBoneID(_BoneName);
                    if (lBoneIndex >= 0 && lBoneIndex < (int)HumanBodyBones.LastBone)
                    {
                        lParentTransform = lAnimator.GetBoneTransform((HumanBodyBones)lBoneIndex);
                    }
                    else
                    {
                        Transform lBoneTransform = MountPoints.FindBone(_Owner.transform, _BoneName);
                        if (lBoneTransform != null) { lParentTransform = lBoneTransform; }
                    }
                }
            }

            // Parent the mount point to this new transform. We don't
            // change it's position or rotation since we may need an offset.
            _Anchor.transform.parent = lParentTransform;
        }

        /// <summary>
        /// Anchors the mount point to the specified bone, essencially making
        /// this mount point a child of the bone.
        /// </summary>
        /// <param name="rBoneName">Bone name to anchor to or empty string to anchor to the root</param>
        public void AnchorTo(HumanBodyBones rHumanBodyBoneID)
        {
            if (rHumanBodyBoneID < 0) { return; }
            if (rHumanBodyBoneID > HumanBodyBones.LastBone) { return; }

            _BoneName = "";
            Transform lParentTransform = _Owner.transform;

            if (_Anchor == null) { _Anchor = new GameObject(); }

            Animator lAnimator = _Owner.GetComponent<Animator>();
            if (lAnimator != null)
            {
                lParentTransform = lAnimator.GetBoneTransform(rHumanBodyBoneID);
                if (lParentTransform != null)
                {
                    _BoneName = MountPoints.GetHumanBodyBoneName(rHumanBodyBoneID);
                }
            }            

            // Parent the mount point to this new transform. We don't
            // change it's position or rotation since we may need an offset.
            _Anchor.transform.parent = lParentTransform;
        }

        /// <summary>
        /// Attaches this mount point's owner to the specified
        /// parent. Clears the attachment if the parent is null
        /// </summary>
        /// <param name="rParent"></param>
        public void ChildTo(MountPoint rParent)
        {
            // If we have an invalid owner, then the owner was
            // probably deleted and we need to disconnect this object
            if (_Owner == null) 
            {
                if (ParentMountPoint != null) { ParentMountPoint.RemoveChild(this); }
                return; 
            }

            // Ensure we're not trying to parent to a child
            for (int i = 0; i < ChildMountPoints.Count; i++)
            {
                if (ChildMountPoints[i].MountPoint == rParent) { return; }
            }

            // Clear out the parent
            if (rParent == null)
            {
                // Disconnect the mount points
                if (ParentMountPoint != null) { ParentMountPoint.RemoveChild(this); }

                // Disconnect the actual parenting
                _Owner.transform.parent = null;
                _Owner.transform.localScale = _OriginalScale;
            }
            // Assign the parent
            else if (rParent._Anchor != null)
            {
                // Ensure the our orientation is correct
                if (rParent.ForceChildOrientation)
                {
                    _Owner.transform.rotation = Quaternion.identity;
                    _Owner.transform.rotation = OrientTo(_Anchor.transform.rotation, rParent._Anchor.transform.rotation, rParent.InvertOrientation);
                }

                // Since we changed the orientation of the parent, we
                // may need to reposition it to the mount point
                Vector3 lDelta = rParent._Anchor.transform.position - _Anchor.transform.position;
                _Owner.transform.position += lDelta;

                // Store our current scale so we can go back to it if needed
                if (_Owner.transform.parent == null)
                {
                    _OriginalScale = _Owner.transform.lossyScale;
                }

                // Parent and adjust the scale as needed
                _Owner.transform.parent = rParent._Anchor.transform;

                // Record this mount point is attached to the parent
                if (ParentMountPoint != rParent)
                {
                    rParent.AddChild(this); 
                }
            }
        }

        /// <summary>
        /// Adds a child mount point to the list
        /// </summary>
        /// <param name="rChild"></param>
        public void AddChild(MountPoint rChild)
        {
            if (rChild == null) { return; }
            if (!_AllowChildren) { return; }

            // If the child already has a parent, we need to clear it and reset
            if (rChild.ParentMountPoint != null) { rChild.ParentMountPoint.RemoveChild(rChild); }
            rChild.ParentMountPoint = this;

            // First, check if it already exists. We don't want to add it twice
            for (int i = 0; i < ChildMountPoints.Count; i++)
            {
                MountPointPtr lChild = ChildMountPoints[i];
                if (lChild.Owner == rChild._Owner && lChild.GUID == rChild.GUID)
                {
                    return;
                }
            }

            // If we got here, we need to add the child
            MountPointPtr lReference = new MountPointPtr(rChild);
            ChildMountPoints.Add(lReference);
        }

        /// <summary>
        /// Removes a child mount point from the list
        /// </summary>
        /// <param name="rChild"></param>
        public void RemoveChild(MountPoint rChild)
        {
            if (rChild == null) { return; }

            // Remove the child's parent
            if (rChild.ParentMountPoint == this) { rChild.ParentMountPoint = null; }

            // Remove the child from the list
            for (int i = ChildMountPoints.Count - 1; i >= 0; i--)
            {
                MountPointPtr lChild = ChildMountPoints[i];
                if (lChild.Owner == rChild._Owner && lChild.GUID == rChild.GUID)
                {
                    // Just in case we find it more than once, clear out the parent again
                    MountPoint lChildMountPoint = lChild.MountPoint;
                    if (lChildMountPoint.ParentMountPoint == this) 
                    { 
                        lChildMountPoint.ParentMountPoint = null; 
                    }

                    // Remove it from the list
                    ChildMountPoints.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Determines if the resource is already instanciated and mounted
        /// </summary>
        /// <returns></returns>
        public bool IsChild(string rResourcePath)
        {
            for (int i = 0; i < ChildMountPoints.Count; i++)
            {
                MountPointPtr lChild = ChildMountPoints[i];
                if (lChild.MountPoint != null && lChild.MountPoint.Owner != null)
                {
                    // This is the fastest approach to determninng if the resource is the same
                    if (lChild.MountPoint.OwnerResourcePath.Length > 0)
                    {
                        if (lChild.MountPoint.OwnerResourcePath == rResourcePath)
                        {
                            return true;
                        }
                    }
                    // This is a slow approach, but we cache the results
                    else if (rResourcePath.Length > 0)
                    {
                        GameObject lObject = Resources.Load(rResourcePath) as GameObject;
                        if (lObject != null)
                        {
                            MeshFilter lMesh = lChild.MountPoint.Owner.GetComponent<MeshFilter>();
                            if (lMesh == null) { lMesh = lChild.MountPoint.Owner.GetComponentInChildren<MeshFilter>(); }
                            if (lMesh == null) { continue; }

                            MeshFilter lObjectMesh = lObject.GetComponent<MeshFilter>();
                            if (lObjectMesh == null) { lObjectMesh = lObject.GetComponentInChildren<MeshFilter>(); }
                            if (lObjectMesh == null) { continue; }

                            if (lMesh.sharedMesh == lObjectMesh.sharedMesh)
                            {
                                Renderer lRenderer = lChild.MountPoint.Owner.GetComponent<Renderer>();
                                if (lRenderer == null) { lRenderer = lChild.MountPoint.Owner.GetComponentInChildren<Renderer>(); }
                                if (lRenderer == null) { continue; }

                                Renderer lObjectRenderer = lObject.GetComponent<Renderer>();
                                if (lObjectRenderer == null) { lObjectRenderer = lObject.GetComponentInChildren<Renderer>(); }
                                if (lObjectRenderer == null) { continue; }

                                if (lRenderer.sharedMaterial == lObjectRenderer.sharedMaterial)
                                {
                                    lChild.MountPoint.OwnerResourcePath = rResourcePath;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a rotation that when set as the 'transform.rotation' will mimic that
        /// of the 'to'.
        /// </summary>
        /// <param name="rFrom">This object we're operating on</param>
        /// <param name="rTo">Quaternion to rotate towards</param>
        /// <returns>Resulting quaternion</returns>
        private Quaternion OrientTo(Quaternion rFrom, Quaternion rTo, bool rInvert)
        {
            Quaternion lInvFrom = Quaternion.Inverse(rFrom);
            Quaternion lResult = rTo * lInvFrom;

            if (rInvert)
            {
                lResult = lResult * Quaternion.AngleAxis(180f, Vector3.up);
            }

            return lResult;
        }
    }
}
