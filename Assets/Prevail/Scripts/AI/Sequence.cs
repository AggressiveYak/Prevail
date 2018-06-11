using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTrees
{
    public class Sequence : Node
    {
        /// <summary>
        /// Children nodes that belong to this sequence
        /// </summary>
        private List<Node> m_nodes = new List<Node>();
        
        public Sequence(List<Node> nodes)
        {
            m_nodes = nodes;
        }

        /// <summary>
        /// If any child node returns a failure, the entire node fails.
        /// When all nodes return a success, the node reports a success.
        /// </summary>
        /// <returns></returns>
        public override NodeStates Evaluate()
        {
            bool anyChildRunning = false;

            foreach(Node node in m_nodes)
            {
                switch (node.Evaluate())
                {
                    case NodeStates.FAILURE:
                        m_nodeState = NodeStates.FAILURE;
                        return m_nodeState;
                    case NodeStates.SUCCESS:
                        continue;
                    case NodeStates.RUNNING:
                        anyChildRunning = true;
                        continue;
                    default:
                        m_nodeState = NodeStates.SUCCESS;
                        break;
                }
            }
            m_nodeState = anyChildRunning ? NodeStates.RUNNING : NodeStates.SUCCESS;
            return m_nodeState;
        }
    }
}