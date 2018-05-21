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
[CustomEditor(typeof(Mount))]
public class MountEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // Object we're editing
    private Mount mTarget;
    private SerializedObject mTargetSO;

    // Active mount points
    private MountPoint mChildSnapPoint = null;
    private MountPoint mParentSnapPoint = null;

    // Tracks all the mount points in the scene (other than from this object)
    private List<MountPoint> mScenePoints = new List<MountPoint>();
    private List<Vector3> mScenePointPositions = new List<Vector3>();

    // Lookup for bone names
    private List<string> mBoneNames = new List<string>();

    /// <summary>
    /// Called when the script object is loaded
    /// </summary>
    void OnEnable()
    {
        LoadBoneNames();

        // Grab the serialized objects
        mTarget = (Mount)target;
        mTargetSO = new SerializedObject(target);

        // Ensure we always have a mount point
        if (mTarget.Point == null || mTarget.Point.Anchor == null)
        {
            mIsDirty = true;
            mTarget.CreateMountPoint("Mount Point", "", true);
        }

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

        // If the transform has changed, we need to reset the scale
        if (mTarget.transform.hasChanged)
        {
            mTarget.Point.OriginalScale = mTarget.transform.lossyScale;
            mTarget.transform.hasChanged = false;
        }

        // Clean up the mount points to ensure everything stays in sync
        PreProcessPoints();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Mount");

        EditorHelper.DrawInspectorDescription("Manages a single mount point for simple items. This mount cannot be a parent to other mount points.", MessageType.None);

        GUILayout.Space(5);

        if (!IsAddMountPointEnabled(mTarget))
        {
            GUILayout.Space(5);

            EditorGUILayout.HelpBox("Unity prevents mount points from being parented directly on the prefab. Instead, add them to a prefab instance and then press 'apply' to update the prefab.", MessageType.Warning);
        }

        // Show the points
        EditorGUILayout.BeginVertical(EditorHelper.Box);

        bool lItemIsDirty = DrawPointDetailItem(mTarget.Point);
        if (lItemIsDirty) { mIsDirty = true; }

        EditorGUILayout.EndVertical();

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
    /// Renders the properties of the motion so they can be changed here
    /// </summary>
    /// <param name="rLayerIndex">Layer the motion belongs to</param>
    /// <param name="rMotionIndex">Motions whose properites are to be listed</param>
    private bool DrawPointDetailItem(MountPoint rPoint)
    {
        bool lIsDirty = false;

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

        if (rPoint.Anchor != null)
        {
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

            GUILayout.Space(5);
        }

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

        if (mTarget.Point.Anchor != null)
        {
            // Highlight all the mount points on the object
            EditorHelper.DrawCircle(mTarget.Point.Anchor.transform.position, 0.01f, lSelectedMountColor);
            EditorHelper.DrawCircle(mTarget.Point.Anchor.transform.position, 0.05f, lSelectedOuterMountColor);
            EditorHelper.DrawText(mTarget.Point.Name, mTarget.Point.Anchor.transform.position + (Vector3.up * 0.075f), lSelectedMountColor);

            // Allow the selected mount point to be moved
            if (mTarget.Point != null && mTarget.Point.Anchor != null)
            {
                Vector3 lNewPosition = mTarget.Point.Anchor.transform.position;

                if (mTarget.Point.IsLocked)
                {
                    Handles.color = Color.blue;
                    Handles.DrawLine(mTarget.Point.Anchor.transform.position, mTarget.Point.Anchor.transform.position + (mTarget.Point.Anchor.transform.forward * 0.2f));

                    Handles.color = Color.red;
                    Handles.DrawLine(mTarget.Point.Anchor.transform.position, mTarget.Point.Anchor.transform.position + (mTarget.Point.Anchor.transform.right * 0.2f));

                    Handles.color = Color.green;
                    Handles.DrawLine(mTarget.Point.Anchor.transform.position, mTarget.Point.Anchor.transform.position + (mTarget.Point.Anchor.transform.up * 0.2f));
                }
                else
                {
                    lNewPosition = Handles.PositionHandle(mTarget.Point.Anchor.transform.position, mTarget.Point.Anchor.transform.rotation);

                    LoadOtherMountPoints();
                }

                // Update any data
                if (GUI.changed)
                {
                    bool lIsDirty = false;
                    lIsDirty = (lIsDirty || (mTarget.Point.Anchor.transform.position != lNewPosition));

                    if (lIsDirty)
                    {
                        mIsDirty = true;

                        mTarget.Point.Anchor.transform.position = lNewPosition;
                    }
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
        MountPoint lPoint = mTarget.Point;

        // Ensure the mount point is still valid
        if (lPoint.Owner == null)
        {
            lPoint.ChildTo(null);
            lPoint.Owner = mTarget.gameObject;
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
        for (int i = 0; i < mScenePointPositions.Count; i++)
        {
            float lDistance = Vector3.Distance(mTarget.Point.Anchor.transform.position, mScenePointPositions[i]);
            if (lDistance < MountPoints.EditorSnapDistance && lDistance < lMinDistance)
            {
                lMinDistance = lDistance;
                lChildSnapPoint = mTarget.Point;
                lParentSnapPoint = mScenePoints[i];
            }
        }

        // Test if we need to invert the parenting
        if (lChildSnapPoint != null && lParentSnapPoint != null)
        {
            // Flip the snap points if we have a selected point
            if (mTarget.Point != null && mTarget.Point == lParentSnapPoint)
            {
                lParentSnapPoint = lChildSnapPoint;
                lChildSnapPoint = mTarget.Point;
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
        MountPoint lMountPoint = mTarget.Point;
        if (lMountPoint.ParentMountPoint != null)
        {
            float lDistance = Vector3.Distance(lMountPoint.Anchor.transform.position, lMountPoint.ParentMountPoint.Anchor.transform.position);
            if (lDistance > MountPoints.EditorSnapDistance)
            {
                DisconnectMountPoints(lMountPoint, lMountPoint.ParentMountPoint);
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
        SerializedProperty lPointItemSP = mTargetSO.FindProperty("MountPoint");
        if (lPointItemSP != null)
        {
            return lPointItemSP;
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
    /// <returns>Index of the bone that represents the HumanBoneID</returns>
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
    private bool IsAddMountPointEnabled(Mount rMount)
    {
        PrefabType lType = PrefabUtility.GetPrefabType(rMount);
        return lType != PrefabType.Prefab;
    }
}
