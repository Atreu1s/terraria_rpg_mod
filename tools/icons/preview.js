const { path, PNG, root, entries, read, write } = require('./common');
// Tiny 3x5 bitmap font used only for preview labels, never in game assets.
const patterns = {
 a:'010101111101101',b:'110101110101110',c:'011100100100011',d:'110101101101110',e:'111100110100111',f:'111100110100100',
 g:'011100101101011',h:'101101111101101',i:'111010010010111',j:'001001001101010',k:'101101110101101',l:'100100100100111',
 m:'101111111101101',n:'101111111111101',o:'010101101101010',p:'110101110100100',q:'010101101111011',r:'110101110101101',
 s:'011100010001110',t:'111010010010010',u:'101101101101111',v:'101101101101010',w:'101101111111101',x:'101101010101101',
 y:'101101010010010',z:'111001010100111',_:'000000000000111',
 '0':'111101101101111','1':'010110010010111','2':'110001010100111','3':'110001010001110','4':'101101111001001',
 '5':'111100110001110','6':'011100111101111','7':'111001010010010','8':'111101111101111','9':'111101111001110'
};
const v2 = process.argv.includes('--v2');
const ids = v2 ? ['warrior', 'ranger', 'mage', 'summoner', ...require('./v2-ids.json')] : entries.map(e => e.id);
const selected = ids.map(id => entries.find(e => e.id === id));
const filename = v2 ? '_preview_v2.png' : '_preview.png';
const cw = 160, ch = 152, cols = v2 ? 7 : 10, rows = Math.ceil(selected.length / cols);
const canvas = new PNG({ width: cols * cw, height: rows * ch });
for (let i = 0; i < canvas.data.length; i += 4) canvas.data.set([47, 58, 150, 255], i);
function pixel(x, y, color) { canvas.data.set(color, (y * canvas.width + x) * 4); }
function label(text, cx, y) {
  const start = Math.floor(cx - (text.length * 8 - 2) / 2);
  [...text.toLowerCase()].forEach((c, j) => {
    const bits = patterns[c]; if (!bits) throw Error('Unsupported label character: ' + c);
    for (let k = 0; k < 15; k++) if (bits[k] === '1') for (let dy = 0; dy < 2; dy++) for (let dx = 0; dx < 2; dx++) pixel(start + j * 8 + (k % 3) * 2 + dx, y + Math.floor(k / 3) * 2 + dy, [240, 240, 255, 255]);
  });
}
let missing = 0;
selected.forEach((e, i) => {
  const ox = i % cols * cw, oy = Math.floor(i / cols) * ch;
  try {
    const src = read(path.join(root, e.output));
    if (src.width !== 32 || src.height !== 32) throw Error('Expected 32x32');
    for (let y = 0; y < 128; y++) for (let x = 0; x < 128; x++) {
      const p = (Math.floor(y / 4) * 32 + Math.floor(x / 4)) * 4;
      if (src.data[p + 3] === 255) pixel(ox + 16 + x, oy + 4 + y, src.data.subarray(p, p + 4));
    }
  } catch (error) { missing++; console.error(e.id + ': ' + error.message); }
  label(e.id, ox + cw / 2, oy + 136);
});
write(path.join(__dirname, filename), canvas);
console.log(`Preview: tools/icons/${filename} (${selected.length - missing}/${selected.length} icons, ${cols} per row)`);
if (missing) process.exitCode = 1;

