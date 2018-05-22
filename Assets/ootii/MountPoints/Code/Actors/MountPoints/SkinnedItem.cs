using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using com.ootii.Base;
using com.ootii.Geometry;

namespace com.ootii.Actors
{
    /// <summary>
    /// A mount item is created when we instanciate a skinned mesh
    /// </summary>
    [Serializable]
    public class SkinnedItem : BaseObject
    {
        /// <summary>
        /// Determines if we create an instance at start
        /// </summary>
        public bool _InstantiateOnStart = true;
        public bool InstantiateOnStart
        {
            get { return _InstantiateOnStart; }
            set { _InstantiateOnStart = value; }
        }

        /// <summary>
        /// Determines if the object is visible
        /// </summary>
        public bool _IsVisible = false;
        public bool IsVisible
        {
            get { return _IsVisible; }

            set
            {
                _IsVisible = value;

                if (_GameObject != null)
                {
                    Renderer lRenderer = _GameObject.GetComponent<Renderer>();
                    if (lRenderer == null) { lRenderer = _GameObject.GetComponentInChildren<Renderer>(); }
                    if (lRenderer != null) { lRenderer.enabled = value; }
                }
            }
        }

        /// <summary>
        /// Determines if the mask is visible. Due to the performance cost of refreshing the
        /// masks, you need to manage the MountPoints.SetBodyMasks() and MountPoints.ApplyBodyMasks()
        /// calls yourself. See MountPointsEditor.cs for an example.
        /// </summary>
        public bool _IsMaskVisible = false;
        public bool IsMaskVisible
        {
            get { return _IsMaskVisible; }
            set { _IsMaskVisible = value; }
        }

        /// <summary>
        /// It may be useful to update the "off screen" dimensions for some skinned items like
        /// shoes. This is useful if they disappear while in view.
        /// </summary>
        public bool _UpdateWhenOffScreen = false;
        public bool UpdateWhenOffScreen
        {
            get { return _UpdateWhenOffScreen; }
            set { _UpdateWhenOffScreen = value; }
        }

        /// <summary>
        /// Resource path to the prefab for the item
        /// </summary>
        public string _ResourcePath = "";
        public string ResourcePath
        {
            get { return _ResourcePath; }
            set { _ResourcePath = value; }
        }

        /// <summary>
        /// Resource path to the mask
        /// </summary>
        public string _MaskPath = "";
        public string MaskPath
        {
            get { return _MaskPath; }
            set { _MaskPath = value; }
        }

        /// <summary>
        /// GameObject that is instanciated
        /// </summary>
        public GameObject _GameObject = null;
        public GameObject GameObject
        {
            get { return _GameObject; }
        }

        /// <summary>
        /// Texture we're using as the mas
        /// </summary>
        public Texture2D MaskTexture = null;

