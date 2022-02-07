# Programming Plays Celeste
Control Celeste WITH THE POWER OF YOUR MIND.

Or a close alternative. This was inspired by watching that [amazing Celeste TAS run a few years back](https://www.youtube.com/watch?v=BEcv7BD1q9o). In that spirit, this repository has been HEAVILY based on the CelesteTAS-EverestInterop repository: https://github.com/EverestAPI/CelesteTAS-EverestInterop

There are three major differences though:

1. Gamepad input is controlled directly by a python script that [reads information supplied by the mod](https://github.com/GDACollab/ProgrammingPlaysCeleste/wiki/Data-Reference).

2. There are multiple scripts controlling the player at any one time. One might be in charge of just deciding when to jump. Another might just pick whether Madeline goes left or right.

3. The mod allows for random combination of scripts. The idea is to see which random combinations perform the best. 

Right now, this is only designed to work with the Forsaken City level, but you're welcome to try it on other levels. The process for getting started is somewhat in-depth, so please check out [the wiki](https://github.com/GDACollab/ProgrammingPlaysCeleste/wiki) for detailed guides on usage.
