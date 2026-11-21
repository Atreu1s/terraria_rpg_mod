const fs = require('node:fs');
const path = require('node:path');

const root = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(root, 'Common/Data/SkillTree.cs'), 'utf8');
const locale = fs.readFileSync(path.join(root, 'Localization/en-US_Mods.RPGMod.hjson'), 'utf8');

const classes = {
  warrior: { name: 'Warrior', color: '#e8604c', path: [0, 1, 3, 7] },
  ranger: { name: 'Ranger', color: '#5cc461', path: [0, 1, 4, 9] },
  mage: { name: 'Mage', color: '#a77bf0', path: [0, 2, 5, 11] },
  summoner: { name: 'Summoner', color: '#3cc8d0', path: [0, 2, 6, 14] },
  rogue: { name: 'Rogue', color: '#d46bb0', path: [0, 1, 4, 10] },
};

function readSection(name) {
  const section = locale.match(new RegExp('^' + name + ': \\{\\r?\\n([\\s\\S]*?)^}', 'm'));
  if (!section) throw new Error('Missing localization section: ' + name);
  return Object.fromEntries(Array.from(section[1].matchAll(/^\t([a-z_]+): \{\r?\n([\s\S]*?)^\t\}/gm), ([, key, body]) => [
    key, Object.fromEntries(Array.from(body.matchAll(/^\t\t(Name|Effect|Bonus): (.+)$/gm), ([, field, value]) => [
      field, value.startsWith('"') ? JSON.parse(value) : value,
    ])),
  ]));
}

const trees = Object.fromEntries(Object.keys(classes).map(id => {
  const registry = source.match(new RegExp('string\\[\\] ' + id + '\\s*=\\s*\\{([^}]+)\\}'));
  if (!registry) throw new Error('Missing skill tree: ' + id);
  const nodes = Array.from(registry[1].matchAll(/"([a-z_]+)"/g), match => match[1]);
  if (nodes.length !== 15 || nodes[0] !== id || new Set(nodes).size !== nodes.length)
    throw new Error('Invalid skill tree: ' + id);
  return [id, nodes];
}));

module.exports = { root, classes, trees, nodes: readSection('Nodes'), awakenings: readSection('Awakening') };