        /// <summary>
        /// Creates the mask's cache
        /// </summary>
        public void CreateMask()
        {
            if (Application.isPlaying && _MaskPath.Length > 0)
            {
                MaskTexture = Resources.Load<Texture2D>(_MaskPath);
            }
        }


        
        /// <summary>
        /// Creates an instance of the prefab at the resource path
        /// </summary>
        /// <param name="rParent">MountPoints that will be the parent of the instance</param>
        /// <returns>Instance that was created</returns>
        public GameObject CreateInstance(MountPoints rParent)
        {
            if (_ResourcePath.Length == 0) { return null; }

            // Extract out the parent's skinned mesh renderer. This should be a high level
            // renderer that contains all the bones we'll need.
            SkinnedMeshRenderer lParentSMR = rParent._Renderer;
            if (lParentSMR == null) { lParentSMR = rParent.gameObject.GetComponent<SkinnedMeshRenderer>(); }
            if (lParentSMR == null) { lParentSMR = rParent.gameObject.GetComponentInChildren<SkinnedMeshRenderer>(); }
            if (lParentSMR == null) { return null; }

            // Create the child
            UnityEngine.Object lResource = Resources.Load(_ResourcePath);

#if UNITY_EDITOR
            if (lResource == null) { lResource = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets\\" + _ResourcePath); }
            if (lResource == null) { lResource = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets\\" + _ResourcePath + ".prefab"); }
            if (lResource == null) { lResource = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets\\" + _ResourcePath + ".fbx"); }
#endif

            if (lResource == null) { return null; }

            _GameObject = (GameObject)GameObject.Instantiate(lResource);
            if (_GameObject == null) { return null; }


            //NetworkServer.Spawn(_GameObject);

            // Don't show the child in the hierarchy
            //lChild.hideFlags = HideFlags.HideInHierarchy;

            // Extract out the Skinned Mesh Renderers
            SkinnedMeshRenderer[] lChildSMRs = _GameObject.GetComponents<SkinnedMeshRenderer>();
            if (lChildSMRs == null || lChildSMRs.Length == 0) { lChildSMRs = _GameObject.GetComponentsInChildren<SkinnedMeshRenderer>(); }

            // If there's no skinned mesh renderer, we can't really continue
            if (lChildSMRs == null || lChildSMRs.Length == 0)
            {
                DestroyInstance();
                return null;
            }

            // For each of the renderers, we need to set the bones
            List<Transform> lOldRootBones = new List<Transform>();

            for (int i = 0; i < lChildSMRs.Length; i++)
            {
                SkinnedMeshRenderer lChildSMR = lChildSMRs[i];

                Cloth lChildSMRCloth = lChildSMR.gameObject.GetComponent<Cloth>();

                // Parent it to the parent
                _GameObject.transform.parent = rParent.gameObject.transform;
                _GameObject.transform.localPosition = Vector3.zero;
                _GameObject.transform.localRotation = Quaternion.identity;
                _GameObject.transform.localScale = Vector3.one;

                // Go through the bones of the new SM and try to find matching
                // bones from the parent
                List<Transform> lChildBones = new List<Transform>();
                List<Transform> lTargetBones = new List<Transform>();

                for (int j = 0; j < lChildSMR.bones.Length; j++)
                {
                    Transform lChildBone = lChildSMR.bones[j];
                    Transform lTargetBone = null;

                    for (int k = 0; k < lParentSMR.bones.Length; k++)
                    {
                        if (MountPoints.CompareBoneNames(lParentSMR.bones[k].name, lChildBone.name))
                        {
                            lTargetBone = lParentSMR.bones[k];
                            break;
                        }
                    }

                    if (lTargetBone == null)
                    {
                        lTargetBone = MountPoints.FindBone(rParent.gameObject.transform, lChildBone.name);
                    }

                    if (lTargetBone != null)
                    {
                        lChildBones.Add(lChildBone);
                        lTargetBones.Add(lTargetBone);
                    }
                }

                lChildSMR.bones = lTargetBones.ToArray();

                // We want to grab the parent's bone that corresponds to our root. We don't
                // just take the parent's root bone since it could be different (ie for boots).
                if (!lOldRootBones.Contains(lChildSMR.rootBone)) { lOldRootBones.Add(lChildSMR.rootBone); }
                lChildSMR.rootBone = MountPoints.FindBone(rParent.gameObject.transform, lChildSMR.rootBone.name);

                // If we're dealing with clothe we need to reassign the colliders
                // Handle clothing if it exists. We may need to move colliders
                if (lChildSMRCloth != null && lChildSMRCloth.capsuleColliders != null)
                {
                    List<CapsuleCollider> lTargetColliders = new List<CapsuleCollider>();

                    for (int j = 0; j < lChildSMRCloth.capsuleColliders.Length; j++)
                    {
                        if (lChildSMRCloth.capsuleColliders[j] == null) { lTargetColliders.Add(null); continue; }

                        int lIndex = lChildBones.IndexOf(lChildSMRCloth.capsuleColliders[j].transform);

                        // If the clothing bone has a collider, our real skeleton may need one too
                        CapsuleCollider lChildCollider = lChildSMRCloth.capsuleColliders[j];
                        if (lChildCollider != null)
                        {
                            // If there's not one, create it
                            CapsuleCollider lParentCollider = lTargetBones[lIndex].GetComponent<CapsuleCollider>();
                            if (lParentCollider == null)
                            {
                                lParentCollider = lTargetBones[lIndex].gameObject.AddComponent<CapsuleCollider>();
                                lParentCollider.GetCopyOf(lChildCollider);
                            }

                            // Change the collider in the clothing
                            for (int k = 0; k < lChildSMRCloth.capsuleColliders.Length; k++)
                            {
                                if (lChildSMRCloth.capsuleColliders[k] == lChildCollider)
                                {
                                    lTargetColliders.Add(lParentCollider);
                                }
                            }
                        }
                    }

                    lChildSMRCloth.capsuleColliders = lTargetColliders.ToArray();
                }

                // Set the update flag per request
                if (_UpdateWhenOffScreen)
                {
                    lChildSMR.updateWhenOffscreen = _UpdateWhenOffScreen;
                }
            }

            // Destroy any old root bones
            for (int i = lOldRootBones.Count - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.GameObject.Destroy(lOldRootBones[i].gameObject);
                }
                else
                {
                    UnityEngine.GameObject.DestroyImmediate(lOldRootBones[i].gameObject);
                }
            }

            // Destroy any old animators
            Animator[] lOldAnimators = _GameObject.GetComponents<Animator>();
            if (lOldAnimators == null || lOldAnimators.Length == 0) { _GameObject.GetComponentsInChildren<Animator>(); }

            for (int i = lOldAnimators.Length - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Component.Destroy(lOldAnimators[i]);
                }
                else
                {
                    UnityEngine.Component.DestroyImmediate(lOldAnimators[i]);
                }
            }

            // Disable visibility if we need to
            if (!_IsVisible)
            {
                Renderer lRenderer = _GameObject.GetComponent<Renderer>();
                if (lRenderer == null) { lRenderer = _GameObject.GetComponentInChildren<Renderer>(); }
                if (lRenderer != null) { lRenderer.enabled = false; }
            }

            // Return the new game object
            return _GameObject;
        }

        /// <summary>
        /// Clears any objects that are being represented by the item
        /// </summary>
        public void DestroyInstance()
        {
            if (_GameObject != null)
            {
                // Destroy any colliders

                // Extract out the Skinned Mesh Renderers
                SkinnedMeshRenderer[] lChildSMRs = _GameObject.GetComponents<SkinnedMeshRenderer>();
                if (lChildSMRs == null || lChildSMRs.Length == 0) { lChildSMRs = _GameObject.GetComponentsInChildren<SkinnedMeshRenderer>(); }

                // For each of the skinned mesh, we need to destroy cloth colliders
                for (int i = 0; i < lChildSMRs.Length; i++)
                {
                    SkinnedMeshRenderer lChildSMR = lChildSMRs[i];

                    Cloth lChildSMRCloth = lChildSMR.gameObject.GetComponent<Cloth>();
                    if (lChildSMRCloth == null || lChildSMRCloth.capsuleColliders == null || lChildSMRCloth.capsuleColliders.Length == 0) { continue; }

                    for (int j = lChildSMRCloth.capsuleColliders.Length - 1; j >= 0; j--)
                    {
                        CapsuleCollider lChildCollider = lChildSMRCloth.capsuleColliders[j];
                        if (lChildCollider != null)
                        {
                            // Destroy the object
                            if (Application.isPlaying)
                            {
                                UnityEngine.Component.Destroy(lChildCollider);
                            }
                            else
                            {
                                UnityEngine.Component.DestroyImmediate(lChildCollider);
                            }
                        }
                    }
                }

                // Destroy the object
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(_GameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(_GameObject);
                }

                _GameObject = null;
            }
        }
    }
}
