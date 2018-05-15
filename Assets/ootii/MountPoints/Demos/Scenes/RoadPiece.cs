using System;
using UnityEngine;
using com.ootii.Actors;

namespace com.ootii.Demos
{
    public class RoadPiece : MonoBehaviour
    {
        protected float mSnapDistance = 0.25f;

        protected Camera mCamera = null;

        protected MountPoints mMountPoints = null;

        public void Start()
        {
            mCamera = Camera.main; 
            mMountPoints = gameObject.GetComponent<MountPoints>();
        }

        private void OnMouseDrag()
        {
            RaycastHit lHitInfo;
            Ray lRay = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (UnityEngine.Physics.Raycast(lRay, out lHitInfo, Mathf.Infinity, 1 << 4))
            {
                Vector3 lNewPosition = lHitInfo.point + (Vector3.up * 0.025f);
                gameObject.transform.position = lNewPosition;
            }
        }

        private void OnMouseUp()
        {
            // Grab the closest point that doesn't belong to this object
            MountPoint lClosestMountPoint = MountPoints.GetClosestMountPoint(mMountPoints);
            if (lClosestMountPoint != null)
            {
                // Grab the closest point from this object to the just-found point
                Vector3 lTestPosition = lClosestMountPoint._Anchor.transform.position;
                MountPoint lThisMountPoint = mMountPoints.GetClosestMountPoint(lTestPosition);
                if (lThisMountPoint != null)
                {
                    // Disconnect any existing parent
                    for (int i = 0; i < mMountPoints.Points.Count; i++)
                    {
                        mMountPoints.Points[i].ChildTo(null);
                    }

                    // Check if we're in snap distance
                    float lDistance = Vector3.Distance(lTestPosition, lThisMountPoint._Anchor.transform.position);
                    if (lDistance < mSnapDistance)
                    {
                        // Connect this new one
                        lThisMountPoint.ChildTo(lClosestMountPoint);
                    }
                }
            }
        }
    }
}
