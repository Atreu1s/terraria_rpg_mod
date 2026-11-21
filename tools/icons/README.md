# RPG skill-tree icons

Prompts, ids, order, and exact destination paths come from `docs/icons.json`. Images are generated individually with the built-in image generation tool and a transparent background. No Python is used.

## Reprocess one icon

From the repository root:

```powershell
node tools/icons/process.js --id berserker
```

This regenerates the final PNG from `tools/icons/_raw/berserker.png`; it does not call an image API. To redraw an icon, use its unchanged manifest prompt with the built-in image generator, request a transparent 512x512 image, replace its raw PNG, and run the command above. The service can return larger images; the processor normalizes raw files to 512x512 with nearest-neighbor sampling.

## Install and build the set

```powershell
npm.cmd install --prefix tools/icons --cache tools/icons/node_modules/.npm-cache --no-audit --no-fund
node tools/icons/process.js
node tools/icons/check.js
node tools/icons/preview.js
```

Processing removes a border-connected solid matte when at least three corners agree, thresholds alpha at 128, crops to the opaque bounds, and samples the symbol into a centered 28x28 area of a 32x32 canvas. Resizing always uses nearest neighbor. Weighted median-cut quantization limits the opaque palette to 16 colors. Outputs are 8-bit RGBA PNGs.

The validator checks every manifest path, dimensions, PNG RGBA mode, all four transparent corners, binary alpha, nonempty silhouettes, and at most 16 opaque colors. Failures print individually and produce a nonzero exit status.

`_preview.png` uses exact 4x nearest-neighbor enlargement, navy #2f3a96, ten icons per row in manifest order, and bitmap id labels. Labels are confined to the preview.

## Visual review

Recommended redraws after inspecting the 4x nearest-neighbor preview:

- `will_shield`: the solid bright gold barrier has a softer contour and weaker dark outline than the set.
- `eye_for_eye`: the horizontal silhouette is too thin; the central heart is difficult to read.
- `long_range`: the shaft and dotted trail have very little visual weight at 32x32.
- `beastmaster`: wispy, fragmented edge detail weakens the wolf silhouette and consistent outline.
- `pack`: the three wolf heads blend together; use three clearer, simpler heads.
- `bond`: the hand and spirit paw are hard to distinguish from the linking thread.
- `legion`: the small repeated shields lose their individual silhouettes at this resolution.

These are visual recommendations. The automated checker validates PNG properties, not artistic consistency. See `REPORT.md` for the final validation result.


## v2 (Calamity rogue)

The 17 v2 IDs are listed in `v2-ids.json`. Generate each with its complete
`docs/icons.json` prompt and a transparent background, save the source to
`_raw/<id>.png`, then process only that ID:

```powershell
node tools/icons/process.js --id rogue
node tools/icons/check.js
node tools/icons/check-v2.js
node tools/icons/preview.js --v2
```

Do not run `process.js` without `--id` for v2: existing v1 PNGs must stay
unchanged. `check-v2.js` compares all 74 v1 assets against the SHA-256 hashes
in `baseline-v1.json` and checks the 17 normalized 512x512 raw sources.
The unchanged v1 processor produces 32x32 RGBA, binary alpha, a centered
28x28 symbol area, nearest-neighbor downscaling and at most 16 opaque colors.

`_preview_v2.png` contains the four existing roots (`warrior`, `ranger`,
`mage`, `summoner`) followed by all 17 v2 icons, seven per row, enlarged
exactly 4x on the same navy background as the v1 preview.
See `docs/v2/REPORT_B.md` for visual review and generation notes.
