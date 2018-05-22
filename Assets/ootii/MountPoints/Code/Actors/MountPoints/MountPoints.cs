//#define OOTII_PROFILE

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using com.ootii.Helpers;

#if UNITY_EDITOR && OOTII_PROFILE
using System.Diagnostics;
#endif

namespace com.ootii.Actors
{
    /// <summary>
    /// Mount Points provide a way of attaching one object to another
    /// in a pre-defined spot. While we can use transforms to connect objects
    /// together, we want a systematic way for determing where these
    /// connection points are.
    /// 
    /// This list is used by objects to define the custom set of mount points
    /// available on the object.
    /// </summary>
    [Serializable]
    [AddComponentMenu("ootii/Mount Points/Mount List")]
    public class MountPoints : NetworkBehaviour
    {
        /// <summary>
        /// Determines if we use body masks to help hide body penetration
        /// </summary>
        public bool _UseBodyMasks = false;
        public bool UseBodyMasks
        {
            get { return _UseBodyMasks; }

            set
            {
                if (!Application.isPlaying)
                {
                    _UseBodyMasks = value;
                }
                else
                {
                    // If we're disabling body masks
                    if (_UseBodyMasks && !value)
                    {
                        ClearBodyMasks();
                        _UseBodyMasks = false;
                    }
                    // If we're enabling body masks
                    else if (!_UseBodyMasks && value)
                    {
                        _UseBodyMasks = true;

                        // Ensure our body mask is initialized
                        if (mBlitMaterial == null) { InitializeBodyMask(); }

                        // Create the masks as needed
                        for (int i = 0; i < SkinnedItems.Count; i++)
                        {
                            SkinnedItems[i].CreateMask();
                        }

                        // Apply them
                        ApplyBodyMasks();
                    }
                }
            }
        }

        /// <summary>
        /// Renderer that contains the material we'll be modifying
        /// </summary>
        public SkinnedMeshRenderer _Renderer = null;
        public SkinnedMeshRenderer Renderer
        {
            get { return _Renderer; }
            set { _Renderer = value; }
        }

        /// <summary>
        /// Index of the material to modify
        /// </summary>
        public int _MaterialIndex = 0;
        public int MaterialIndex
        {
            get { return _MaterialIndex; }
            set { _MaterialIndex = value; }
        }

        /// <summary>
        /// Holds the points that we'll use to attract and mount
        /// </summary>
        public List<MountPoint> Points = new List<MountPoint>();

        /// <summary>
        /// Holds items that were created as skinned mesh elements
        /// </summary>
        public List<SkinnedItem> SkinnedItems = new List<SkinnedItem>();

        /// <summary>
        /// Texture that holds the original (unmodified) body texture
        /// </summary>
        private Texture2D mBodyTexture = null;

        /// <summary>
        /// Render texture we'll put the body texture and masks into
        /// </summary>
        private RenderTexture mBodyRenderTexture = null;

        /// <summary>
        /// Shader we'll use to render with
        /// </summary>
        private Material mBlitMaterial = null;

        /// <summary>
        /// Format of our render texture
        /// </summary>
        private RenderTextureFormat mRenderTextureFormat = RenderTextureFormat.ARGB32;

        /// <summary>
        /// Color space of the render texture
        /// </summary>
        private RenderTextureReadWrite mRenderTextureColorSpace = RenderTextureReadWrite.Linear;

        /// <summary>
        /// Used by the editor to keep the point selected
        /// </summary>
        public int EditorPointIndex = -1;

        /// <summary>
        /// Used by the editor to keep the item selected
        /// </summary>
        public int EditorItemIndex = -1;

