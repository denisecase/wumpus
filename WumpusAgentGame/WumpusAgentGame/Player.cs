using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using WumpusAgentGame.Agents;

namespace WumpusAgentGame
{
    class Player : Sprite
    {
        const string PLAYER_ASSETNAME = "Player";
        private int START_POSITION_X = 0;
        private int START_POSITION_Y = 400;
        const int PLAYER_SPEED = 160;
        const int MOVE_UP = -1;
        const int MOVE_DOWN = 1;
        const int MOVE_LEFT = -1;
        const int MOVE_RIGHT = 1;
        private int SCREEN;
        private int startY;

        //Current Positions and Destination Positions
        private int posX = 0;
        private int destPosX = 0;
        private int posY = 0;
        private int destPosY = 0;
        public bool transition = false;

        // added to allow for fast learning - DMC
        private int Player_Speed; 

        //Human or Agent
        bool isHuman = true;
        //DStarAgent AI;   // Removed specific agent declaration - DMC 
        Agent AI;  // generic declaration so we can make either a DFS or a QLearning agent - DMC
 
        //Log
        public List<string> playerLog;
        public bool listUpdated = false;
        public string playerLoc;

        //Win or Lose
        public bool goingToDie = false;
        public bool escaped = false;
        public bool picking_up = false;
        public bool firedArrow = false;
        public Action lastAction;

        //Score
        public int Score;
        private int arrows;

        //Player states
        enum State
        {
            Walking,Stopped,Dead,Escaped,Firing,Fired
        }
        //
        Texture2D _PlayerDead;
        Texture2D _PlayerFiring;
        Texture2D _PlayerWalk;
        Texture2D _PlayerStop;
        //Used in animating the walk
        double millisecondsPerFrame = 250; //Update every 2 seconds
        double nextUpdate = 0; //Accumulate the elapsed time
        bool switchTexture = true;

        //Current State: Stopped, not moving
        private State CurrentState = State.Stopped;
        Vector2 Direction = Vector2.Zero;
        Vector2 Speed = Vector2.Zero;

        public Player(int y,int _screen)
        {
            SCREEN = _screen;
            startY = y;
            posY = y;
            destPosY = y;
            //START_POSITION_Y = 100 * 4 + 45;
            playerLog = new List<string>();
            AI = new DFSAgent(playerLog);
            Score = ((y + 1) * (y + 1)) * 2;
            arrows = 1;

			
        }

        //Prepares the Player Sprite
        public void LoadContent(ContentManager theContentManager)
        {
            Position = new Vector2(START_POSITION_X, START_POSITION_Y);
            base.LoadContent(theContentManager, PLAYER_ASSETNAME);
            _PlayerStop = theContentManager.Load<Texture2D>(PLAYER_ASSETNAME);
            _PlayerDead = theContentManager.Load<Texture2D>("PlayerDead");
            _PlayerFiring = theContentManager.Load<Texture2D>("PlayerShoot");
            _PlayerWalk = theContentManager.Load<Texture2D>("PlayerWalk");
        }

