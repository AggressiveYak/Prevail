using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTrees
{
    public class ActionNode : Node
    {
        /// <summary>
        /// Method signature for the action.
        /// </summary>
        /// <returns></returns>
        public delegate NodeStates ActionNodeDelegate();

        /// <summary>
        /// The delegate that is called to evaluate this node.
        /// </summary>
        private ActionNodeDelegate m_action;

        /// <summary>
        /// Because this node contains no logic itself,
        /// the logic must be passed in the form of a
        /// delegate. As the signature states, the action
        /// needs to return a NodeStates enum.
        /// </summary>
        /// <param name="action"></param>
        public ActionNode(ActionNodeDelegate action)
        {
            m_action = action;
        }

        /// <summary>
        /// Evaluates the node using the passed in delegate and
        /// reports the resulting state as appropriate.
        /// </summary>
        /// <returns></returns>
        public override NodeStates Evaluate()
        {
            switch (m_action())
            {
                case NodeStates.SUCCESS:
                    m_nodeState = NodeStates.SUCCESS;
                    return m_nodeState;
                case NodeStates.FAILURE:
                    m_nodeState = NodeStates.FAILURE;
                    return m_nodeState;
                case NodeStates.RUNNING:
                    m_nodeState = NodeStates.RUNNING;
                    return m_nodeState;
                default:
                    m_nodeState = NodeStates.FAILURE;
                    return m_nodeState;
            }
        }
    }
}

