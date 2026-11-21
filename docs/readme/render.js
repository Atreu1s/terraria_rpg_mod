const fs = require('node:fs');
const path = require('node:path');
const { root, classes, trees } = require('./data');
const { PNG } = require('../../tools/icons/node_modules/pngjs');

const out = __dirname;

const hex = (h, a = 255) => [parseInt(h.slice(1, 3), 16), parseInt(h.slice(3, 5), 16), parseInt(h.slice(5, 7), 16), a];
const C = {
  page: hex('#0d1117'), panel: hex('#2f3a96'), panelBorder: hex('#9fb0ff'),
  slot: hex('#3f52be'), slotBorder: hex('#b8c4ff'), dark: hex('#191e50'), darkBorder: hex('#5d6bb8'),
  line: hex('#1b2266'), accent: hex('#ffe066'), gold: hex('#f2c14e'), white: hex('#ffffff'),
};
const colors = Object.fromEntries(Object.entries(classes).map(([id, info]) => [id, hex(info.color)]));

function canvas(w, h, bg) {
  const png = new PNG({ width: w, height: h });
  for (let i = 0; i < w * h; i++) png.data.set(bg, i * 4);
  return png;
}
function blend(png, x, y, [r, g, b, a]) {
  if (x < 0 || y < 0 || x >= png.width || y >= png.height || a === 0) return;
  const i = (y * png.width + x) * 4, k = a / 255, d = png.data;
  d[i] = Math.round(r * k + d[i] * (1 - k));
  d[i + 1] = Math.round(g * k + d[i + 1] * (1 - k));
  d[i + 2] = Math.round(b * k + d[i + 2] * (1 - k));
  d[i + 3] = 255;
}
function rect(png, x, y, w, h, col) {
  for (let yy = y; yy < y + h; yy++) for (let xx = x; xx < x + w; xx++) blend(png, xx, yy, col);
}
function rrect(png, x, y, w, h, r, fill, border, t = 2) {
  for (let yy = 0; yy < h; yy++) for (let xx = 0; xx < w; xx++) {
    const cx = Math.max(r - xx - 0.5, 0, xx + 0.5 - (w - r)), cy = Math.max(r - yy - 0.5, 0, yy + 0.5 - (h - r));
    const dist = Math.hypot(cx, cy);
    if (dist > r) continue;
    const edge = xx < t || yy < t || xx >= w - t || yy >= h - t || dist > r - t;
    blend(png, x + xx, y + yy, edge && border ? border : fill);
  }
}
function glow(png, cx, cy, radius, col, strength) {
  for (let y = -radius; y <= radius; y++) for (let x = -radius; x <= radius; x++) {
    const d = Math.hypot(x, y) / radius;
    if (d >= 1) continue;
    blend(png, cx + x, cy + y, [col[0], col[1], col[2], Math.round(255 * strength * (1 - d) * (1 - d))]);
  }
}
const iconCache = {};
function icon(rel) {
  return iconCache[rel] ??= PNG.sync.read(fs.readFileSync(path.join(root, rel)));
}
function blit(png, src, x, y, scale = 1, tint = null) {
  for (let sy = 0; sy < src.height; sy++) for (let sx = 0; sx < src.width; sx++) {
    const i = (sy * src.width + sx) * 4;
    let c = [src.data[i], src.data[i + 1], src.data[i + 2], src.data[i + 3]];
    if (c[3] === 0) continue;
    if (tint) c = [c[0] * tint[0] / 255, c[1] * tint[1] / 255, c[2] * tint[2] / 255, c[3] * tint[3] / 255];
    for (let dy = 0; dy < scale; dy++) for (let dx = 0; dx < scale; dx++) blend(png, x + sx * scale + dx, y + sy * scale + dy, c);
  }
}
function slot(png, x, y, size, iconRel, opts = {}) {
  const { border = C.slotBorder, fill = C.slot, scale = 1, glowCol = null, tint = null } = opts;
  if (glowCol) glow(png, x + size / 2, y + size / 2, size * 0.85, glowCol, 0.55);
  rrect(png, x, y, size, size, 8, fill, border, 2);
  const ic = icon(iconRel), w = ic.width * scale;
  blit(png, ic, x + Math.floor((size - w) / 2), y + Math.floor((size - w) / 2), scale, tint);
}
const save = (name, png) => fs.writeFileSync(path.join(out, name), PNG.sync.write(png));

// Skill tree geometry matches the character panel mockups.
const centers = [[70, 360], [250, 180], [250, 540], [440, 90], [440, 270], [440, 450], [440, 630]];
for (let i = 0; i < 8; i++) centers.push([620, 45 + 90 * i]);

