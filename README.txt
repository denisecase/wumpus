===============================================================================
      README.TXT
===============================================================================

The Wumpus Agent Game is an XNA version of the Hunt the Wumpus game 
developed for our CIS 730 - Artificial Intelligence class project 
at Kansas State University.

It was created by:

	Joel Livergood - Joel.Livergood@gmail.com
	Denise Case - casedm@ksu.edu

===============================================================================
      WUMPUS AGENT GAME 
===============================================================================

The default setting is single-player.  You're the agent and the goal is to find 
gold, grab gold, get back to the starting tile, and escape safely. 

Be careful!  There are deadly Wumpi - you can tell them by their green stench. 
There are also deadly bottomless pits (which you can detect by their telltale 
white breezes). 

===============================================================================
      QUICK START GUIDE 
===============================================================================

Double-click on the WumpusAgentGame.sln to open the solution in Visual Studio.

From the Solution Explorer window, right-click on the bold "WumpusAgentGame"
project and from the context menu, click "Debug / Start New Instance".
Click the "Play" button in the lower right. 

Use the arrow keys to move up, down, right, or left and find the gold.
Use the spacebar to pick up the gold. 
Use the arrow keys to return to the starting tile and the spacebar to climb 
out to safety.
Close the agent game when done (it's a one shot game right now).

To try the AI agent, check the "Auto Play" option before clicking "Play".
The AI agent will look for gold using a depth first search.

To try the Q-Learning agent, click the "With Q Learning" option before clicking
"Play". The Q-Learning Agent should play at least 20 fast random trials 
(Q-Learning kills a lot of agents), before using its machine learning algorithm to 
attempt a smart round. 

===============================================================================
      ABOUT THIS PROJECT 
===============================================================================

The project was a great way to explore some intelligent agent techniques.

Joel built the game framework and provided all of the art work. He made a 3D
version of the Wumpus game for his senior project:
http://people.cis.ksu.edu/~joellive/projects/projects-wumpus-3d.html.

===============================================================================
      DEVELOPMENT TOOLS
===============================================================================

Microsoft Visual Studio 
	http://www.microsoft.com/visualstudio

XNA Game Studio
        http://msdn.microsoft.com/en-us/centrum-xna.aspx

DreamSpark - Free Software for Students
        http://www.dreamspark.com/

===============================================================================
   
===============================================================================