        //Updates the Player
        public void Update(GameTime theGameTime, Tile CurrentTile)
        {
            Action newAction = new Action();
            KeyboardState aCurrentKeyboardState = new KeyboardState();
            //if (isHuman == true)
            //if(!Game.Instance.AutoPlay)  // human playable
            //{
            //Animates Walking
            if (CurrentState == State.Walking && (theGameTime.TotalGameTime.TotalMilliseconds > nextUpdate))
            {
                if (switchTexture == true)
                {
                    base.ChangeSprite(_PlayerWalk);
                    switchTexture = false;
                }
                else
                {
                    base.ChangeSprite(_PlayerStop);
                    switchTexture = true;
                }
                nextUpdate = theGameTime.TotalGameTime.TotalMilliseconds + millisecondsPerFrame;
            }

            if (CurrentState == State.Stopped && Game.Instance.AutoPlay)
            {
                newAction = AI.GetAction(CurrentTile);
                if (Game.Instance.QLearning)
                {
                    Game.Instance.LastAction = newAction;  // make the last action taken available publicly
                    if (newAction.IsFiringAction())
                    {
                        CurrentState = State.Firing;
                        playerLog.Add("Preparing to Fire ARROW");
                        listUpdated = true;
                    }
                }
                else
                {
                    if (newAction.IsFiringAction() && arrows > 0)
                    {
                        CurrentState = State.Firing;
                        playerLog.Add("Preparing to Fire ARROW");
                        listUpdated = true;
                    }
                }
            }
            else
            {
                aCurrentKeyboardState = Keyboard.GetState();
            }
			if (aCurrentKeyboardState.IsKeyDown(Keys.F) == true && CurrentState == State.Stopped && arrows > 0)
            {
				CurrentState = State.Firing;
				playerLog.Add("Preparing to Fire ARROW");
				listUpdated = true;
            }
            if (!Game.Instance.AutoPlay)
            {
                newAction = getInput(aCurrentKeyboardState);
            }
            if (newAction.CurrentCommand == Action.Command.Climb && escaped == false && (posX == 0 && posY == startY))
            {
				escaped = true;
				CurrentState = State.Escaped;
				playerLog.Add("Player Escaped! Final Score: " + Score + " Points");
				listUpdated = true;
                if (Game.Instance.QLearning)
                {
                    Game.Instance.EndGameX = posX;
                    Game.Instance.EndGameY = posY;
                    Game.Instance.EndGameCarryingGold = true;
                    Game.Instance.EndGameAction = Game.Instance.LastAction;
                    Game.Instance.EndGameRewardState = Game.RewardState.PlayerWon;
                    Game.Instance.CurrentRewardState = Game.RewardState.PlayerWon;
                }

            }
			else if (newAction.CurrentCommand == Action.Command.PickUp)
            {
				picking_up = true;
                if (Game.Instance.QLearning)
                {
                    if (!Game.Instance.PlayerCarryingGold)
                    {
                        Game.Instance.CurrentRewardState = Game.RewardState.PickedUpGold;
                    }
                    else Game.Instance.CurrentRewardState = Game.RewardState.TriedImpossibleAction;
                }
            }
            UpdateMovement(newAction);
            if (CurrentState == State.Firing) shootArrow(newAction);
            lastAction = newAction;
            base.Update(theGameTime, Speed, Direction);
        }

   
        //Updates the Movement
        public void UpdateMovement(Action curAction)
        {
            playerLoc = "Player location (" + Position.X + "," + Position.Y + ")";
           
            //If Moving, check to see if at destination
            if (CurrentState == State.Walking)
            {
                int zA = destPosX / 5;
                int zB = destPosY / 5;
                int newX = (destPosX - (5 * zA)) * 100;
                int newY = (destPosY - (5 * zB)) * 100;
                
                if (zA == posX / 5 && zB == posY / 5 || transition == true)
                {
                    if ((Direction.Y == MOVE_UP || Direction.X == MOVE_LEFT) &&
                        (this.Position.X) <= (newX) && (this.Position.Y) <= (newY))
                    {
                        Speed = Vector2.Zero;
                        Direction = Vector2.Zero;
                        Position.X = newX;
                        Position.Y = newY;
                        posX = destPosX;
                        posY = destPosY;
                        CurrentState = State.Stopped;
                        base.ChangeSprite(_PlayerStop);
                        transition = false;
                    }
                    else if ((Direction.Y == MOVE_DOWN || Direction.X == MOVE_RIGHT) &&
                        (this.Position.X) >= (newX) && (this.Position.Y) >= (newY))
                    {
                        Speed = Vector2.Zero;
                        Direction = Vector2.Zero;
                        Position.X = newX;
                        Position.Y = newY;
                        posX = destPosX;
                        posY = destPosY;
                        CurrentState = State.Stopped;
                        base.ChangeSprite(_PlayerStop);
                        transition = false;
                    }
                }
                else
                {
                    if ((Direction.Y == MOVE_UP) &&
                        (this.Position.Y) <= -40)
                    {
                        Position.Y = (SCREEN * 100-40);
                        transition = true;
                    }
                    else if ((Direction.Y == MOVE_DOWN) &&
                        (this.Position.Y) >= SCREEN*100-40)
                    {
						Position.Y = -40;
						transition = true;
                    }
                    else if ((Direction.X == MOVE_LEFT) &&
                        (this.Position.X) <= -40)
                    {
                        Position.X = (SCREEN * 100-40);
                        transition = true;
                    }
                    else if ((Direction.X == MOVE_RIGHT) &&
                        (this.Position.X) >= SCREEN*100-40)
                    {
                        Position.X = -40;
                        transition = true;
                    }
                }
            }
         
            if (CurrentState == State.Stopped)
            {
                var tile = Game.Instance.GetTile(posX, posY);
               //playerLog.Add("Stopped -  Getting tile information for posX, poxY = " + posX + "," + posY);
             
               if (curAction.CurrentCommand == Action.Command.Left)
                {
                    Speed.X = PLAYER_SPEED;
                    if (Game.Instance.QLearning) Speed.X = Player_Speed;  // modified for fast option - DMC
                    Direction.X = MOVE_LEFT;
                   if(tile._isLeftWall)
                    {
                        CurrentState = State.Walking;
                        listUpdated = true;
                        if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.TriedImpossibleAction;
                    }
					else
					{
						destPosX += MOVE_LEFT;
						CurrentState = State.Walking;
						playerLog.Add("Player moved LEFT");
						listUpdated = true;
						Score--;
                        if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.Playing;
                    }
                }
                else if (curAction.CurrentCommand == Action.Command.Right)
                {
                    Speed.X = PLAYER_SPEED;
                    if (Game.Instance.QLearning) Speed.X = Player_Speed;  // modified for fast option - DMC
                    Direction.X = MOVE_RIGHT;
                    if(tile._isRightWall)
					{
						CurrentState = State.Walking;
						listUpdated = true;
                        if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.TriedImpossibleAction;
                    }
					else
					{
						destPosX += MOVE_RIGHT;
						CurrentState = State.Walking;
						playerLog.Add("Player moved RIGHT");
						listUpdated = true;
						Score--;
                        if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.Playing;
                    }
                }
                else if (curAction.CurrentCommand == Action.Command.Up)
                {
                    Speed.Y = PLAYER_SPEED;
                    if (Game.Instance.QLearning) Speed.Y = Player_Speed;  // modified for fast option - DMC
                    Direction.Y = MOVE_UP;
                    if(tile._isTopWall)
					{
						CurrentState = State.Walking;
 						listUpdated = true;
                        if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.TriedImpossibleAction;
                    }
					else
					{
						destPosY += MOVE_UP;
						CurrentState = State.Walking;
						playerLog.Add("Player moved UP");
						listUpdated = true;
						Score--;
                        if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.Playing;
                    }
                }
                else if (curAction.CurrentCommand == Action.Command.Down)
                {
                    Speed.Y = PLAYER_SPEED;
                    if (Game.Instance.QLearning) Speed.Y = Player_Speed;  // modified for fast option - DMC
                    Direction.Y = MOVE_DOWN;
		            if(tile._isBottomWall)
					{
						CurrentState = State.Walking;
  						listUpdated = true;
                        if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.TriedImpossibleAction;
                    }
					else
					{
	                destPosY += MOVE_DOWN;
                    CurrentState = State.Walking;
                     listUpdated = true;
                    Score--;
                    if (Game.Instance.QLearning) Game.Instance.CurrentRewardState = Game.RewardState.Playing;
                    }
                }
            }
        }

