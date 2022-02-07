want_to_dash = False

def update(data, debug_print):
	global want_to_dash
	return_str = ""
	if data["player"]["numDashes"] > 0:
		return_str = "X"
		if want_to_dash:
			return_str = ""
			want_to_dash = False
		else:
			want_to_dash = True

	return return_str