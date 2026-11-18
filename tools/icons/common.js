// Paths resolve relative to the repository, independent of cwd.
const fs = require('node:fs');
const path = require('node:path');
const { PNG } = require('pngjs');
const root = path.resolve(__dirname, '../..');
const entries = JSON.parse(fs.readFileSync(path.join(root, 'docs/icons.json'), 'utf8').replace(/^\uFEFF/, ''));
function read(file) { return PNG.sync.read(fs.readFileSync(file)); }
function write(file, png) {
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, PNG.sync.write(png, { colorType: 6, inputColorType: 6 }));
}
function nearest(src, width, height) {
  const dst = new PNG({ width, height });
  for (let y = 0; y < height; y++) for (let x = 0; x < width; x++) {
    const from = (Math.floor(y * src.height / height) * src.width + Math.floor(x * src.width / width)) * 4;
    dst.data.set(src.data.subarray(from, from + 4), (y * width + x) * 4);
  }
  return dst;
}
module.exports = { fs, path, PNG, root, entries, read, write, nearest };
