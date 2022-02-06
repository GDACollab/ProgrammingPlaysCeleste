import matplotlib.pyplot as plt


# There's technically no init function, so we can make some global variables here:
level_name = "to show that we have an initial value"

# We can access these global variables from the 

def new_level(data, debug_print):
    # You might want to create some kind of global dictionary to store the current level.
    debug_print(level_name)

    # Visualizing the current level/screen as a scatter plot (assuming you have the matplotlib library):
    solids = data["solids"]
    
    x = []
    y = []
    for coords in solids:
        x.append(coords[0])
        y.append(-coords[1])
    # We can also add the goal, just so we can see where we need to go:
    x.append(data["goal"][0])
    y.append(-data["goal"][1])
    plt.scatter(x,y)
    plt.plot()
    plt.savefig("level.png")
    plt.clf()


def update(data, debug_print):
    # BASIC INFORMATION
    # Print the data we're given. DON'T EVER USE PRINT().
    debug_print(data)
    # If you want some delay to read console information:
    #time.sleep(0.01)

    # We access our global variables from the scope:
    global level_name

    if data["levelName"] != level_name:
        debug_print(level_name)
        debug_print("Wow! New level!")
        level_name = data["levelName"]
        new_level(data, debug_print)
    
    # You'll notice adding R (right) to the inputs will cause the player to move right.
    # Adding U (Up) to the inputs however, results in no changes, because divisions.ini doesn't allow code in the head folder to use up. 
    return_str = "RU"
    if data["player"]["onGround"] or data["player"]["jumpTimer"] > 0:
        return_str += "J"
    # Continually go to the right (and jump if we can):
    return return_str