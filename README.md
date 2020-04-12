# Solitaire

This is a simple solver for Scorpion Solitaire from the excellent [cardgames.io](https://cardgames.io/scorpionsolitaire). 
I wasn't winning very often and needed a fast solver to find out how winnable the game actually is.
It seems like a game that lends itself well to solving since the available moves are tightly constrained, keeping the branching
factor low. 

Spoiler: with the constraint that you can only deal from the tail when there are no other moves available, solving the first 20000 games
suggests that around 9% of games are winnable.
