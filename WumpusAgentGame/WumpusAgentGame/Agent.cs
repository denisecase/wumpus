using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Timers;
using Microsoft.Xna.Framework;

namespace WumpusAgentGame
{
    /// <summary>
    /// Abstract Agent class from which many types of agents can be made.
    /// </summary>
    abstract class Agent
    {
        /// <summary>
        /// Abstract method to retrieve an action from the agent.
        /// </summary>
        /// <param name="CurrentTile">Current tile location</param>
        /// <returns>Agent-selected action</returns>
        public abstract Action GetAction(Tile CurrentTile);

    }
}
