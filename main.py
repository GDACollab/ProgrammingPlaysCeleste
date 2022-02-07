import importlib.util
import glob
import configparser
import random
import sys
import os
import json

# We need to do a few things.

# One: Load in the configuration settings to get the allowed inputs and division of scripts into files.

parser = configparser.ConfigParser()

base_path = os.getcwd()

if "\\Mods\\ProgrammingPlaysCeleste" not in base_path:
    base_path += "\\Mods\\ProgrammingPlaysCeleste"

parser.read(base_path + "\\divisions.ini")

divisions = parser["Input Divisions"]

i = 0

divisions_arr = []

for division in divisions:
    if ("name" in division):
        divisions_arr[i].append(divisions[division])
        i += 1
    else:
        divisions_arr.append([])
        divisions_arr[i].append(divisions[division].split(","))

# Two: Load in the relevant scripts based on folder names.
# Because we want for there to be multiple scripts allowed in each folder, we have additional config options.

scripts_to_load = []

allowed = {
    "Left": "L",
    "Right": "R",
    "Up": "U",
    "Down": "D",
    "Jump": "J",
    "Climb": "C",
    "Dash": "X"
}

# I should really organize this into some proper functions.

def get_combo(combo_path):
    global scripts_to_load

    f = open(combo_path, "r")
    combos = f.readlines()
    combo_to_remove = random.randint(0, len(combos) - 1)
    scripts_to_load = combos[combo_to_remove].split(",")
    f.close()
    # So we can restart the process once we've gone through all combos:
    if len(combos) == 1:
        os.remove(combo_path)
    curr_line = 0

    # Go through the file, write all files except for the combination we picked.
    with open(combo_path, "w") as fw:
        for line in combos:
            if  curr_line != combo_to_remove:
                fw.write(line)
            curr_line += 1
        
def write_all_combos(combo_path):
    global divisions_arr
    total_scripts = []
    total_scripts_i = []
    for i in range(len(divisions_arr)): # First, go through all items.
        item = divisions_arr[i] 
        scripts = glob.glob(base_path + '\\code\\' + item[1] + '\\*.py')
        total_scripts.append(scripts)
        total_scripts_i.append(0)
    # Alright, here's how this works. Let's say we have 3 divisions, and each one has a different number of scripts.
    # So division A has 5 scripts, division B 2 scripts, division C 3 scripts.
    # That's 5 * 2 * 3 = 30 total combinations.
    # To make things simpler on ourselves, we can treat this like a number system.
    # So we create an array, total_scripts = [A scripts, B scripts, C scripts].
    # And then we iterate by "adding" from one script to the next:
    # [A1, B1, C1] Add to last index
    # [A1, B1, C2] Add to last index
    # [A1, B1, C3] Add to last index
    # [A1, B2, C1] Add to last index, add to second index
    # This should give us all the possible combinations of the scripts without repetition.

    f = open(combo_path, "w")
    while True:

        # Write all current possibilities:
        for i in range(len(total_scripts)):
            to_write = total_scripts[i][total_scripts_i[i]]
            if i != len(total_scripts) - 1:
                to_write += ","
            f.write(to_write)
        f.write("\n")

        should_break = True
        for i in range(len(total_scripts_i)):
            if total_scripts_i[i] < len(total_scripts[i]) -1:
                should_break = False
        if should_break:
            break
        
        curr_digit = 0

        # Then increment:
        while True:
            if curr_digit >= len(total_scripts):
                break

            total_scripts_i[curr_digit] += 1
            if total_scripts_i[curr_digit] >= len(total_scripts[curr_digit]):
                if curr_digit >= len(total_scripts) - 1:
                    break
                total_scripts_i[curr_digit] = 0
                curr_digit += 1
            else:
                break
    f.close()
    get_combo(combo_path)

# If we're supposed to go through all combinations of scripts:
if parser["Script Selection"]["get-all-script-combos"].lower() == "true":
    combo_path = base_path + "\\possiblecombinations.txt"

    # If the file exists, we can read through all the existing combos and pick one at random.
    if os.path.exists(combo_path):
        get_combo(combo_path)
    else: # If not, we just need to write all possible combinations.
        write_all_combos(combo_path)
else:
    # Otherwise, just get the paths either randomly (from python), or from the specific filename.
    for i in range(len(divisions_arr)):
        item = divisions_arr[i]
        if len(item) == 2 and item[1] in parser["Script Selection"]:
            selected_script = parser["Script Selection"][item[1]]
            script_path = ""
            if selected_script == "Random":
                scripts = glob.glob(base_path + '\\code\\' + item[1] + '\\*.py')
                if len(scripts) > 0:
                    selected_script = scripts[random.randint(0, len(scripts) - 1)]
                    script_path = selected_script
            else:
                script_path = base_path + "\\" + item[1] + "\\" + selected_script
            if len(script_path) > 0:
                scripts_to_load.append(script_path)

scripts = []
for i in range(len(scripts_to_load)):
    script = scripts_to_load[i].replace("\n", "")
    spec = importlib.util.spec_from_file_location(os.path.basename(script), script)
    module = importlib.util.module_from_spec(spec)
    sys.modules[os.path.basename(script)] = module
    # Init code for all the modules (so if you have a script in one of the folders, this is where your code gets initialized):
    spec.loader.exec_module(module)

    inputs_allowed = []

    for item in divisions_arr[i][0]:
        inputs_allowed.append(allowed.get(item))

    scripts.append({"module": module, "allowed_inputs": inputs_allowed})

# Three: Create some debug printing options. YOU SHOULD NOT BE USING PRINT(). Just going to have to hope that people don't use print(), because I can't find a way to override it
# in a way that also allows the mod to intercept stdout.
def debug_print(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)

# Hack-y way around this. Not sure how to communicate directly from C# to python, so we'll just use console input.
while (1):
    json_input = input()
    # So we can close the relevant stuff when we're done:
    if json_input == "FINISHED":
        break

    try:
        data = json.loads(json_input)
    except:
        debug_print("ERROR: DATA SUPPLIED IS NOT FORMATTED AS A JSON.")

    print_string = "--START OF INPUT STRING--"

    for item in scripts:
        # This is where we call the output of every script that's currently loaded:
        string_to_add = item["module"].update(data, debug_print)
        allowed_chars = item["allowed_inputs"]
        for char in string_to_add:
            if char in allowed_chars:
                print_string += char
    print_string += "--END OF INPUT STRING--"
    print(print_string)