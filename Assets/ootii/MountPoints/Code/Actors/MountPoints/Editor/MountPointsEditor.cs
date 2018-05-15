using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using com.ootii.Actors;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(MountPoints))]
public class MountPointsEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // Object we're editing
    private MountPoints mTarget;
    private SerializedObject mTargetSO;

    // List object for our layer motions
    private ReorderableList mPointList;

    // List object for our layer motions
    private ReorderableList mItemList;

    // Active mount points
    private MountPoint mSelectedPoint = null;
    private MountPoint mChildSnapPoint = null;
    private MountPoint mParentSnapPoint = null;

    // Tracks all the mount points in the scene (other than from this object)
    private List<MountPoint> mScenePoints = new List<MountPoint>();
    private List<Vector3> mScenePointPositions = new List<Vector3>();

    // Lookup for bone names
    private List<string> mBoneNames = new List<string>();

    private string mLastPath = "";

    /// <summary>
    /// Called when the script object is loaded
    /// </summary>
    void OnEnable()
    {
        LoadBoneNames();

        // Grab the serialized objects
        mTarget = (MountPoints)target;
        mTargetSO = new SerializedObject(target);

        InstanciatePointList();
        InstanciateItemList();

        // Runs through all other objects and loads the mount point
        LoadOtherMountPoints();
    }

    /// <summary>
    /// This function is called when the scriptable object goes out of scope.
    /// </summary>
    private void OnDisable()
    {
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mTargetSO.Update();

        // Clean up the mount points to ensure everything stays in sync
        PreProcessPoints();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Mount List");

        EditorHelper.DrawInspectorDescription("Manages multiple mount point and allows for others to connect. Also supports skinned meshes.", MessageType.None);

        GUILayout.Space(5);

        float lNewSnapDistance = EditorGUILayout.FloatField(new GUIContent("Snap Distance", "Snap distance for all mount points."), MountPoints.EditorSnapDistance);
        if (lNewSnapDistance != MountPoints.EditorSnapDistance)
        {
            MountPoints.EditorSnapDistance = lNewSnapDistance;
        }

        if (!IsAddMountPointEnabled(mTarget))
        {
            GUILayout.Space(5);

            EditorGUILayout.HelpBox("Unity prevents mount points from being parented directly on the prefab. Instead, add them to a prefab instance and then press 'apply' to update the prefab.", MessageType.Warning);
        }

        // Show the points
        GUILayout.BeginVertical(EditorHelper.GroupBox);
        mPointList.DoLayoutList();
        GUILayout.EndVertical();

        if (mPointList.index >= 0)
        {
            EditorGUILayout.BeginVertical(EditorHelper.Box);

            MountPoint lPoint = mTarget.Points[mPointList.index];

            bool lItemIsDirty = DrawPointDetailItem(lPoint);
            if (lItemIsDirty) { mIsDirty = true; }

            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(5);
        EditorHelper.DrawLine();
        GUILayout.Space(5);

        SkinnedMeshRenderer lNewRenderer = EditorGUILayout.ObjectField(new GUIContent("Body Skin Renderer", "Skinned Mesh Renderer containing the bones and materials we'll use."), mTarget.Renderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
        if (lNewRenderer != mTarget.Renderer)
        {
            mIsDirty = true;
            mTarget.Renderer = lNewRenderer;
        }

        bool lNewUseBodyMasks = EditorGUILayout.Toggle(new GUIContent("Use Body Masks", "Processes mask textures to hide parts of the main body."), mTarget.UseBodyMasks);
        if (lNewUseBodyMasks != mTarget.UseBodyMasks)
        {
            mIsDirty = true;
            mTarget.UseBodyMasks = lNewUseBodyMasks;
        }

        if (mTarget.UseBodyMasks)
        {
            int lNewBodyMaskIndex = EditorGUILayout.IntField(new GUIContent("  Material Index", "Index of the material that is the skin texture to modify."), mTarget.MaterialIndex);
            if (lNewBodyMaskIndex != mTarget.MaterialIndex)
            {
                mIsDirty = true;
                mTarget.MaterialIndex = lNewBodyMaskIndex;
            }

            EditorHelper.DrawInspectorDescription("Ensure your actor's body material Rendering Mode is set to 'Cutout' or 'Transparency'.", MessageType.None);
            GUILayout.Space(5);
        }

        // Show the skinned items
        GUILayout.BeginVertical(EditorHelper.GroupBox);
        mItemList.DoLayoutList();
        GUILayout.EndVertical();

        if (mItemList.index >= 0)
        {
            if (mItemList.index < mTarget.SkinnedItems.Count)
            {
                EditorGUILayout.BeginVertical(EditorHelper.Box);

                SkinnedItem lItem = mTarget.SkinnedItems[mItemList.index];

                bool lItemIsDirty = DrawItemDetailItem(lItem);
                if (lItemIsDirty) { mIsDirty = true; }

                EditorGUILayout.EndVertical();
            }
            else
            {
                mItemList.index = -1;
            }
        }

        GUILayout.Space(5);

        // If there is a change... update.
        if (mIsDirty)
        {
            // Flag the object as needing to be saved
            EditorUtility.SetDirty(mTarget);

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            EditorApplication.MarkSceneDirty();
#else
            if (!EditorApplication.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
#endif

            // Pushes the values back to the runtime so it has the changes
            mTargetSO.ApplyModifiedProperties();

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstanciatePointList()
    {
        SerializedProperty lPoints = mTargetSO.FindProperty("Points");
        mPointList = new ReorderableList(mTargetSO, lPoints, true, true, true, true);

        mPointList.drawHeaderCallback = DrawPointListHeader;
        mPointList.drawFooterCallback = DrawPointListFooter;
        mPointList.drawElementCallback = DrawPointListItem;
        mPointList.onAddCallback = OnPointListItemAdd;
        mPointList.onRemoveCallback = OnPointListItemRemove;
        mPointList.onSelectCallback = OnPointListItemSelect;
        mPointList.onReorderCallback = OnPointListReorder;
        mPointList.footerHeight = 17f;

        if (mTarget.EditorPointIndex < mPointList.count)
        {
            mPointList.index = mTarget.EditorPointIndex;
            OnPointListItemSelect(mPointList);
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawPointListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Mount Points");
        if (GUI.Button(rRect, "", EditorStyles.label))
        {
            mPointList.index = -1;
            OnPointListItemSelect(mPointList);
        }
    }

    /// <summary>
    /// Allows us to draw each item in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawPointListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mTarget.Points.Count)
        {
            MountPoint lPoint = mTarget.Points[rIndex];

            Rect lNameRect = new Rect(rRect.x, rRect.y + 1, rRect.width - 25, EditorGUIUtility.singleLineHeight);
            string lNewName = EditorGUI.TextField(lNameRect, lPoint.Name);
            if (lNewName != lPoint.Name)
            {
                mIsDirty = true;
                lPoint.Name = lNewName;
            }

            Rect lLockedRect = new Rect(lNameRect.x + lNameRect.width + 5, rRect.y + 1, 20, EditorGUIUtility.singleLineHeight);
            bool lNewIsLocked = EditorGUI.Toggle(lLockedRect, lPoint.IsLocked);
            if (lNewIsLocked != lPoint.IsLocked)
            {
                mIsDirty = true;
                lPoint.IsLocked = lNewIsLocked;
            }
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawPointListFooter(Rect rRect)
    {
        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add mount point."), EditorStyles.miniButtonLeft)) { OnPointListItemAdd(mPointList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete mount point."), EditorStyles.miniButtonRight)) { OnPointListItemRemove(mPointList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnPointListItemAdd(ReorderableList rList)
    {
        // Create the mount point and initialize it
        mSelectedPoint = mTarget.CreateMountPoint("New Mount Point", "", true);

        // Make it the selected one
        mPointList.index = mTarget.Points.Count - 1;
        OnPointListItemSelect(rList);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnPointListItemSelect(ReorderableList rList)
    {
        mTarget.EditorPointIndex = rList.index;

        if (mTarget.EditorPointIndex == -1)
        {
            mSelectedPoint = null;
        }
        else
        {
            mSelectedPoint = mTarget.Points[mTarget.EditorPointIndex];
        }

        if (SceneView.sceneViews.Count > 0)
        {
            SceneView lSceneView = (SceneView)SceneView.sceneViews[0];
            //lSceneView.Focus();
            lSceneView.Repaint();
        }
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnPointListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int lIndex = rList.index;

            // Grab the mount point to remove so we can destroy it
            MountPoint lPoint = mTarget.Points[lIndex];
            if (lPoint.Anchor != null) { GameObject.DestroyImmediate(lPoint.Anchor, true); }

            // Now remove it
            mTarget.Points.RemoveAt(lIndex);

            // Select the next item
            rList.index--;
            OnPointListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnPointListReorder(ReorderableList rList)
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Renders the properties of the motion so they can be changed here
    /// </summary>
    /// <param name="rLayerIndex">Layer the motion belongs to</param>
    /// <param name="rMotionIndex">Motions whose properites are to be listed</param>
    private bool DrawPointDetailItem(MountPoint rPoint)
    {
        bool lIsDirty = false;

        if (rPoint._Anchor == null && rPoint._BoneName.Length > 0)
        {
            rPoint.AnchorTo(rPoint._BoneName);
        }

        EditorHelper.DrawSmallTitle(rPoint.Name);

        EditorGUILayout.BeginHorizontal();

        // Friendly name
        string lNewName = EditorGUILayout.TextField(new GUIContent("Name", "Friendly name of the mount point."), rPoint.Name);
        if (lNewName != rPoint.Name)
        {
            lIsDirty = true;
            rPoint.Name = lNewName;
        }

        GUILayout.Space(5);

        // Locked
        EditorGUILayout.LabelField(new GUIContent("Locked", "Determines if the MP can be moved."), GUILayout.Width(43));
        bool lNewIsLocked = EditorGUILayout.Toggle(rPoint.IsLocked, GUILayout.Width(16));
        if (lNewIsLocked != rPoint.IsLocked)
        {
            lIsDirty = true;
            rPoint.IsLocked = lNewIsLocked;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Bone
        string lBoneName = rPoint.BoneName;

        int lOriginalSelectedBoneIndex = GetBoneIndex(lBoneName);
        int lSelectedBoneIndex = EditorGUILayout.Popup("Bone", lOriginalSelectedBoneIndex, mBoneNames.ToArray());
        if (lSelectedBoneIndex != lOriginalSelectedBoneIndex && lSelectedBoneIndex == (mBoneNames.Count - 1)) { lBoneName = ""; }

        string lNewBoneName = lBoneName;
        if (lSelectedBoneIndex == (mBoneNames.Count - 1))
        {
            lNewBoneName = EditorGUILayout.TextField(new GUIContent("Bone Name", "Full path and name to the bone the MP should anchor to."), lBoneName);
        }
        else
        {
            lNewBoneName = mBoneNames[lSelectedBoneIndex];
        }

        bool lIsBoneDirty = false;
        if (lNewBoneName != lBoneName)
        {
            lIsDirty = true;
            lIsBoneDirty = true;

            rPoint.BoneName = lNewBoneName;
        }

        GUILayout.Space(5);

        // Position
        Vector3 lPositionValues = rPoint.Anchor.transform.localPosition;
        lPositionValues.x = Convert.ToSingle(lPositionValues.x.ToString("0.0000"));
        lPositionValues.y = Convert.ToSingle(lPositionValues.y.ToString("0.0000"));
        lPositionValues.z = Convert.ToSingle(lPositionValues.z.ToString("0.0000"));
        Vector3 lNewPositionValues = EditorGUILayout.Vector3Field(new GUIContent("Position", "Position relative to the transform the MP belongs to."), lPositionValues);
        if (lNewPositionValues != lPositionValues)
        {
            lIsDirty = true;
            rPoint.Anchor.transform.localPosition = lNewPositionValues;
        }

        // Rotation
        Vector3 lRotationValues = rPoint.Anchor.transform.localRotation.eulerAngles;
        lRotationValues.x = Convert.ToSingle(lRotationValues.x.ToString("0.0000"));
        lRotationValues.y = Convert.ToSingle(lRotationValues.y.ToString("0.0000"));
        lRotationValues.z = Convert.ToSingle(lRotationValues.z.ToString("0.0000"));
        Vector3 lNewRotationValues = EditorGUILayout.Vector3Field(new GUIContent("Orientation (p, y, r)", "Rotation relative to the transform the MP belongs to."), lRotationValues);
        if (lNewRotationValues != lRotationValues)
        {
            lIsDirty = true;
            rPoint.Anchor.transform.localRotation = Quaternion.Euler(lNewRotationValues);
        }

        GUILayout.Space(5);

        // Allow children
        bool lNewAllowChildren = EditorGUILayout.Toggle(new GUIContent("Allow Children", "Determines if this mount point can have children mounted to it."), rPoint.AllowChildren);
        if (lNewAllowChildren != rPoint.AllowChildren)
        {
            lIsDirty = true;
            rPoint.AllowChildren = lNewAllowChildren;
        }

        // Set child orientation
        bool lNewForceOrientation = EditorGUILayout.Toggle(new GUIContent("Set Child Orientation", "Determines if this MP will force the child to rotate to match its orientation."), rPoint.ForceChildOrientation);
        if (lNewForceOrientation != rPoint.ForceChildOrientation)
        {
            lIsDirty = true;
            rPoint.ForceChildOrientation = lNewForceOrientation;
        }

        if (rPoint.ForceChildOrientation)
        {
            bool lNewInvertOrientation = EditorGUILayout.Toggle(new GUIContent("Invert Orientation", "Instead of aligning the orientations, ensure the z-axis face each other."), rPoint.InvertOrientation);
            if (lNewInvertOrientation != rPoint.InvertOrientation)
            {
                lIsDirty = true;
                rPoint.InvertOrientation = lNewInvertOrientation;
            }
        }

        // Ignore parent scale
        bool lNewIgnoreParentScale = EditorGUILayout.Toggle(new GUIContent("Preserve Child Scale", "Determines if this MP will prevent children from scaling when they connect."), rPoint.IgnoreParentScale);
        if (lNewIgnoreParentScale != rPoint.IgnoreParentScale)
        {
            lIsDirty = true;
            rPoint.IgnoreParentScale = lNewIgnoreParentScale;
        }

        // Render the parent mount point
        if (rPoint.ParentMountPoint != null)
        {
            EditorHelper.DrawLine();

            MountPoint lParentPoint = rPoint.ParentMountPoint;

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Parent:", GUILayout.Width(45));
            GUILayout.Label(lParentPoint.Owner.name + "." + lParentPoint.Name, GUILayout.MinWidth(100));

            if (GUILayout.Button(new GUIContent("", "Select parent"), EditorHelper.BlueSelectButton, GUILayout.Width(16), GUILayout.Height(16)))
            {
                Selection.activeGameObject = lParentPoint.Owner;
            }

            if (GUILayout.Button(new GUIContent("", "Break connection"), EditorHelper.RedXButton, GUILayout.Width(16), GUILayout.Height(16)))
            {
                DisconnectMountPoints(rPoint, lParentPoint);
            }

            GUILayout.Space(2);

            EditorGUILayout.EndHorizontal();
        }

        // Render the children mount points
        if (rPoint.ChildMountPoints.Count > 0)
        {
            EditorHelper.DrawLine();

            // Show the remaining child points
            for (int i = 0; i < rPoint.ChildMountPoints.Count; i++)
            {
                MountPoint lChildPoint = rPoint.ChildMountPoints[i].MountPoint;
                if (lChildPoint.Owner == null) { continue; }

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Child:", GUILayout.Width(40));
                GUILayout.Label(lChildPoint.Owner.name + "." + lChildPoint.Name, GUILayout.MinWidth(100));

                if (GUILayout.Button(new GUIContent("", "Select child"), EditorHelper.BlueSelectButton, GUILayout.Width(16), GUILayout.Height(16)))
                {
                    Selection.activeGameObject = lChildPoint.Owner;
                }

                if (GUILayout.Button(new GUIContent("", "Break connection"), EditorHelper.RedXButton, GUILayout.Width(16), GUILayout.Height(16)))
                {
                    DisconnectMountPoints(lChildPoint, rPoint);
                }

                GUILayout.Space(2);

                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(5);

        if (lIsBoneDirty)
        {
            UpdateMountPointBone(ref rPoint);
        }

        if (lIsDirty)
        {
            LoadOtherMountPoints();
        }

        return lIsDirty;
    }

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstanciateItemList()
    {
        SerializedProperty lItems = mTargetSO.FindProperty("SkinnedItems");
        mItemList = new ReorderableList(mTargetSO, lItems, true, true, true, true);

        mItemList.drawHeaderCallback = DrawItemListHeader;
        mItemList.drawFooterCallback = DrawItemListFooter;
        mItemList.drawElementCallback = DrawItemListItem;
        mItemList.onAddCallback = OnItemListItemAdd;
        mItemList.onRemoveCallback = OnItemListItemRemove;
        mItemList.onSelectCallback = OnItemListItemSelect;
        mItemList.onReorderCallback = OnItemListReorder;
        mItemList.footerHeight = 17f;

        if (mTarget.EditorItemIndex < mItemList.count)
        {
            mItemList.index = mTarget.EditorItemIndex;
            OnItemListItemSelect(mItemList);
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawItemListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Skinned Items");
        if (GUI.Button(rRect, "", EditorStyles.label))
        {
            mItemList.index = -1;
            OnItemListItemSelect(mItemList);
        }
    }

    /// <summary>
    /// Allows us to draw each item in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawItemListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mTarget.SkinnedItems.Count)
        {
            SkinnedItem lItem = mTarget.SkinnedItems[rIndex];

            if (lItem.Name.Length > 0 || lItem.ResourcePath.Length == 0)
            {
                Rect lNameRect = new Rect(rRect.x, rRect.y + 1, rRect.width - 25, EditorGUIUtility.singleLineHeight);
                string lNewName = EditorGUI.TextField(lNameRect, lItem.Name);
                if (lNewName != lItem.Name)
                {
                    mIsDirty = true;
                    lItem.Name = lNewName;
                }
            }
            else
            {
                Rect lNameRect = new Rect(rRect.x, rRect.y + 1, rRect.width - 25, EditorGUIUtility.singleLineHeight);
                EditorGUI.TextField(lNameRect, lItem.ResourcePath);
            }
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawItemListFooter(Rect rRect)
    {
        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add skinned item."), EditorStyles.miniButtonLeft)) { OnItemListItemAdd(mItemList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete skinned item."), EditorStyles.miniButtonRight)) { OnItemListItemRemove(mItemList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListItemAdd(ReorderableList rList)
    {
        // Create a skinned item
        SkinnedItem lItem = new SkinnedItem();
        mTarget.AddSkinnedItem(lItem);

        // Set the visible values after we add it 
        lItem.IsVisible = true;
        lItem.IsMaskVisible = true;

        // Make it the selected one
        mItemList.index = mTarget.SkinnedItems.Count - 1;
        OnItemListItemSelect(rList);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListItemSelect(ReorderableList rList)
    {
        mTarget.EditorItemIndex = rList.index;

        if (mTarget.EditorItemIndex == -1)
        {
        }
        else
        {
        }
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int lIndex = rList.index;

            // Grab the mount point to remove so we can destroy it
            SkinnedItem lItem = mTarget.SkinnedItems[lIndex];
            mTarget.RemoveSkinnedItem(lItem);

            // Select the next item
            rList.index--;
            OnItemListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListReorder(ReorderableList rList)
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Renders the properties of the motion so they can be changed here
    /// </summary>
    /// <param name="rLayerIndex">Layer the motion belongs to</param>
    /// <param name="rMotionIndex">Motions whose properites are to be listed</param>
    private bool DrawItemDetailItem(SkinnedItem rItem)
    {
        bool lIsDirty = false;

        EditorHelper.DrawSmallTitle(rItem.Name);

        // Warning about the resource not being found
        if (rItem._GameObject == null && rItem._ResourcePath.Length > 0 && Resources.Load(rItem._ResourcePath) == null)
        {
            EditorHelper.DrawInspectorDescription("Resource not found. If you want to instantiate the item at run-time, please ensure the file is in a 'Resources' folder.", MessageType.None);
            GUILayout.Space(5);
        }
        else
        {
            EditorGUILayout.LabelField("", GUILayout.Height(1f));
        }

        // Friendly name
        string lNewName = EditorGUILayout.TextField(new GUIContent("Name", "Friendly name of the skinned item."), rItem.Name);
        if (lNewName != rItem.Name)
        {
            lIsDirty = true;
            rItem.Name = lNewName;
        }

        // Resource Path
        EditorGUILayout.BeginHorizontal();
             
        string lNewResourcePath = EditorGUILayout.TextField(new GUIContent("Resource Path", "Path to the prefab resource that is the item."), rItem.ResourcePath);
        if (lNewResourcePath != rItem.ResourcePath)
        {
            lIsDirty = true;
            rItem.ResourcePath = lNewResourcePath;
        }

        if (GUILayout.Button(new GUIContent("...", "Select resource"), EditorStyles.miniButton, GUILayout.Width(20)))
        {
            lNewResourcePath = EditorUtility.OpenFilePanel("Select the file", mLastPath, "fbx,prefab");
            if (lNewResourcePath.Length != 0)
            {
                mLastPath = lNewResourcePath;

                int lStartResource = lNewResourcePath.IndexOf("Resources");
                if (lStartResource >= 0)
                {
                    lNewResourcePath = lNewResourcePath.Substring(lStartResource + 10);
                }

                lStartResource = lNewResourcePath.IndexOf("Assets");
                if (lStartResource >= 0)
                {
                    lNewResourcePath = lNewResourcePath.Substring(lStartResource + 7);
                }

                int lStartExtension = lNewResourcePath.LastIndexOf(".");
                if (lStartExtension > 0)
                {
                    lNewResourcePath = lNewResourcePath.Substring(0, lStartExtension);
                }

                if (lNewResourcePath != rItem.ResourcePath)
                {
                    lIsDirty = true;
                    rItem.ResourcePath = lNewResourcePath;
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        // Mask Path
        EditorGUILayout.BeginHorizontal();

        string lNewMaskPath = EditorGUILayout.TextField(new GUIContent("Mask Path", "Path to the texture resource that will mask the body."), rItem.MaskPath);
        if (lNewMaskPath != rItem.MaskPath)
        {
            lIsDirty = true;

            // Now we can set the mask
            rItem.MaskPath = lNewMaskPath;

            // If it was visible or if it is now, we need to apply the mask
            if (Application.isPlaying && mTarget.UseBodyMasks)
            {
                rItem.CreateMask();
                mTarget.ApplyBodyMasks();
            }
        }

        if (GUILayout.Button(new GUIContent("...", "Select mask"), EditorStyles.miniButton, GUILayout.Width(20)))
        {
            lNewMaskPath = EditorUtility.OpenFilePanel("Select the file", mLastPath, "png,jpg,psd");
            if (lNewMaskPath.Length != 0)
            {
                mLastPath = lNewMaskPath;

                int lStartResource = lNewMaskPath.IndexOf("Resources");
                if (lStartResource >= 0)
                {
                    lNewMaskPath = lNewMaskPath.Substring(lStartResource + 10);
                }

                lStartResource = lNewMaskPath.IndexOf("Assets");
                if (lStartResource >= 0)
                {
                    lNewMaskPath = lNewMaskPath.Substring(lStartResource + 7);
                }

                int lStartExtension = lNewMaskPath.LastIndexOf(".");
                if (lStartExtension > 0)
                {
                    lNewMaskPath = lNewMaskPath.Substring(0, lStartExtension);
                }

                if (lNewMaskPath != rItem.MaskPath)
                {
                    lIsDirty = true;

                    // Now we can set the mask
                    rItem.MaskPath = lNewMaskPath;

                    // If it was visible or if it is now, we need to apply the mask
                    if (Application.isPlaying && mTarget.UseBodyMasks)
                    {
                        rItem.CreateMask();
                        mTarget.ApplyBodyMasks();
                    }
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        bool lNewInstanciateOnStart = EditorGUILayout.Toggle(new GUIContent("Instantiate On Start", "Determines if we create an instance of the object and mask when the game starts."), rItem.InstantiateOnStart);
        if (lNewInstanciateOnStart != rItem.InstantiateOnStart)
        {
            lIsDirty = true;
            rItem.InstantiateOnStart = lNewInstanciateOnStart;
        }

        bool lNewIsVisible = EditorGUILayout.Toggle(new GUIContent("Is Instance Visible", "Determines if the object itself is visible."), rItem.IsVisible);
        if (lNewIsVisible != rItem.IsVisible)
        {
            lIsDirty = true;
            rItem.IsVisible = lNewIsVisible;
        }

        bool lNewIsMaskVisible = EditorGUILayout.Toggle(new GUIContent("Is Mask Visible", "Determines if the body mask is visible."), rItem.IsMaskVisible);
        if (lNewIsMaskVisible != rItem.IsMaskVisible)
        {
            lIsDirty = true;

            // Now we can set the visability
            rItem.IsMaskVisible = lNewIsMaskVisible;

            // If it was visible or if it is now, we need to apply the mask
            if (Application.isPlaying && mTarget.UseBodyMasks)
            {
                mTarget.ApplyBodyMasks();
            }
        }

        bool lNewUpdateWhenOffScreen = EditorGUILayout.Toggle(new GUIContent("Update Offscreen", "Useful if Unity hides the item at-runtime."), rItem.UpdateWhenOffScreen);
        if (lNewUpdateWhenOffScreen != rItem.UpdateWhenOffScreen)
        {
            lIsDirty = true;
            rItem.UpdateWhenOffScreen = lNewUpdateWhenOffScreen;
        }

        // Object
        EditorHelper.DrawLine();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Instance:", GUILayout.Width(60));

        if (rItem.GameObject == null)
        {
            GUILayout.Label("<none>", GUILayout.MinWidth(100));

            if (GUILayout.Button(new GUIContent("", "Create instance"), EditorHelper.BlueAddButton, GUILayout.Width(16), GUILayout.Height(16)))
            {
                rItem.CreateInstance(mTarget);

                if (mTarget.UseBodyMasks)
                {
                    rItem.CreateMask();
                    mTarget.ApplyBodyMasks();
                }
            }
        }
        else
        {
            GUILayout.Label(rItem.GameObject.name, GUILayout.MinWidth(100));

            if (GUILayout.Button(new GUIContent("", "Select instance"), EditorHelper.BlueSelectButton, GUILayout.Width(16), GUILayout.Height(16)))
            {
                Selection.activeGameObject = rItem.GameObject;
            }

            if (GUILayout.Button(new GUIContent("", "Delete instance"), EditorHelper.RedXButton, GUILayout.Width(16), GUILayout.Height(16)))
            {
                mTarget.RemoveSkinnedItemInstance(rItem);
            }
        }

        GUILayout.Space(2);

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        return lIsDirty;
    }

    /// <summary>
    /// Renders gizmos and GUI to the scene view
    /// </summary>
    private void OnSceneGUI()
    {
        Event lCurrent = Event.current;
        switch (lCurrent.type)
        {
            case EventType.MouseUp:
                FindSnapPoints();
                TestAndBreakConnections();
                ConnectMountPoints(mChildSnapPoint, mParentSnapPoint);
                break;
        }

        Color lMountColor = Color.yellow;
        Color lOuterMountColor = lMountColor;
        lOuterMountColor.a = 0.075f;

        Color lSelectedMountColor = Color.green;
        Color lSelectedOuterMountColor = lSelectedMountColor;
        lSelectedOuterMountColor.a = 0.075f;

        Color lOtherMountColor = Color.white;
        Color lOtherOuterMountColor = lMountColor;
        lOtherOuterMountColor.a = 0.075f;

        // Highlight all the mount points on the object
        int lCount = mTarget.Points.Count;
        for (int i = 0; i < lCount; i++)
        {
            if (mTarget.Points[i]._Anchor == null) { continue; }

            EditorHelper.DrawCircle(mTarget.Points[i].Anchor.transform.position, 0.01f, (i == mPointList.index ? lSelectedMountColor : lMountColor));
            EditorHelper.DrawCircle(mTarget.Points[i].Anchor.transform.position, 0.05f, (i == mPointList.index ? lSelectedOuterMountColor : lOuterMountColor));
            EditorHelper.DrawText(mTarget.Points[i].Name, mTarget.Points[i].Anchor.transform.position + (Vector3.up * 0.075f), (i == mPointList.index ? lSelectedMountColor : lMountColor));
        }

        // Allow the selected mount point to be moved
        if (mSelectedPoint != null && mSelectedPoint.Anchor != null)
        {
            Vector3 lNewPosition = mSelectedPoint.Anchor.transform.position;

            if (mSelectedPoint.IsLocked)
            {
                Handles.color = Color.blue;
                Handles.DrawLine(mSelectedPoint.Anchor.transform.position, mSelectedPoint.Anchor.transform.position + (mSelectedPoint.Anchor.transform.forward * 0.2f));

                Handles.color = Color.red;
                Handles.DrawLine(mSelectedPoint.Anchor.transform.position, mSelectedPoint.Anchor.transform.position + (mSelectedPoint.Anchor.transform.right * 0.2f));

                Handles.color = Color.green;
                Handles.DrawLine(mSelectedPoint.Anchor.transform.position, mSelectedPoint.Anchor.transform.position + (mSelectedPoint.Anchor.transform.up * 0.2f));
            }
            else
            {
                lNewPosition = Handles.PositionHandle(mSelectedPoint.Anchor.transform.position, mSelectedPoint.Anchor.transform.rotation);

                LoadOtherMountPoints();
            }

            // Update any data
            if (GUI.changed)
            {
                bool lIsDirty = false;
                lIsDirty = (lIsDirty || (mSelectedPoint.Anchor.transform.position != lNewPosition));

                if (lIsDirty)
                {
                    mIsDirty = true;

                    mSelectedPoint.Anchor.transform.position = lNewPosition;
                }
            }
        }

        // Render all the anchors in the scene
        if (mScenePointPositions.Count > 0)
        {
            for (int i = 0; i < mScenePointPositions.Count; i++)
            {
                EditorHelper.DrawCircle(mScenePointPositions[i], 0.01f, lOtherMountColor);
            }
        }
    }

    /// <summary>
    /// Run through the mount points to ensure everything is connected to 
    /// a valid object. This way we can clean up before we process.
    /// </summary>
    private void PreProcessPoints()
    {
        // Clean up mount points to ensure everything stays in synch
        for (int i = mTarget.Points.Count - 1; i >= 0; i--)
        {
            MountPoint lPoint = mTarget.Points[i];

            // Ensure the mount point is still valid
            if (lPoint.Owner == null)
            {
                lPoint.ChildTo(null);
                mTarget.Points.Remove(lPoint);
            }
            // Ensure any parent we may have is valid
            else if (lPoint.ParentMountPoint != null && lPoint.ParentMountPoint.Owner == null)
            {
                lPoint.ChildTo(null);
            }
            // Process the mount point properties
            else
            {
                // We force it here so that if the owner changes scale, we can reset it
                lPoint.IgnoreParentScale = lPoint.IgnoreParentScale;

                // Ensure all the child mount points are valid. It's possible that the 
                // owner of a child mount point has been deleted. We need to check for that first
                for (int j = lPoint.ChildMountPoints.Count - 1; j >= 0; j--)
                {
                    MountPoint lChildPoint = lPoint.ChildMountPoints[j].MountPoint;
                    if (lChildPoint == null || lChildPoint.Owner == null)
                    {
                        lPoint.ChildMountPoints.RemoveAt(j);
                        mIsDirty = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update the mount point with the new anchor information
    /// </summary>
    /// <param name="rMountPoint">Mount point to update</param>
    private void UpdateMountPointBone(ref MountPoint rMountPoint)
    {
        rMountPoint.AnchorTo(rMountPoint.BoneName);

        // Flag the list as needing updating
        mIsDirty = true;
    }

    /// <summary>
    /// Use this function to find the closest snap points and put them together
    /// </summary>
    /// <returns>Boolean that determines if a snap point was found</returns>
    private bool FindSnapPoints()
    {
        float lMinDistance = float.MaxValue;
        MountPoint lChildSnapPoint = null;
        MountPoint lParentSnapPoint = null;

        // Cycle through our mount points and compare them to other 
        // object mount points. If we find one close enough, we'll snap
        for (int i = 0; i < mTarget.Points.Count; i++)
        {
            for (int j = 0; j < mScenePointPositions.Count; j++)
            {
                float lDistance = Vector3.Distance(mTarget.Points[i].Anchor.transform.position, mScenePointPositions[j]);
                if (lDistance < MountPoints.EditorSnapDistance && lDistance < lMinDistance)
                {
                    lMinDistance = lDistance;
                    lChildSnapPoint = mTarget.Points[i];
                    lParentSnapPoint = mScenePoints[j];
                }
            }
        }

        // Test if we need to invert the parenting
        if (lChildSnapPoint != null && lParentSnapPoint != null)
        {
            // Flip the snap points if we have a selected point
            if (mSelectedPoint != null && mSelectedPoint == lParentSnapPoint)
            {
                lParentSnapPoint = lChildSnapPoint;
                lChildSnapPoint = mSelectedPoint;
            }

            // Ensure they aren't already parented the other way
            if (lParentSnapPoint.ParentMountPoint == lChildSnapPoint)
            {
                MountPoint lTemp = lParentSnapPoint;
                lParentSnapPoint = lChildSnapPoint;
                lChildSnapPoint = lTemp;
            }

            // If the parent doesn't allow children, don't snap
            if (!lParentSnapPoint.AllowChildren)
            {
                lParentSnapPoint = null;
                lChildSnapPoint = null;
            }
        }

        // Set the resulting snap points (if found)
        mChildSnapPoint = lChildSnapPoint;
        mParentSnapPoint = lParentSnapPoint;

        return (mChildSnapPoint != null);
    }

    /// <summary>
    /// Cycle through all the mount points for this object and
    /// break any connections whose distance exceeds the snap distance
    /// </summary>
    private void TestAndBreakConnections()
    {
        // Cycle through our mount points and compare them to other 
        // object mount points. If we find one close enough, we'll snap
        for (int i = 0; i < mTarget.Points.Count; i++)
        {
            MountPoint lMountPoint = mTarget.Points[i];
            if (lMountPoint.ParentMountPoint != null)
            {
                float lDistance = Vector3.Distance(lMountPoint.Anchor.transform.position, lMountPoint.ParentMountPoint.Anchor.transform.position);
                if (lDistance > MountPoints.EditorSnapDistance)
                {
                    DisconnectMountPoints(lMountPoint, lMountPoint.ParentMountPoint);
                }
            }
        }
    }

    /// <summary>
    /// Manages the connection between two mount points (and thier owners)
    /// </summary>
    /// <param name="rChild"></param>
    /// <param name="rParent"></param>
    private void ConnectMountPoints(MountPoint rChild, MountPoint rParent)
    {
        if (rChild == null) { return; }
        if (!rChild.IsLocked) { return; }
        if (rParent != null && !rParent.AllowChildren) { return; }

        // Parent the two
        rChild.ChildTo(rParent);

        // Flag the list as needing updating
        mIsDirty = true;
    }

    /// <summary>
    /// Breaks the connection between the child and parent mount points
    /// </summary>
    /// <param name="rChild">Child mount point to break</param>
    private void DisconnectMountPoints(MountPoint rChild, MountPoint rParent)
    {
        if (rChild == null) { return; }
        rChild.ChildTo(null);

        // Sanity check to ensure the child is removed from the parent
        if (rParent != null) { rParent.RemoveChild(rChild); }

        // Flag the list as needing updating
        mIsDirty = true;
    }

    /// <summary>
    /// Retrieves the serialized property that represents the motion
    /// </summary>
    /// <param name="rLayerIndex"></param>
    /// <param name="rMotionIndex"></param>
    /// <returns></returns>
    private SerializedProperty GetSerializedMountPoint(int rPointIndex)
    {
        // Now we need to update the serialized object
        SerializedProperty lPointListSP = mTargetSO.FindProperty("MountPoints");
        if (lPointListSP != null)
        {
            if (lPointListSP.arraySize > rPointIndex)
            {
                SerializedProperty lPointItemSP = lPointListSP.GetArrayElementAtIndex(rPointIndex);
                if (lPointItemSP != null)
                {
                    return lPointItemSP;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Loads the mounts points that are part of object in the scene and
    /// NOT this one.
    /// </summary>
    private void LoadOtherMountPoints()
    {
        mScenePoints.Clear();
        mScenePointPositions.Clear();

        UnityEngine.Object[] lScenePointLists = Resources.FindObjectsOfTypeAll(typeof(MountPoints));
        for (int i = 0; i < lScenePointLists.Length; i++)
        {
            MountPoints lScenePointList = lScenePointLists[i] as MountPoints;
            if (lScenePointList == mTarget) { continue; }
            if (lScenePointList.gameObject.activeInHierarchy == false) { continue; }

            for (int j = 0; j < lScenePointList.Points.Count; j++)
            {
                MountPoint lPoint = lScenePointList.Points[j] as MountPoint;
                if (lPoint.Owner != null && lPoint.Anchor != null)
                {
                    mScenePoints.Add(lPoint);
                    mScenePointPositions.Add(lPoint.Anchor.transform.position);
                }
            }
        }

        lScenePointLists = Resources.FindObjectsOfTypeAll(typeof(Mount));
        for (int i = 0; i < lScenePointLists.Length; i++)
        {
            Mount lScenePoint = lScenePointLists[i] as Mount;
            if (lScenePoint == mTarget) { continue; }
            if (lScenePoint.gameObject.activeInHierarchy == false) { continue; }

            MountPoint lPoint = lScenePoint.Point;
            if (lPoint.Owner != null && lPoint.Anchor != null)
            {
                mScenePoints.Add(lPoint);
                mScenePointPositions.Add(lPoint.Anchor.transform.position);
            }
        }
    }

    /// <summary>
    /// Creates a texture given the specified color
    /// </summary>
    /// <param name="rWidth">Width of the texture</param>
    /// <param name="rHeight">Height of the texture</param>
    /// <param name="rColor">Color of the texture</param>
    /// <returns></returns>
    private Texture2D CreateTexture(int rWidth, int rHeight, Color rColor)
    {
        Color[] lPixels = new Color[rWidth * rHeight];
        for (int i = 0; i < lPixels.Length; i++)
        {
            lPixels[i] = rColor;
        }

        Texture2D result = new Texture2D(rWidth, rHeight);
        result.SetPixels(lPixels);
        result.Apply();

        return result;
    }

    /// <summary>
    /// Builds the list of bones to tie the mount point to
    /// </summary>
    private void LoadBoneNames()
    {
        mBoneNames.Clear();

        for (int i = 0; i < MountPoints.UnityBones.Length; i++)
        {
            mBoneNames.Add(MountPoints.GetHumanBodyBoneName((HumanBodyBones)i));
        }

        mBoneNames.Add("Custom");
    }

    /// <summary>
    /// Given a bone name, return the associated index
    /// </summary>
    /// <param name="rBoneName">Name of the bone to find</param>
    /// <returns>Index of the bone that represents the HumanBoneID </returns>
    private int GetBoneIndex(string rBoneName)
    {
        for (int i = 0; i < mBoneNames.Count; i++)
        {
            if (mBoneNames[i] == rBoneName)
            {
                return i;
            }
        }

        return mBoneNames.Count - 1;
    }

    /// <summary>
    /// Determine if we're dealing with an actual prefab or an instance
    /// </summary>
    /// <param name="rMountPoints"></param>
    /// <returns></returns>
    private bool IsAddMountPointEnabled(MountPoints rMountPoints)
    {
        PrefabType lType = PrefabUtility.GetPrefabType(rMountPoints);
        return lType != PrefabType.Prefab;
    }
}
