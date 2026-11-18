const { fs, path, root, entries, read } = require('./common');
const failures = [];
for (const e of entries) {
  const file = path.resolve(root, e.output), problems = [];
  try {
    const bytes = fs.readFileSync(file), png = read(file);
    if (png.width !== 32 || png.height !== 32) problems.push(`size ${png.width}x${png.height}, expected 32x32`);
    if (bytes[25] !== 6 || bytes[24] !== 8) problems.push('mode is not 8-bit RGBA');
    const d = png.data, colors = new Set(); let opaque = 0;
    const corners = [0, png.width - 1, (png.height - 1) * png.width, png.width * png.height - 1];
    if (corners.some(p => d[p * 4 + 3] !== 0)) problems.push('corner pixel is not transparent');
    let partial = false;
    for (let i = 0; i < d.length; i += 4) {
      if (d[i + 3] !== 0 && d[i + 3] !== 255) partial = true;
      if (d[i + 3] === 255) { opaque++; colors.add(`${d[i]},${d[i + 1]},${d[i + 2]}`); }
    }
    if (partial) problems.push('alpha is not binary');
    if (!opaque) problems.push('empty symbol');
    if (colors.size > 16) problems.push(`${colors.size} opaque colors, maximum 16`);
  } catch (error) { problems.push(error.code === 'ENOENT' ? 'file missing' : error.message); }
  if (problems.length) failures.push(`${e.id} (${e.output}): ${problems.join('; ')}`);
}
if (failures.length) { console.error(failures.join('\n')); console.error(`FAIL: ${failures.length}/${entries.length} icons failed`); process.exitCode = 1; }
else console.log(`PASS: ${entries.length}/${entries.length} icons; 32x32, RGBA, transparent corners, binary alpha, <=16 opaque colors.`);
