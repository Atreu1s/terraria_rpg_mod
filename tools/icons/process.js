const { fs, path, PNG, root, entries, read, write, nearest } = require('./common');

// Remove border-connected solid matte, without deleting enclosed symbol colors.
function clean(src) {
  const { width: w, height: h, data: d } = src;
  const corners = [0, w - 1, (h - 1) * w, h * w - 1];
  const colors = new Map();
  for (const p of corners) if (d[p * 4 + 3] >= 128) {
    const c = [...d.subarray(p * 4, p * 4 + 3)];
    const key = c.join(','); const item = colors.get(key) || { c, n: 0 }; item.n++; colors.set(key, item);
  }
  const matte = [...colors.values()].sort((a, b) => b.n - a.n)[0];
  if (matte && matte.n >= 3) {
    const seen = new Uint8Array(w * h), queue = [];
    const matches = p => d[p * 4 + 3] < 128 || matte.c.every((v, j) => Math.abs(d[p * 4 + j] - v) <= 8);
    const add = p => { if (!seen[p] && matches(p)) { seen[p] = 1; queue.push(p); } };
    for (let x = 0; x < w; x++) { add(x); add((h - 1) * w + x); }
    for (let y = 0; y < h; y++) { add(y * w); add(y * w + w - 1); }
    for (let i = 0; i < queue.length; i++) {
      const p = queue[i]; d[p * 4 + 3] = 0;
      if (p % w) add(p - 1); if (p % w < w - 1) add(p + 1);
      if (p >= w) add(p - w); if (p < w * (h - 1)) add(p + w);
    }
  }
  for (let i = 0; i < d.length; i += 4) {
    d[i + 3] = d[i + 3] >= 128 ? 255 : 0;
    if (!d[i + 3]) d.fill(0, i, i + 4);
  }
  return src;
}

// Weighted median-cut; transparent pixels never enter the palette.
function quantize(png) {
  const histogram = new Map(); const d = png.data;
  for (let i = 0; i < d.length; i += 4) if (d[i + 3]) {
    const key = d[i] * 65536 + d[i + 1] * 256 + d[i + 2];
    const c = histogram.get(key) || { rgb: [...d.subarray(i, i + 3)], n: 0 }; c.n++; histogram.set(key, c);
  }
  if (histogram.size <= 16) return png;
  let boxes = [[...histogram.values()]];
  const extent = b => [0, 1, 2].map(j => Math.max(...b.map(c => c.rgb[j])) - Math.min(...b.map(c => c.rgb[j])));
  while (boxes.length < 16) {
    let best = -1, score = -1;
    boxes.forEach((b, i) => { const s = Math.max(...extent(b)) * Math.sqrt(b.reduce((n, c) => n + c.n, 0)); if (b.length > 1 && s > score) { best = i; score = s; } });
    if (best < 0) break;
    const b = boxes.splice(best, 1)[0], ex = extent(b), channel = ex.indexOf(Math.max(...ex));
    b.sort((a, c) => a.rgb[channel] - c.rgb[channel]);
    const total = b.reduce((n, c) => n + c.n, 0); let n = 0, cut = 1;
    for (let i = 0; i < b.length - 1; i++) { n += b[i].n; cut = i + 1; if (n >= total / 2) break; }
    boxes.push(b.slice(0, cut), b.slice(cut));
  }
  const palette = boxes.map(b => { const n = b.reduce((s, c) => s + c.n, 0); return [0, 1, 2].map(j => Math.round(b.reduce((s, c) => s + c.rgb[j] * c.n, 0) / n)); });
  for (let i = 0; i < d.length; i += 4) if (d[i + 3]) {
    let best = palette[0], distance = Infinity;
    for (const c of palette) { const v = c.reduce((s, x, j) => s + (x - d[i + j]) ** 2, 0); if (v < distance) { best = c; distance = v; } }
    d.set(best, i);
  }
  return png;
}
function processEntry(e) {
  const file = path.join(__dirname, '_raw', e.id + '.png');
  let raw = read(file);
  // The built-in service may ignore requested dimensions. Normalize using NEAREST only.
  if (raw.width !== 512 || raw.height !== 512) { raw = nearest(raw, 512, 512); write(file, raw); }
  const src = clean(raw), d = src.data;
  let left = 512, top = 512, right = -1, bottom = -1;
  for (let y = 0; y < 512; y++) for (let x = 0; x < 512; x++) if (d[(y * 512 + x) * 4 + 3]) {
    left = Math.min(left, x); top = Math.min(top, y); right = Math.max(right, x); bottom = Math.max(bottom, y);
  }
  if (right < 0) throw Error('Raw image contains no opaque symbol');
  const w = right - left + 1, h = bottom - top + 1;
  const dw = Math.max(1, Math.round(w * 28 / Math.max(w, h))), dh = Math.max(1, Math.round(h * 28 / Math.max(w, h)));
  const dst = new PNG({ width: 32, height: 32 }), ox = Math.floor((32 - dw) / 2), oy = Math.floor((32 - dh) / 2);
  for (let y = 0; y < dh; y++) for (let x = 0; x < dw; x++) {
    const sx = left + Math.min(w - 1, Math.floor((x + 0.5) * w / dw));
    const sy = top + Math.min(h - 1, Math.floor((y + 0.5) * h / dh));
    const from = (sy * 512 + sx) * 4;
    dst.data.set(d.subarray(from, from + 4), ((y + oy) * 32 + x + ox) * 4);
  }
  const output = path.resolve(root, e.output);
  if (!/^(Assets\/|Content\/Items\/[^/]+\.png$)/.test(e.output.replaceAll('\\', '/')) || !output.startsWith(root + path.sep)) throw Error('Output outside permitted asset directories');
  write(output, quantize(dst));
}
const args = process.argv.slice(2);
if (args.length && (args.length !== 2 || args[0] !== '--id')) { console.error('Usage: node tools/icons/process.js [--id ID]'); process.exit(1); }
const selected = args.length ? entries.filter(e => e.id === args[1]) : entries;
if (!selected.length) { console.error('Unknown icon id: ' + args[1]); process.exit(1); }
let failed = 0;
for (const e of selected) { try { processEntry(e); console.log('Processed ' + e.id); } catch (error) { failed++; console.error(e.id + ': ' + error.message); } }
if (failed) process.exitCode = 1;
