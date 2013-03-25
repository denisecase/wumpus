using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WumpusAgentGame
{
    class Action
    {
        public enum Command
        {
            Left, Right, Up, Down, PickUp, ShootLeft, ShootRight, ShootUp, ShootDown, Climb, None
        }

        public Command CurrentCommand = Command.None;

        public Action(Command com)
        {
            CurrentCommand = com;
        }
        public Action()
        {
            CurrentCommand = Command.None;
        }

        public override bool Equals(object obj)
        {
            Action other = (Action)obj;
            if (other.CurrentCommand == CurrentCommand)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the index of a given Action.Command in the Action enumeration.
        /// </summary>
        /// <param name="cmd">Given Action.Command</param>
        /// <returns>index</returns>
        public static int GetIndex(Command cmd)
        {
            int iIndex = -1;
            Array strings = Enum.GetNames(typeof(Action.Command));
            for (int i = 0; i < strings.Length; i++)
            {
                if ((string)strings.GetValue(i) == (string)cmd.ToString()) iIndex = i;
            }
            return iIndex;
        }

        /// <summary>
        /// Get the Action.Command given an index of the Action enumeration.
        /// </summary>
        /// <param name="iAction">the index of the desired action command</param>
        /// <returns>Action.Command</returns>
        public static Action.Command GetAction(int iAction)  // sorry this is ugly... didn't want to mess with the enum since you were already using...
        {
            Action.Command a;
            switch (iAction)
            {
                case 0:
                    a = Command.Left;
                    break;
                case 1:
                    a = Command.Right;
                    break;
                case 2:
                    a = Command.Up;
                    break;
                case 3:
                    a = Command.Down;
                    break;
                case 4:
                    a = Command.PickUp;
                    break;
                case 5:
                    a = Command.ShootLeft;
                    break;
                case 6:
                    a = Command.ShootRight;
                    break;
                case 7:
                    a = Command.ShootUp;
                    break;
                case 8:
                    a = Command.ShootDown;
                    break;
                case 9:
                    a = Command.Climb;
                    break;
                default:
                    a = Command.None;
                    break;
            }
            return a;
        }

        public bool IsFiringAction()
        {
            if (CurrentCommand == Action.Command.ShootDown ||
                CurrentCommand == Action.Command.ShootUp ||
                CurrentCommand == Action.Command.ShootRight ||
                CurrentCommand == Action.Command.ShootLeft)
                return true;
            else
                return false;
        }
      
    }
}