        //Checks if the player is between 2 tiles
        public bool isBetweenTiles()
        {
            int zA = destPosX / 5;
            int zB = destPosY / 5;
            int newX = (destPosX - (5 * zA)) * 100;
            int newY = (destPosY - (5 * zB)) * 100;
            bool between = false;
            if ((Direction.X == MOVE_UP || Direction.Y == MOVE_LEFT) &&
                (this.Position.X) <= (newX+75) && (this.Position.Y) <= (newY+75))
            {
                between = true;
            }
            else if ((Direction.X == MOVE_DOWN || Direction.Y == MOVE_RIGHT) &&
                (this.Position.X) >= (newX-75) && (this.Position.Y) >= (newY-75))
            {
                between = true;
            }
            return between;
        }
        
        public void setPositionX(int x, GameTime gt)
        {
            posX = x;
        }
        public void setPositionY(int y, GameTime gt)
        {
            posY = y;
        }


        public int PosX
        {
            get { return posX; }
        }
        public int PosY
        {
            get { return posY; }
        }
        public int DestinationX
        {
            get { return destPosX; }
        }
        public int DestinationY
        {
            get { return destPosY; }
        }

        //Quick Death method, needs improvement
        public void dead()
        {
            CurrentState = State.Dead;
            base.ChangeSprite(_PlayerDead);
            Speed = new Vector2(0, 0);
            Score = 0;
        }

