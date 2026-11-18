# Icon generation report

Generated and saved all **74/74** icons from `docs/icons.json` using the built-in image generation tool. All ids and destination paths are unchanged: 64 skill icons, 6 stat icons, and 4 item icons.

## Validation

`node tools/icons/check.js` completed with exit code 0:

```text
PASS: 74/74 icons; 32x32, RGBA, transparent corners, binary alpha, <=16 opaque colors.
```

A separate inspection confirmed that all 74 files in `_raw/` are 512x512. The final preview contains all 74 icons, enlarged exactly 4x using nearest neighbor, ten per row in manifest order, against #2f3a96.

## Files and regeneration

- `_raw/<id>.png`: transparent generated source images, normalized to 512x512 with nearest-neighbor sampling when the image service returned a larger square image. The generator's original files remain in its default generated-images directory.
- `process.js`: matte cleanup, alpha threshold 128, opaque-bound cropping, nearest-neighbor fit into a centered 28x28 area, at most 16 opaque colors, and 32x32 RGBA export to the manifest path.
- `check.js`: manifest-wide validation, detailed failure list, and nonzero exit status on failure.
- `preview.js` and `_preview.png`: full-set preview with bitmap id labels.
- `package.json` and `package-lock.json`: pure-JavaScript `pngjs` dependency. No Python was installed or used.

Regenerate one final asset from its raw image:

```powershell
node tools/icons/process.js --id berserker
```

See `README.md` for installation and redraw instructions. Generation used each manifest prompt plus an explicit 512x512 request. `gunner` and `overheat` initially received image-service safety rejections; both succeeded after adding fictional game-inventory context and nonfunctional cartoon-prop proportions. Their manifest prompts were not edited.

## Recommended redraws

The full preview was visually inspected. These seven icons would benefit from another art pass:

- `will_shield`: the bright gold barrier has a softer contour and weaker dark outline than the set.
- `eye_for_eye`: thin horizontal silhouette; the central heart is difficult to read.
- `long_range`: the shaft and dotted trail have insufficient visual weight at 32x32.
- `beastmaster`: wispy, fragmented edge detail weakens the wolf silhouette and outline.
- `pack`: the three wolf heads blend together; simplify and separate their silhouettes.
- `bond`: the hand and spirit paw are hard to distinguish from the connecting thread.
- `legion`: the repeated shields are too small to retain clear individual silhouettes.

The PNG-property checks pass for these icons as well; artistic consistency is a visual assessment, not something the checker certifies.

## Scope

This work wrote only icon PNGs under `Assets/` and `Content/Items/`, and assets, scripts, dependency files, and reports under `tools/icons/`. No C#, HJSON, build.txt, project files, or files under `docs/` were changed by this work.