function hline(png, x1, x2, y, t, col, dashed) {
  const [a, b] = [Math.min(x1, x2) - t / 2, Math.max(x1, x2) + t / 2];
  for (let x = a; x < b; x += dashed ? 13 : b - a) rect(png, Math.round(x), Math.round(y - t / 2), Math.ceil(Math.min(dashed ? 7 : b - a, b - x)), t, col);
}
function vline(png, x, y1, y2, t, col, dashed) {
  const [a, b] = [Math.min(y1, y2) - t / 2, Math.max(y1, y2) + t / 2];
  for (let y = a; y < b; y += dashed ? 13 : b - a) rect(png, Math.round(x - t / 2), Math.round(y), t, Math.ceil(Math.min(dashed ? 7 : b - a, b - y)), col);
}
function edge(png, ox, oy, p, c, col, t, dashed = false) {
  const [px, py] = centers[p], [cx, cy] = centers[c];
  const x1 = ox + px + 26, x2 = ox + cx - 26, mid = (x1 + x2) / 2;
  hline(png, x1, mid, oy + py, t, col, dashed);
  vline(png, mid, oy + py, oy + cy, t, col, dashed);
  hline(png, mid, x2, oy + cy, t, col, dashed);
}

function renderTree(cls, demo, file = 'tree_' + cls + '.png') {
  const ids = trees[cls];
  const W = 880, H = 760, ox = 40, oy = 20;
  const png = canvas(W, H, C.page);
  rrect(png, 8, 4, W - 16, H - 8, 12, [...C.panel.slice(0, 3), 235], C.panelBorder, 2);
  const picked = new Set(demo);
  const last = demo.at(-1);
  for (let i = 1; i < 15; i++) edge(png, ox, oy, (i - 1) >> 1, i, C.line, 4);
  for (let i = 1; i < 15; i++) if (picked.has(i)) edge(png, ox, oy, (i - 1) >> 1, i, C.accent, 4);
  for (let i = 0; i < 15; i++) {
    const [cx, cy] = centers[i];
    const sib = i === 0 ? -1 : (i % 2 ? i + 1 : i - 1);
    const parentPicked = i === 0 || picked.has((i - 1) >> 1);
    const isPicked = picked.has(i);
    const closed = !isPicked && (!parentPicked || picked.has(sib));
    const opts = isPicked
      ? { border: i === last ? C.gold : C.accent, glowCol: i === last ? C.gold : (i === 0 ? colors[cls] : null) }
      : closed ? { border: C.darkBorder, fill: C.dark, tint: [154, 166, 230, picked.has(sib) ? 120 : 255] }
        : { border: C.white };
    slot(png, ox + cx - 26, oy + cy - 26, 52, `Assets/SkillIcons/${ids[i]}.png`, opts);
  }
  // Show the class awakening beside the highlighted tier III node.
  const [lx, ly] = centers[last];
  slot(png, ox + lx + 84, oy + ly - 26, 52, `Assets/SkillIcons/awaken_${cls}.png`, { border: C.gold, glowCol: C.gold });
  hline(png, ox + lx + 26, ox + lx + 84, oy + ly, 3, C.gold, true);
  save(file, png);
}

for (const [id, info] of Object.entries(classes)) renderTree(id, info.path);
renderTree('rogue', [0, 2, 5, 11], 'tree_rogue_trickster.png');

// A row of 15 icons for each class.
{
  const S = 2, cell = 76, pad = 24;
  const png = canvas(pad * 2 + 15 * cell, pad * 2 + Object.keys(trees).length * cell, C.page);
  Object.entries(trees).forEach(([cls, ids], row) => {
    rrect(png, pad - 8, pad + row * cell - 2, 15 * cell + 16, cell - 4, 10, [...colors[cls].slice(0, 3), 40], [...colors[cls].slice(0, 3), 160], 2);
    ids.forEach((id, col) => {
      const ic = icon(`Assets/SkillIcons/${id}.png`);
      blit(png, ic, pad + col * cell + (cell - 32 * S) / 2, pad + row * cell + (cell - 4 - 32 * S) / 2 - 2, S);
    });
  });
  save('banner.png', png);
}

// Class, stat, awakening, and item galleries.
function row(name, items, { size = 88, scale = 2, gap = 28, border = null, glowCol = null } = {}) {
  const pad = 28;
  const png = canvas(pad * 2 + items.length * size + (items.length - 1) * gap, pad * 2 + size, C.page);
  items.forEach((it, i) => slot(png, pad + i * (size + gap), pad, size, it.icon, {
    scale, border: it.border ?? border ?? C.slotBorder, glowCol: it.glow ?? glowCol,
  }));
  save(name, png);
}
row('classes.png', Object.keys(trees).map(c => ({ icon: `Assets/SkillIcons/${c}.png`, border: colors[c], glow: colors[c] })), { size: 104, scale: 3, gap: 44 });
row('stats.png', ['str', 'agi', 'int', 'spi', 'cun', 'vit', 'luck'].map(s => ({ icon: `Assets/StatIcons/stat_${s}.png` })));
row('awakening.png', Object.keys(trees).map(c => ({ icon: `Assets/SkillIcons/awaken_${c}.png` })), { size: 104, scale: 3, gap: 44, border: C.gold, glowCol: C.gold });
row('items.png', ['StatResetItem', 'TreeResetItem', 'AwakeningEssence', 'AwakeningStone'].map(n => ({ icon: `Content/Items/${n}.png` })));

console.log('README illustrations generated.');
