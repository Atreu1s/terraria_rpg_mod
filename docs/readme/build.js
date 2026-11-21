const fs = require('node:fs');
const path = require('node:path');
const { root, classes, trees, nodes, awakenings } = require('./data');

const escapeCell = text => text.replaceAll('|', '\\|');
const icon = id => '<img src="Assets/SkillIcons/' + id + '.png" width="32" alt=""/>';
const tiers = ['Class', 'I', 'II', 'III'];

function entry(section, id, fields) {
  const value = section[id];
  if (!value || fields.some(field => !value[field])) throw new Error('Missing English localization for ' + id);
  return value;
}

const nodeTables = Object.entries(classes).map(([id, info]) => {
  const title = info.name + (id === 'rogue' ? ' (Calamity)' : '') + ' — all 15 nodes';
  const rows = trees[id].map((skill, index, tree) => {
    const { Name, Effect } = entry(nodes, skill, ['Name', 'Effect']);
    const parent = index === 0 ? '—' : entry(nodes, tree[Math.floor((index - 1) / 2)], ['Name']).Name;
    return '| ' + [
      icon(skill),
      tiers[Math.floor(Math.log2(index + 1))],
      '**' + escapeCell(Name) + '**',
      escapeCell(parent),
      escapeCell(Effect),
    ].join(' | ') + ' |';
  });

  return [
    '<details>',
    '<summary><b>' + title + '</b></summary>',
    '',
    '| Icon | Tier | Node | Requires | Effect |',
    '| :---: | :---: | --- | --- | --- |',
    ...rows,
    '',
    '</details>',
  ].join('\n');
}).join('\n\n');

const awakeningTable = [
  '| Icon | Class | Awakening | Effect | Specialization bonus |',
  '| :---: | --- | --- | --- | --- |',
  ...Object.entries(classes).map(([id, info]) => {
    const { Name, Effect, Bonus } = entry(awakenings, id, ['Name', 'Effect', 'Bonus']);
    return '| ' + [icon('awaken_' + id), info.name, '**' + escapeCell(Name) + '**', escapeCell(Effect), escapeCell(Bonus)].join(' | ') + ' |';
  }),
].join('\n');

require('./render');
const template = fs.readFileSync(path.join(__dirname, 'README.template.md'), 'utf8');
const output = template.replace('{{NODES}}', () => nodeTables).replace('{{AWAKENINGS}}', () => awakeningTable);
fs.writeFileSync(path.join(root, 'README.md'), output);
console.log('README.md generated.');
