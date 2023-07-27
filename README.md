# Rain

The code I've written is in:

- `include/rain/`
- `src/rain/`
- `src/csrain/`

## Building

> requirements: python3, ninja, clang 15, csc, mono2, glfw3

```bash
python3 gen.py
ninja
```

**NB:**
This runs arbitrary shell commands, so check the build.template.ninja file to ensure that there isn't any malicious commands being run. All that `gen.py` does is replace commands in backticks with the output of that command. (TODO: wording)

## Running

> requirements: mono2, opengl3.3

```bash
./main
```
