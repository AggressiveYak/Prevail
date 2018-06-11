using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTrees
{
    public class Selector : Node
    {
        /// <summary>
        /// The child nodes for this selector.
        /// </summary>
        protected List<Node> m_nodes = new List<Node>();
        
        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="nodes"></param>
        public Selector(List<Node> nodes)
        {
            m_nodes = nodes;
        }

        /// <summary>
        /// If any of the children reports a success,
        /// the selector will immediately report a success upwards.
        /// If all children fail, it will report a failure instead.
        /// </summary>
        /// <returns></returns>
        public override NodeStates Evaluate()
        {
            foreach (Node node in m_nodes)
            {
                switch (node.Evaluate())
                {
                    case NodeStates.FAILURE:
                        continue;
                    case NodeStates.SUCCESS:
                        m_nodeState = NodeStates.SUCCESS;
                        return m_nodeState;
                    case NodeStates.RUNNING:
                        m_nodeState = NodeStates.RUNNING;
                        return m_nodeState;
                    default:
                        continue;
                }
            }
            m_nodeState = NodeStates.FAILURE;
            return m_nodeState;
        }
    }
}


