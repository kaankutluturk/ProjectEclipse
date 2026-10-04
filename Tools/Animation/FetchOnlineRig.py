"""Explicitly download a pinned CC-BY Cesium Man acceptance asset into a fresh directory.

No third-party model is vendored in Eclipse. Invoke this only for online import tests.
"""
import argparse
import hashlib
import json
from pathlib import Path
from urllib.request import Request, urlopen

REVISION = 'edc7c9e67c639d230715049ee31f9a96a6babbbe'
SHA256 = 'b7001eaeea8254bd44773bcd247e78696d94169388fbb2a1800fc69434e777d9'
SOURCE = 'https://github.com/KhronosGroup/glTF-Sample-Assets/tree/' + REVISION + '/Models/CesiumMan'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=False)
    base = 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/' + REVISION + '/Models/CesiumMan/'
    for remote, local in (('glTF-Binary/CesiumMan.glb', 'CesiumMan.glb'), ('LICENSE.md', 'LICENSE.md'),
                          ('README.md', 'SOURCE_README.md'), ('metadata.json', 'metadata.json')):
        with urlopen(Request(base + remote, headers={'User-Agent': 'Eclipse-Rig-Acceptance'}), timeout=60) as response:
            data = response.read(4*1024*1024 + 1)
        if len(data) > 4*1024*1024:
            raise ValueError('Pinned sample unexpectedly exceeds 4 MiB')
        if local.endswith('.glb') and (data[:4] != b'glTF' or hashlib.sha256(data).hexdigest() != SHA256):
            raise ValueError('Pinned source GLB fingerprint differs')
        (args.output / local).write_bytes(data)
    (args.output / 'download.json').write_text(json.dumps({'source': SOURCE, 'revision': REVISION,
        'sha256': SHA256, 'credit': 'Copyright 2017 Cesium, CC BY 4.0; see SOURCE_README.md and LICENSE.md'}, indent=2)+'\n', encoding='utf-8')
    print('ONLINE RIG: ' + str(args.output.resolve()))


if __name__ == '__main__':
    main()
