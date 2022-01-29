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

base_path = os.getcwd() + "/Mods/ProgrammingPlaysCeleste/"

parser.read(base_path + "divisions.ini")

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

for i in range(len(divisions_arr)):
    item = divisions_arr[i]
    if len(item) == 2 and item[1] in parser["Script Selection"]:
        selected_script = parser["Script Selection"][item[1]]
        script_path = ""
        if selected_script == "Random":
            scripts = glob.glob(base_path + '/code/' + item[1] + '/*.py')
            if len(scripts) > 0:
                selected_script = scripts[random.randint(0, len(scripts) - 1)]
                script_path = selected_script
                selected_script = os.path.basename(selected_script)
        else:
            script_path = base_path + "/" + item[1] + "/" + selected_script
        if len(script_path) > 0:
            spec = importlib.util.spec_from_file_location(selected_script, script_path)
            module = importlib.util.module_from_spec(spec)
            sys.modules[selected_script] = module
            # Init code for all the modules (so if you have a script in one of the folders, this is where your code gets initialized):
            spec.loader.exec_module(module)

            inputs_allowed = []

            for allowed_input in item[0]:
                inputs_allowed.append(allowed.get(allowed_input))

            scripts_to_load.append({"module": module, "allowed_inputs": inputs_allowed})


# Three: Block stdout so other scripts can't call print. Only when WE want to. From https://stackoverflow.com/questions/8391411/how-to-block-calls-to-print
class HidePrints:
    def __enter__(self):
        self._original_stdout = sys.stdout
        sys.stdout = open(os.devnull, 'w')
        return self._original_stdout

    def __exit__(self, exc_type, exc_val, exc_tb):
        sys.stdout.close()
        sys.stdout = self._original_stdout


# Four: Create some debug printing options.
def debug_print(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)

with HidePrints() as to_print:
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

        print_string = ""

        for item in scripts_to_load:
            # This is where we call the output of every script that's currently loaded:
            string_to_add = item["module"].update(data, debug_print)
            allowed_chars = item["allowed_inputs"]
            for char in string_to_add:
                if char in allowed_chars:
                    print_string += char
        print_string += "--END OF INPUT STRING--"
        print(print_string, file=to_print)