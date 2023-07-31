import re, os, subprocess, shlex

with (
	open('build.env', 'r' if os.path.exists('build.env') else 'w+') as env_file,
	open('build.ninja', 'w') as out_file,
	open('build.template.ninja', 'r') as inp_file,
):
	env = os.environ.copy()

	multiline_entry = False
	name = ""
	for line in env_file:
		if multiline_entry: # continue last entry
			env[name] = line.strip()
			multiline_entry = (env[name] != '' and env[name][-1] == '\\')
			if multiline_entry: env[name] = env[name][:-1] # remove the backslash
		name, value = map(lambda s: s.strip(), line.split('='))
		if value == '' and name in env: del env[name] # remove empty values
		value = ' '.join(shlex.split(value)) # allow quoting (and thus for empty strings)
		if value[-1] == '\\': # multiline entry
				value = value[:-1] # remove the backslash
				multiline_entry = True
		env[name] = value
	
	macros: dict[str, str] = {}

	def run_macros(src: str) -> str: # replace macros (recursively)
		def repl(m: re.Match[str]):
			name, *args = shlex.split(m.group(1)) # allow quoting
			cmd = macros[name].format(name, *args).strip() # replace args
			if cmd != '' and cmd[0] == cmd[-1] == '`': # is a shell command
				return run_cmd(cmd[1:-1])
			return run_macros(cmd)

		# @[name arg1 arg2 ...]
		return re.sub(
			r"(?<!\\)@\[([a-zA-Z_]+(\s+(\\\]|[^\]])+)*)\]",
			repl,
			src
		)

	def run_cmd(cmd: str) -> str: # run shell command and replace macros on input
		return subprocess.check_output(
			run_macros(cmd),
			shell = True,
			env = env
		).decode().strip()

	def meta_repl(m: re.Match[str]) -> str: # replace macro defs with an empty string
		macros[m.group(1)] = m.group(2).replace("\\n", "\n")
		return ""

	out_file.write(re.sub(
		r"`\s*((\\`|[^`])+)\s*`",
		lambda m: run_cmd(m.group(1)),
		run_macros(re.sub(
			# backslash before newline for multiline stuff
			# @name = code
			r"^\s*@([a-zA-Z_]+)\s*=\s*((\\\n|[^\n])*)\s*(\n|$)",
			meta_repl,
			inp_file.read(),
			flags = re.MULTILINE
		))
	))

