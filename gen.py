open('build.ninja', 'w').write(__import__(
're').sub(r"`([^`]*)`",lambda m: __import__(
'subprocess').getoutput(m.group(1)).strip(),
open('build.template.ninja', 'r').read()))

