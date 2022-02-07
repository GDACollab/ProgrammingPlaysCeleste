import matplotlib.pyplot as plt
import time


# There's technically no init function, so we can make some global variables here:
level_name = "to show that we have an initial value"

# We can access these global variables from the 

def new_level(data, debug_print):
    # You might want to create some kind of global dictionary to store the current level.
    debug_print(level_name)

    # Visualizing the current level/screen as a scatter plot (assuming you have the matplotlib library):
    solids = data["levelData"]["solids"]
    
    x = []
    y = []
    for coords in solids:
        x.append(coords[0])
        y.append(-coords[1])
    # We can also add the goal, just so we can see where we need to go:
    x.append(data["levelData"]["goal"][0])
    y.append(-data["levelData"]["goal"][1])
    plt.scatter(x,y)
    plt.plot()
    # This is saved into your Celeste Directory/level.png
    plt.savefig("level.png")
    plt.clf()

want_to_jump = False

def update(data, debug_print):
    # BASIC INFORMATION
    # Print the data we're given. DON'T EVER USE PRINT().
    # We can see a copy of the statements from debug_print in your Celeste Directory/code_log.txt
    #debug_print(data)
    # If you want some delay to read console information:
    #time.sleep(0.01)

    # We access our global variables from the scope:
    global level_name
    global want_to_jump

    if data["levelData"]["name"] != level_name:
        debug_print(level_name)
        debug_print("Wow! New level!")
        level_name = data["levelData"]["name"]
        new_level(data, debug_print)
    
    # You'll notice adding R (right) to the inputs will cause the player to move right.
    # Adding U (Up) to the inputs however, results in no changes, because divisions.ini doesn't allow code in the head folder to use up. 
    return_str = "RU"
    debug_print(data["player"]["isJumping"])

    # We need to reset jumping every time we die (try holding down the jump button in celeste, dying, and then keep holding jump. Nothing will happen.)
    # Time resets after death, and so we wait to apply inputs until after we've respawned.
    if data["player"]["currentState"] != "Player.StIntroRespawn" and data["player"]["onGround"] or data["player"]["jumpTimer"] > 0:
        return_str += "J"
        # If we wanted to jump from the last frame, but that's not happening because there is no jump timer (because the player just died or something), we need to temporarily release the jump button:
        if want_to_jump and data["player"]["jumpTimer"] <= 0:
            return_str = "RU"
            want_to_jump = False
        else:
            want_to_jump = True
    debug_print(return_str)
    # Continually go to the right (and jump if we can):
    return return_str