        //Gets the Human Players Input
        private Action getInput(KeyboardState aCurrentKeyboardState)
        {
            Action currentAction = new Action();
            if (CurrentState == State.Firing && aCurrentKeyboardState.IsKeyDown(Keys.Left) == true) currentAction.CurrentCommand = Action.Command.ShootLeft;
            else if (CurrentState == State.Firing == true && aCurrentKeyboardState.IsKeyDown(Keys.Right) == true) currentAction.CurrentCommand = Action.Command.ShootRight;
            else if (CurrentState == State.Firing == true && aCurrentKeyboardState.IsKeyDown(Keys.Up) == true) currentAction.CurrentCommand = Action.Command.ShootUp;
            else if (CurrentState == State.Firing == true && aCurrentKeyboardState.IsKeyDown(Keys.Down) == true) currentAction.CurrentCommand = Action.Command.ShootDown;
            else if (aCurrentKeyboardState.IsKeyDown(Keys.F) == false && aCurrentKeyboardState.IsKeyDown(Keys.Left) == true) currentAction.CurrentCommand = Action.Command.Left;
            else if (aCurrentKeyboardState.IsKeyDown(Keys.F) == false && aCurrentKeyboardState.IsKeyDown(Keys.Right) == true) currentAction.CurrentCommand = Action.Command.Right;//fix this tommorow
            else if (aCurrentKeyboardState.IsKeyDown(Keys.F) == false && aCurrentKeyboardState.IsKeyDown(Keys.Up) == true) currentAction.CurrentCommand = Action.Command.Up;
            else if (aCurrentKeyboardState.IsKeyDown(Keys.F) == false && aCurrentKeyboardState.IsKeyDown(Keys.Down) == true) currentAction.CurrentCommand = Action.Command.Down;
            else if (aCurrentKeyboardState.IsKeyDown(Keys.F) == false && aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && CurrentState != State.Walking)
            {
                if (posX == 0 && posY == startY) currentAction.CurrentCommand = Action.Command.Climb;
                else currentAction.CurrentCommand = Action.Command.PickUp;
            }
            return currentAction;
        }

        private void shootArrow(Action act)
        {
            base.ChangeSprite(_PlayerFiring);
            if (act.CurrentCommand == Action.Command.ShootLeft)
            {
                playerLog.Add("Player Shot Arrow LEFT");
                arrows--;
                listUpdated = true;
                CurrentState = State.Fired;
                firedArrow = true;
                base.ChangeSprite(_PlayerStop);
            }
            else if (act.CurrentCommand == Action.Command.ShootRight)
            {
                playerLog.Add("Player Shot Arrow RIGHT");
                arrows--;
                listUpdated = true;
                CurrentState = State.Fired;
                firedArrow = true;
                base.ChangeSprite(_PlayerStop);
            }
            else if (act.CurrentCommand == Action.Command.ShootUp)
            {
                playerLog.Add("Player Shot Arrow UP");
                arrows--;
                listUpdated = true;
                CurrentState = State.Fired;
                firedArrow = true;
                base.ChangeSprite(_PlayerStop);
            }
            else if (act.CurrentCommand == Action.Command.ShootDown)
            {
                playerLog.Add("Player Shot Arrow DOWN");
                arrows--;
                listUpdated = true;
                CurrentState = State.Fired;
                firedArrow = true;
                base.ChangeSprite(_PlayerStop);
            }
        }

        public void ArrowReset()
        {
            firedArrow = false;
            CurrentState = State.Stopped;
            System.Threading.Thread.Sleep(100);
        }
		public void CallGameUpdate(GameTime theGameTime)
		{
			base.Update(theGameTime, Speed, Direction);
		}


        // DMC added below..................

        /// <summary>
        /// New constructor with a fast player speed for quicker learning trials.
        /// </summary>
        /// <param name="y">player starting y position</param>
        /// <param name="_screen">_screen</param>
        /// <param name="fastSpeed">faster speed</param>
        public Player(int y, int _screen, int fastSpeed)
        {
            SCREEN = _screen;
            startY = y;
            posY = y;
            destPosY = y;
            //START_POSITION_Y = 100 * 4 + 45;
            if (playerLog == null) { playerLog = new List<string>(); }
            AI = new QLearningAgent();
            Score = ((y + 1) * (y + 1)) * 2;
            arrows = 1;
            try
            {
                if (fastSpeed > 0) Player_Speed = fastSpeed;
            }
            catch
            {
                Player_Speed = PLAYER_SPEED;
            }
        }

        /// <summary>
        /// Factory method to make a special type of agent.
        /// </summary>
        /// <returns>Agent</returns>
        private Agent MakeNewAgent()
        {
            if (Game.Instance.CurrentAgentType == Game.AgentType.QLearningAgent)
            {
                return new QLearningAgent();
            }
            return new DFSAgent(new List<string>());
        }
      
 
    }
}
