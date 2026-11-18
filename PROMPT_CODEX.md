# Промпт для Codex (иконки)

Скопируй всё, что ниже линии, в Codex, открытый в той же папке мода. Codex можно запускать параллельно с Claude Code: он работает только с картинками и папкой `tools/icons/`, код мода не трогает.

---

You are generating pixel-art icons for a Terraria tModLoader mod (RPG skill tree). Your scope is ONLY image assets and helper scripts under `tools/icons/`. Do not create, edit or delete any `.cs`, `.hjson`, `build.txt`, `.csproj` or anything under `docs/` — another agent (Claude Code) owns the code.

**Input:** `docs/icons.json` — 74 entries with `id`, `name_ru`, `group`, `output` and `prompt`. Each `prompt` already contains the full shared style. The human-readable list is `docs/ICONS.md`.

**For EACH entry:**
1. Generate one 512×512 image from `prompt` with a transparent background.
2. Save the raw image to `tools/icons/_raw/<id>.png`.
3. Post-process with Python + Pillow (write the script as `tools/icons/process.py`):
   - make the background fully transparent; remove any leftover solid background color;
   - downscale to 32×32 with `Image.NEAREST` only (never bilinear/bicubic/lanczos);
   - quantize to at most 16 colors while keeping alpha; alpha threshold 128 so every pixel is fully opaque or fully transparent;
   - save as RGBA PNG to the exact path in `output` (create folders as needed).
4. Never rename ids or change output paths — the game code loads icons by these exact paths.

**After all icons are done:**
- Write `tools/icons/check.py` that verifies every entry in `docs/icons.json`: file exists at `output`, size is 32×32, mode RGBA, corner pixel transparent, at most 16 opaque colors. Print a list of failures.
- Build `tools/icons/_preview.png`: all icons scaled ×4 with nearest neighbor on a navy background `#2f3a96`, 10 per row, the id printed under each icon, grouped in the order of `docs/icons.json`.
- Write `tools/icons/README.md` with: how to regenerate one icon by id (`python tools/icons/process.py --id berserker`), and the list of icons you think look off-style and should be redone.

**Style rules (must hold for all 74):**
- Only the symbol: no frame, slot, border, background, text, letters or numbers — the game draws frames and states itself.
- Consistent look across the whole set: same outline weight (1 px dark outline), top-left light, flat 2–3 step shading, limited palette.
- Readability at 32×32 matters more than detail: one clear silhouette per icon.

If image generation is not available in your environment, stop and say so instead of drawing placeholder art with code.

When done, report: number of icons generated, check.py result, and which ids you recommend regenerating.
