using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTrees
{
    public abstract class Node
    {
        /// <summary>
        /// Delegate that returns the state of the node
        /// </summary>
        /// <returns></returns>
        public delegate NodeStates NodeReturn();

        /// <summary>
        /// The current state of the node
        /// </summary>
        protected NodeStates m_nodeState;

        public NodeStates nodeState
        {
            get
            {
                return m_nodeState;
            }
        }

        /// <summary>
        /// The default constructor for the Node
        /// </summary>
        public Node() {}

        /// <summary>
        /// Implementing classes use this method to evaluate the desired set of conditions
        /// </summary>
        /// <returns></returns>
        public abstract NodeStates Evaluate();
    }
}


