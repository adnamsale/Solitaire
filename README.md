# Solitaire

This is a simple solver for some of the Solitaire games from the excellent [cardgames.io](https://cardgames.io). 
I wasn't winning Scorpion very often and needed a fast solver to find out how winnable the game actually is.
It seems like a game that lends itself well to solving since the available moves are tightly constrained, keeping the branching
factor low. 

Spoiler: with the constraint that you can only deal from the tail when there are no other moves available, solving the first 20000 games
suggests that around 9% of games are winnable.

Despite the description saying that Canfield is difficult, most games seem to be winnable. Early results suggest around 82%.
