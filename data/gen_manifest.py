import json, os, os.path, dataclasses

THIS_DIR = os.path.dirname(os.path.realpath(__file__))
PROJECT_DIR = os.path.dirname(THIS_DIR)

ASSET_TEXTURE = 0
TEXTURE_EXTS = ['png', 'jpg', 'jpeg', 'gif']
IGNORE_EXTS = ['json', 'ini', 'py'] # TODO

@dataclasses.dataclass
class Asset:
	id: int
	type: int
	name: str

@dataclasses.dataclass
class TextureAsset(Asset):
	format: int
	path: str

# TODO: Take into account old IDs!

def gen_manifest(path: str, idbase: int) -> list[Asset]:
	items: list[Asset] = []
	for (dirpath, _, filenames) in os.walk(path):
		for filename in filenames:
			filepath = os.path.relpath(
				os.path.join(dirpath, filename),
				PROJECT_DIR
			)
			if filepath == os.path.realpath(__file__): continue

			ext = filename[filename.rfind(os.path.extsep)+1:].lower()

			if ext in TEXTURE_EXTS:
				items.append(TextureAsset(
					idbase + len(items), ASSET_TEXTURE,
					os.path.relpath(filepath, THIS_DIR),
					format=4, path=filepath
				))
			elif ext not in IGNORE_EXTS:
				print(f"unknown file extension: '{ext}'. file: {filepath}")

	return items

items = list(map(
	dataclasses.asdict,
	gen_manifest(os.path.dirname(os.path.realpath(__file__)), 1)
))

with open(os.path.join(THIS_DIR, 'manifest.json'), 'w') as fout:
	json.dump(items, fout, indent = '\t')