        /// <summary>
        /// Called when the object is instanciated, but before Update()
        /// is called for the first time
        /// </summary>
        void Start()
        {
            // Ensure the mount points are initialized
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i] != null)
                {
                    Points[i].OriginalScale = gameObject.transform.lossyScale;
                }
            }

            // Initialize mask textures if needed
            if (_UseBodyMasks) { InitializeBodyMask(); }

            // If we have skinned items that aren't initialized, do that now
            for (int i = 0; i < SkinnedItems.Count; i++)
            {
                SkinnedItem lItem = SkinnedItems[i];
                if (lItem._InstantiateOnStart || lItem.GameObject != null)
                {
                    if (lItem.ResourcePath.Length > 0 && lItem.GameObject == null)
                    {
                        lItem.CreateInstance(this);
                    }

                    if (_UseBodyMasks && lItem.MaskPath.Length > 0)
                    {
                        lItem.CreateMask();
                    }
                }
            }

            if (QualitySettings.activeColorSpace != ColorSpace.Linear)
            {
                mRenderTextureColorSpace = RenderTextureReadWrite.Default;
            }

            // Apply the body masks if needed
            ApplyBodyMasks();
        }

        /// <summary>
        /// Grab information from the current texture so we can re-use it
        /// </summary>
        public void InitializeBodyMask()
        {
            int lIndex = (_MaterialIndex >= 0 && _MaterialIndex < _Renderer.sharedMaterials.Length ? _MaterialIndex : 0);
            if (lIndex >= _Renderer.sharedMaterials.Length) { return; }

            mBlitMaterial = new Material(Shader.Find("Hidden/MountPointsBlit"));

            if (_Renderer == null) { _Renderer = gameObject.GetComponent<SkinnedMeshRenderer>(); }
            if (_Renderer == null) { _Renderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>(); }
            if (_Renderer != null)
            {
                mBodyTexture = null;

#if UNITY_STANDALONE || UNITY_EDITOR

                //ProceduralTexture lProceduralTexture = _Renderer.sharedMaterials[lIndex].mainTexture as ProceduralTexture;
                //if (lProceduralTexture != null)
                //{
                //    ProceduralMaterial lSubstance = _Renderer.sharedMaterials[lIndex] as ProceduralMaterial;
                //    if (lSubstance != null) { lSubstance.isReadable = true; }

                //    Color32[] lPixels = lProceduralTexture.GetPixels32(0, 0, lProceduralTexture.width, lProceduralTexture.height);
                //    if (lPixels != null && lPixels.Length == lProceduralTexture.width * lProceduralTexture.height)
                //    {
                //        mBodyTexture = new Texture2D(lProceduralTexture.width, lProceduralTexture.height, lProceduralTexture.format, false, false);
                //        mBodyTexture.SetPixels32(lPixels);
                //        mBodyTexture.Apply();
                //    }
                //}

#endif

                if (mBodyTexture == null)
                {
                    mBodyTexture = _Renderer.sharedMaterials[lIndex].mainTexture as Texture2D;
                }
                
                if (mBodyTexture != null)
                {
                    mBodyRenderTexture = new RenderTexture(mBodyTexture.width, mBodyTexture.height, 0, mRenderTextureFormat, mRenderTextureColorSpace);
                    mBodyRenderTexture.wrapMode = TextureWrapMode.Clamp;

                    mBlitMaterial.SetTexture("_MaskTex", null);
                    UnityEngine.Graphics.Blit(mBodyTexture, mBodyRenderTexture, mBlitMaterial, 0);

                    _Renderer.materials[lIndex].mainTexture = mBodyRenderTexture;
                }
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
            MountPoint lPoint = new MountPoint();
            lPoint.Owner = gameObject;
            lPoint.Name = rName;
            lPoint.BoneName = rBoneName;

            // Attach it to the right bone
            Transform lParentTransform = gameObject.transform;

            lPoint.Anchor = new GameObject();
            lPoint.Anchor.name = "MP_" + lPoint.Anchor.GetInstanceID();

            if (lPoint.BoneName.Length > 0)
            {
                Animator lAnimator = gameObject.GetComponent<Animator>();
                if (lAnimator != null)
                {
                    int lBoneIndex = MountPoints.GetHumanBodyBoneID(lPoint.BoneName);
                    if (lBoneIndex >= 0 && lBoneIndex <= (int)HumanBodyBones.LastBone)
                    {
                        lParentTransform = lAnimator.GetBoneTransform((HumanBodyBones)lBoneIndex);
                    }
                    else
                    {
                        Transform lBoneTransform = MountPoints.FindBone(transform, lPoint.BoneName);
                        if (lBoneTransform != null) { lParentTransform = lBoneTransform; }
                    }
                }
            }

            lPoint.Anchor.transform.position = lParentTransform.position;
            lPoint.Anchor.transform.rotation = lParentTransform.rotation;
            lPoint.Anchor.transform.parent = lParentTransform;

            // Initialize by ignoring the scale
            lPoint.IgnoreParentScale = rIgnoreParentScale;

            // Add to the list of mount points
            Points.Add(lPoint);

            // Return the point
            return lPoint;
        }

        /// <summary>
        /// Connects the child mount point to the parent
        /// </summary>
        /// <param name="rParent">Parent mount point we are connecting to</param>
        /// <param name="rChild">Child mount point being connected</param>
        /// <returns>Boolean used to determine if the connection was made</returns>
        public bool ConnectMountPoints(MountPoint rParentPoint, MountPoint rChildPoint)
        {
            if (rParentPoint == null) { return false; }
            if (!rParentPoint.AllowChildren) { return false; }
            if (rChildPoint == null) { return false; }

            rChildPoint.ChildTo(rParentPoint);

            return true;
        }

        /// <summary>
        /// Conntects the child mount point to the parent
        /// </summary>
        /// <param name="rParent">String representing the parents mount point name</param>
        /// <param name="rChild">Child mount point being connected</param>
        /// <returns>Boolean used to determine if the connection was made</returns>
        public bool ConnectMountPoints(string rParentPoint, MountPoint rChildPoint)
        {
            MountPoint lParentPoint = GetMountPoint(rParentPoint);
            if (lParentPoint == null) { return false; }

            // Find the matching parent and attempt a connect
            return ConnectMountPoints(lParentPoint, rChildPoint);
        }

        /// <summary>
        /// Conntects the child mount point to the parent
        /// </summary>
        /// <param name="rParentPoint">Parent mount point</param>
        /// <param name="rChild">GameObject representing the child</param>
        /// <param name="rChildPointName">String representing the child mount point name</param>
        /// <returns>Boolean used to determine if the connection was made</returns>
        public bool ConnectMountPoints(MountPoint rParentPoint, GameObject rChild, string rChildPointName)
        {
            if (rChild == null) { return false; }
            if (rParentPoint == null) { return false; }

            MountPoint lChildMP = null;
            MountPoints lChildMPList = rChild.GetComponent<MountPoints>();
            if (lChildMPList != null)
            {
                lChildMP = lChildMPList.GetMountPoint(rChildPointName);
            }
            else
            {
                Mount lParentMount = rChild.GetComponent<Mount>();
                if (lParentMount != null) { lChildMP = lParentMount.Point; }
            }

            // If there is no child, get out
            if (lChildMP == null)
            {
                return false;
            }

            // Finally, connect the objects
            return ConnectMountPoints(rParentPoint, lChildMP);
        }

        /// <summary>
        /// Conntects the child mount point to the parent
        /// </summary>
        /// <param name="rParentPoint">String representing the parents mount point name</param>
        /// <param name="rChild">GameObject representing the child</param>
        /// <param name="rChildPointName">String representing the child mount point name</param>
        /// <returns>Boolean used to determine if the connection was made</returns>
        public bool ConnectMountPoints(string rParentPoint, GameObject rChild, string rChildPointName)
        {
            if (rChild == null) { return false; }

            MountPoint lParentMP = GetMountPoint(rParentPoint);
            if (lParentMP == null) { return false; }

            MountPoint lChildMP = null;
            MountPoints lChildMPList = rChild.GetComponent<MountPoints>();
            if (lChildMPList != null)
            {
                lChildMP = lChildMPList.GetMountPoint(rChildPointName);
            }
            else
            {
                Mount lParentMount = rChild.GetComponent<Mount>();
                if (lParentMount != null) { lChildMP = lParentMount.Point; }
            }

            // If there is no child, get out
            if (lChildMP == null)
            {
                return false;
            }

            // Finally, connect the objects
            return ConnectMountPoints(lParentMP, lChildMP);
        }

        /// <summary>
        /// Conntects the child mount point to the parent
        /// </summary>
        /// <param name="rParent">String representing the parents mount point name</param>
        /// <param name="rChildItemPath">Resource path to the object we'll instanciate</param>
        /// <param name="rChildPointName">String representing the child's mount point name</param>
        /// <returns>GameObject that is the child that is instanciated</returns>
        public GameObject ConnectMountPoints(string rParentPoint, string rChildItemPath, string rChildPointName)
        {
            if (rChildItemPath.Length == 0) { return null; }

            MountPoint lParentMP = GetMountPoint(rParentPoint);
            if (lParentMP == null) { return null; }

            GameObject lChild = (GameObject)Instantiate(Resources.Load(rChildItemPath));
            if (lChild == null) { return null; }

            lChild.name = "Instanciated" + lChild.GetInstanceID();

            MountPoint lChildMP = null;
            MountPoints lChildMPList = lChild.GetComponent<MountPoints>();
            if (lChildMPList != null)
            {
                lChildMP = lChildMPList.GetMountPoint(rChildPointName);
                lChildMP.OwnerResourcePath = rChildItemPath;
            }
            else
            {
                Mount lChildMount = lChild.GetComponent<Mount>();
                if (lChildMount != null)
                {
                    lChildMP = lChildMount.Point;
                    lChildMP.OwnerResourcePath = rChildItemPath;
                }
            }

            // If there is no mount point, get out
            if (lChildMP == null)
            {
                GameObject.DestroyObject(lChild);
                return null;
            }

            // Finally, connect the objects
            ConnectMountPoints(lParentMP, lChildMP);

            // Return the newly created child
            return lChild;
        }

        /// <summary>
        /// Disconnects the child mount point from the parent
        /// </summary>
        /// <param name="rParent">Parent mount point who owns the child</param>
        /// <param name="rChild">Child mount point to disconnect</param>
        public void DisconnectMountPoints(MountPoint rParent, MountPoint rChild)
        {
            if (rParent == null) { return; }
            if (rChild == null) { return; }

            if (rChild.ParentMountPoint == rParent)
            {
                rChild.ChildTo(null);
            }
        }

        /// <summary>
        /// Disconnects thechild mount point from the parent
        /// </summary>
        /// <param name="rChildPoint"></param>
        public void DisconnectMountPoints(MountPoint rParent, GameObject rChildObject)
        {
            for (int i = rParent.ChildMountPoints.Count - 1; i >= 0; i--)
            {
                MountPoint lChildPoint = rParent.ChildMountPoints[i].MountPoint;
                if (lChildPoint != null && lChildPoint._Owner == rChildObject)
                {
                    lChildPoint.ChildTo(null);
                }
            }
        }

        /// <summary>
        /// Disconnects thechild mount point from the parent
        /// </summary>
        /// <param name="rChildPoint"></param>
        public void DisconnectMountPoints(MountPoint rChildPoint)
        {
            if (rChildPoint == null) { return; }
            if (rChildPoint.ParentMountPoint == null) { return; }

            rChildPoint.ChildTo(null);
        }

        /// <summary>
        /// Returns the first mount point with the corresponding name
        /// </summary>
        /// <param name="rName">Name to search for</param>
        /// <returns>Mount point corresponding to the name or null if not found</returns>
        public MountPoint GetMountPoint(string rName)
        {
            string lName = StringHelper.CleanString(rName);

            for (int i = 0; i < Points.Count; i++)
            {
                if (StringHelper.CleanString(Points[i].Name) == lName) { return Points[i]; }
            }

            return null;
        }

        /// <summary>
        /// Returns the first mount point tied to the specified transform
        /// </summary>
        /// <param name="rBone">Bone transform the mount point is tied to</param>
        /// <returns>Mount point corresponding to the transform or null if not found</returns>
        public MountPoint GetMountPoint(Transform rBone)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i]._Anchor != null)
                {
                    if (Points[i]._Anchor.transform.parent == rBone)
                    {
                        return Points[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the mount point with the corresponding GUID
        /// </summary>
        /// <param name="rGUID">GUID to search for</param>
        /// <returns>Mount point corresponding to the GUID or null if not found</returns>
        public MountPoint GetMountPointFromGUID(string rGUID)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i]._GUID == rGUID) { return Points[i]; }
            }

            return null;
        }

        /// <summary>
        /// Given the position, find the closest mount point. However, we don't want that mount point to be part of the
        /// current Mount or MountPoints component.
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rSourceMount"></param>
        /// <param name="rSourceMountPoints"></param>
        /// <returns></returns>
        public MountPoint GetClosestMountPoint(Vector3 rPosition)
        {
            MountPoint lMountPoint = null;
            float lDistance = float.MaxValue;

            for (int i = 0; i < Points.Count; i++)
            {
                Vector3 lTestPosition = Points[i]._Anchor.transform.position;
                float lTestDistance = Vector3.Distance(rPosition, lTestPosition);
                if (lTestDistance < lDistance)
                {
                    lDistance = lTestDistance;
                    lMountPoint = Points[i];
                }
            }

            return lMountPoint;
        }
        
        /// <summary>
        /// Retrieves the skinned item by name
        /// </summary>
        /// <param name="rName">Name of the item to retrieve</param>
        /// <returns>First SkinnedItem matching the name</returns>
        public SkinnedItem GetSkinnedItem(string rName)
        {
            for (int i = SkinnedItems.Count - 1; i >= 0; i--)
            {
                if (SkinnedItems[i]._Name == rName)
                {
                    return SkinnedItems[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the skinned item from the resource path
        /// </summary>
        /// <param name="rPath">Resource path of the item to retrieve</param>
        /// <returns>First SkinnedItem matching the resource path</returns>
        public SkinnedItem GetSkinnedItemFromPath(string rPath)
        {
            for (int i = SkinnedItems.Count - 1; i >= 0; i--)
            {
                if (SkinnedItems[i]._ResourcePath == rPath)
                {
                    return SkinnedItems[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Instanciate a child GameObject and use the parent bones in order to drive this child's skinned mesh.
        /// Since skinned meshes wrap specific bones, we don't need a parent bone to attach the child to.
        /// </summary>
        /// <param name="rItem">MountItem containing the location of the prefab and mask to instanciate</param>
        /// <returns>Boolean that determines if we added the skinned mesh</returns>
        public bool AddSkinnedItem(SkinnedItem rItem)
        {
            if (SkinnedItems.Contains(rItem)) { return false; }

            rItem.IsVisible = (rItem.ResourcePath.Length > 0); 
            rItem.IsMaskVisible = (rItem.MaskPath.Length > 0);

            // Create the instance from the path
            if (rItem.ResourcePath.Length > 0)
            {
                rItem.CreateInstance(this);
                if (rItem.GameObject == null) { return false; }
            }

            // Add the mount item to the list
            if (_UseBodyMasks && rItem.MaskPath.Length > 0)
            {
                rItem.CreateMask();
            }

            // Add the item to our list
            SkinnedItems.Add(rItem);

            // Apply the body masks if needed
            ApplyBodyMasks();

            // Return success
            return true;
        }
        
        /// <summary>
        /// Instanciate a child GameObject and use the parent bones in order to drive this child's skinned mesh.
        /// Since skinned meshes wrap specific bones, we don't need a parent bone to attach the child to.
        /// </summary>
        /// <param name="rItemPath">Path to the prefab we'll instanciate</param>
        /// <param name="rMaskPath">Path to the mask texture used to hide part of the body</param>
        /// <returns>MountItem containing the results of the instanciation</returns>
        public SkinnedItem AddSkinnedItem(string rItemPath, string rMaskPath)
        {
            if (rItemPath == null || rItemPath.Length == 0) { return null; }
            if (rMaskPath == null) { rMaskPath = ""; }

            SkinnedItem lItem = new SkinnedItem();
            lItem.ResourcePath = rItemPath;
            lItem.MaskPath = rMaskPath;

            bool lCreated = AddSkinnedItem(lItem);
            if (!lCreated) { return null; }

            return lItem;
        }

        /// <summary>
        /// Instanciate a child GameObject and use the parent bones in order to drive this child's skinned mesh.
        /// Since skinned meshes wrap specific bones, we don't need a parent bone to attach the child to.
        /// </summary>
        /// <param name="rItemPath">Path to the prefab we'll instanciate</param>
        /// <returns>MountItem containing the results of the instanciation</returns>
        public SkinnedItem AddSkinnedItem(string rItemPath)
        {
            if (rItemPath == null || rItemPath.Length == 0) { return null; }

            SkinnedItem lItem = new SkinnedItem();
            lItem.ResourcePath = rItemPath;

            bool lCreated = AddSkinnedItem(lItem);
            if (!lCreated) { return null; }

            return lItem;
        }

        /// <summary>
        /// Clears the skinned mesh items from the list
        /// </summary>
        public void ClearSkinnedItems()
        {
            for (int i = SkinnedItems.Count - 1; i >= 0; i--)
            {
                SkinnedItems[i].DestroyInstance();
            }

            SkinnedItems.Clear();

            // Clear any body masks
            ClearBodyMasks();
        }

        /// <summary>
        /// Removes the skinned mesh from the list of items
        /// </summary>
        /// <param name="rItem"></param>
        public void RemoveSkinnedItem(SkinnedItem rItem)
        {
            if (rItem == null) { return; }

            rItem.DestroyInstance();
            SkinnedItems.Remove(rItem);

            // Apply the body masks if needed
            ApplyBodyMasks();
        }

        /// <summary>
        /// Removes the skinned mesh from the list of items
        /// </summary>
        /// <param name="rItem"></param>
        public void RemoveSkinnedItem(GameObject rItem)
        {
            bool lIsRemoved = false;
            for (int i = SkinnedItems.Count - 1; i >= 0; i--)
            {
                if (SkinnedItems[i]._GameObject == rItem)
                {
                    SkinnedItems[i].DestroyInstance();
                    SkinnedItems.RemoveAt(i);

                    lIsRemoved = true;
                }
            }

            // Apply the body masks if needed
            if (lIsRemoved) { ApplyBodyMasks(); }
        }

        /// <summary>
        /// Removes the skinned mesh from the list of items
        /// </summary>
        /// <param name="rName"></param>
        public void RemoveSkinnedItem(string rName)
        {
            if (rName == null || rName.Length == 0) { return; }

            bool lIsRemoved = false;
            for (int i = SkinnedItems.Count - 1; i >= 0; i--)
            {
                if (SkinnedItems[i]._Name == rName)
                {
                    SkinnedItems[i].DestroyInstance();
                    SkinnedItems.RemoveAt(i);

                    lIsRemoved = true;
                }
            }

            // Apply the body masks if needed
            if (lIsRemoved) {  ApplyBodyMasks(); }
        }

        /// <summary>
        /// Removes the skinned mesh from the list of items
        /// </summary>
        /// <param name="rPath"></param>
        public void RemoveSkinnedItemFromPath(string rPath)
        {
            if (rPath == null || rPath.Length == 0) { return; }

            bool lIsRemoved = false;
            for (int i = SkinnedItems.Count - 1; i >= 0; i--)
            {
                if (SkinnedItems[i]._ResourcePath == rPath)
                {
                    SkinnedItems[i].DestroyInstance();
                    SkinnedItems.RemoveAt(i);

                    lIsRemoved = true;
                }
            }

            // Apply the body masks if needed
            if (lIsRemoved) { ApplyBodyMasks(); }
        }

        /// <summary>
        /// Destroys the instance of the skinned item
        /// </summary>
        /// <param name="rItem"></param>
        public void RemoveSkinnedItemInstance(SkinnedItem rItem)
        {
            if (rItem == null || rItem._GameObject == null) { return; }
            rItem.DestroyInstance();
        }

        /// <summary>
            /// Clears the body masks and reverts to the original texture colors
            /// </summary>
        public void ClearBodyMasks()
        {
            if (!_UseBodyMasks) { return; }
            if (!Application.isPlaying) { return; }

#if UNITY_EDITOR && OOTII_PROFILE
            Stopwatch lProfile = new Stopwatch();
            lProfile.Start();
#endif

            mBlitMaterial.SetTexture("_MaskTex", null);
            UnityEngine.Graphics.Blit(mBodyTexture, mBodyRenderTexture, mBlitMaterial, 0);

#if UNITY_EDITOR && OOTII_PROFILE
            lProfile.Stop();
            Utilities.Debug.Log.FileWrite("MP.ClearBodyMasks() apply-s:" + (lProfile.ElapsedTicks / (float)TimeSpan.TicksPerSecond).ToString("f5"));
#endif
        }

        /// <summary>
        /// Merges the MaskTexture of all the SkinnedItems in order to create
        /// a final body mask.
        /// </summary>
        public void ApplyBodyMasks()
        {
            if (!_UseBodyMasks) { return; }
            if (!Application.isPlaying) { return; }
            if (mBodyTexture == null) { return; }

#if UNITY_EDITOR && OOTII_PROFILE
            Stopwatch lProfile = new Stopwatch();
            lProfile.Start();
#endif

            RenderTexture lSourceRT = RenderTexture.GetTemporary(mBodyTexture.width, mBodyTexture.height, 0, mRenderTextureFormat, mRenderTextureColorSpace);
            RenderTexture lDestinationRT = null;

            mBlitMaterial.SetTexture("_MaskTex", null);
            UnityEngine.Graphics.Blit(mBodyTexture, lSourceRT, mBlitMaterial);

            for (int i = 0; i < SkinnedItems.Count; i++)
            {
                if (!SkinnedItems[i]._IsMaskVisible) { continue; }
                if (SkinnedItems[i].MaskTexture == null) { continue; }

                lDestinationRT = RenderTexture.GetTemporary(mBodyTexture.width, mBodyTexture.height, 0, mRenderTextureFormat, mRenderTextureColorSpace);

                mBlitMaterial.SetTexture("_MaskTex", SkinnedItems[i].MaskTexture);
                UnityEngine.Graphics.Blit(lSourceRT, lDestinationRT, mBlitMaterial);

                RenderTexture.ReleaseTemporary(lSourceRT);
                lSourceRT = lDestinationRT;
            }

            mBlitMaterial.SetTexture("_MaskTex", null);
            UnityEngine.Graphics.Blit(lSourceRT, mBodyRenderTexture, mBlitMaterial);

            RenderTexture.ReleaseTemporary(lSourceRT);

#if UNITY_EDITOR && OOTII_PROFILE
            lProfile.Stop();
            Utilities.Debug.Log.FileWrite("MP.blit() change-s:" + (lProfile.ElapsedTicks / (float)TimeSpan.TicksPerSecond).ToString("f5"));
#endif
        }

        // ----------------------------------- STATIC ------------------------------------

        /// <summary>
        /// Global distance for snapping
        /// </summary>
        public static float EditorSnapDistance = 0.08f;

        /// <summary>
        /// Stores a list of the unity bones by name
        /// </summary>
        public static string[] UnityBones = null;

        /// <summary>
        /// Static constructor
        /// </summary
        static MountPoints()
        {
            UnityBones = System.Enum.GetNames(typeof(HumanBodyBones));
        }

        /// <summary>
        /// Grab the closest mount point to this mount
        /// </summary>
        /// <param name="rSourceMount"></param>
        /// <returns></returns>
        public static MountPoint GetClosestMountPoint(Mount rSourceMount)
        {
            MountPoint lMountPoint = null;

            MountPoint lSourceMountPoint = rSourceMount.Point;
            Vector3 lSourceMountPosition = lSourceMountPoint._Anchor.transform.position;

            MountPoint lTestMountPoint = GetClosestMountPoint(lSourceMountPosition, rSourceMount, null);
            if (lTestMountPoint != null)
            {
                lMountPoint = lTestMountPoint;
            }

            return lMountPoint;
        }

        /// <summary>
        /// Grab the closest mount point to any of the mount points in this list.
        /// </summary>
        /// <param name="rSourceMountPoints"></param>
        /// <returns></returns>
        public static MountPoint GetClosestMountPoint(MountPoints rSourceMountPoints)
        {
            MountPoint lMountPoint = null;
            float lDistance = float.MaxValue;

            for (int i = 0; i < rSourceMountPoints.Points.Count; i++)
            {
                MountPoint lSourceMountPoint = rSourceMountPoints.Points[i];
                Vector3 lSourceMountPosition = lSourceMountPoint._Anchor.transform.position;

                MountPoint lTestMountPoint = GetClosestMountPoint(lSourceMountPosition, null, rSourceMountPoints);
                if (lTestMountPoint != null)
                {
                    Vector3 lTestPosition = lTestMountPoint._Anchor.transform.position;
                    float lTestDistance = Vector3.Distance(lSourceMountPosition, lTestPosition);
                    if (lTestDistance < lDistance)
                    {
                        lDistance = lTestDistance;
                        lMountPoint = lTestMountPoint;
                    }
                }
            }

            return lMountPoint;
        }

        /// <summary>
        /// Given the position, find the closest mount point. However, we don't want that mount point to be part of the
        /// current Mount or MountPoints component.
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rSourceMount"></param>
        /// <param name="rSourceMountPoints"></param>
        /// <returns></returns>
        public static MountPoint GetClosestMountPoint(Vector3 rPosition, Mount rSourceMount, MountPoints rSourceMountPoints)
        {
            MountPoint lMountPoint = null;
            float lDistance = float.MaxValue;

            MountPoints[] lMountPoints = Component.FindObjectsOfType<MountPoints>();
            for (int i = 0; i < lMountPoints.Length; i++)
            {
                if (lMountPoints[i] == rSourceMountPoints) { continue; }

                for (int j = 0; j < lMountPoints[i].Points.Count; j++)
                {
                    Vector3 lTestPosition = lMountPoints[i].Points[j]._Anchor.transform.position;
                    float lTestDistance = Vector3.Distance(rPosition, lTestPosition);
                    if (lTestDistance < lDistance)
                    {
                        lDistance = lTestDistance;
                        lMountPoint = lMountPoints[i].Points[j];
                    }
                }
            }

            Mount[] lMounts = Component.FindObjectsOfType<Mount>();
            for (int i = 0; i < lMounts.Length; i++)
            {
                if (lMounts[i] == rSourceMount) { continue; }

                Vector3 lTestPosition = lMounts[i].Point._Anchor.transform.position;
                float lTestDistance = Vector3.Distance(rPosition, lTestPosition);
                if (lTestDistance < lDistance)
                {
                    lDistance = lTestDistance;
                    lMountPoint = lMounts[i].Point;
                }
            }

            return lMountPoint;
        }

        /// <summary>
        /// Recursively searches for a bone given the name and returns it if found
        /// </summary>
        /// <param name="rParent">Parent to search through</param>
        /// <param name="rBoneName">Bone to find</param>
        /// <returns>Transform of the bone or null</returns>
        public static Transform FindBone(Transform rParent, string rBoneName)
        {
            Transform lBone = null;
            for (int i = 0; i < rParent.transform.childCount; i++)
            {
                lBone = FindChildBone(rParent.transform.GetChild(i), rBoneName);
                if (lBone != null) { return lBone; }
            }

            return lBone;
        }

        /// <summary>
        /// Recursively search for a bone that matches the specifie name
        /// </summary>
        /// <param name="rParent">Parent to search through</param>
        /// <param name="rBoneName">Bone to find</param>
        /// <returns></returns>
        private static Transform FindChildBone(Transform rParent, string rBoneName)
        {
            string lParentName = StringHelper.CleanString(rParent.name);
            string lBoneName = StringHelper.CleanString(rBoneName);

            // We found it. Get out fast
            if (lParentName == lBoneName) { return rParent; }
            
            // Handle the case where the bone name is nested in a namespace
            int lIndex = lParentName.IndexOf(':');
            if (lIndex >= 0)
            {
                lParentName = lParentName.Substring(lIndex + 1);
                if (lParentName == lBoneName) { return rParent; }
            }

            // Since we didn't find it, check the children
            for (int i = 0; i < rParent.transform.childCount; i++)
            {
                Transform lBone = FindChildBone(rParent.transform.GetChild(i), rBoneName);
                if (lBone != null) { return lBone; }
            }

            // Return nothing
            return null;
        }

        /// <summary>
        /// Helps us compare the names of two bones to see if they are the same. We'll
        /// do some clean up first.
        /// </summary>
        /// <param name="rName1">First bone name</param>
        /// <param name="rName2">Second bone name</param>
        /// <returns>Determines if they are the same</returns>
        public static bool CompareBoneNames(string rName1, string rName2)
        {
            if (rName1.Length == 0 || rName2.Length == 0) { return false; }
            if (string.Compare(rName1, rName2, true) == 0) { return true; }

            // Handle the case where the bone name is nested in a namespace
            int lIndex = rName1.IndexOf(':');
            if (lIndex >= 0) { rName1 = rName1.Substring(lIndex + 1); }

            lIndex = rName2.IndexOf(':');
            if (lIndex >= 0) { rName2 = rName2.Substring(lIndex + 1); }

            if (string.Compare(rName1, rName2, true) == 0) { return true; }

            // They aren't the same
            return false;
        }

        /// <summary>
        /// Converts our Unity bone name into the human bone equivalent
        /// </summary>
        /// <param name="rBoneName">Name of the human bone</param>
        /// <returns>Integer representing the human bone enumeration or 25 if it isn't found</returns>
        public static int GetHumanBodyBoneID(string rBoneName)
        {
            string lBoneName = rBoneName;
            if (lBoneName.Length > 6 && lBoneName.Substring(0, 6) == "Unity ") { lBoneName = lBoneName.Substring(6); }

            return Array.IndexOf(UnityBones, lBoneName);
        }

        /// <summary>
        /// Converts our Unity bone id into the name equivalent
        /// </summary>
        /// <param name="rBoneName">Name of the human bone</param>
        /// <returns>String representing the human bone name or an empty string if it isn't found</returns>
        public static string GetHumanBodyBoneName(HumanBodyBones rBoneID)
        {
            string lBoneName = "Unity " + rBoneID.ToString();
            return lBoneName;
        }
    }
}
