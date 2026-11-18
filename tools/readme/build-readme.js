// Собирает README.md из шаблона: таблицы 60 узлов берутся из docs/DESIGN.md §4.
// Запуск: node tools/readme/build-readme.js
const fs = require('node:fs');
const path = require('node:path');

const root = path.resolve(__dirname, '../..');
const design = fs.readFileSync(path.join(root, 'docs/DESIGN.md'), 'utf8').split(/\r?\n/);

const classes = [
  { title: '🔴 Воин — все 15 узлов', marker: '### Воин' },
  { title: '🟢 Стрелок — все 15 узлов', marker: '### Стрелок' },
  { title: '🟣 Маг — все 15 узлов', marker: '### Маг' },
  { title: '🔵 Призыватель — все 15 узлов', marker: '### Призыватель' },
];
const tierLabel = { 'Класс': '🎓', '1': 'I', '2': 'II', '3': 'III' };

let blocks = '';
for (const cls of classes) {
  const start = design.findIndex(l => l.startsWith(cls.marker));
  if (start < 0) throw new Error('not found ' + cls.marker);
  const rows = [];
  for (let i = start + 1; i < design.length && !design[i].startsWith('###') && !design[i].startsWith('## '); i++) {
    const m = design[i].match(/^\| (\d+) \| ([^|]+) \| `([a-z_]+)` \| ([^|]+) \| ([^|]+) \| ([^|]+) \|$/);
    if (m) rows.push({ tier: m[2].trim(), id: m[3], name: m[4].trim(), effect: m[6].trim() });
  }
  if (rows.length !== 15) throw new Error(`${cls.marker}: ${rows.length} rows`);
  blocks += `<details>\n<summary><b>${cls.title}</b></summary>\n\n`;
  blocks += '| | Тир | Узел | Эффект |\n| :---: | :---: | --- | --- |\n';
  for (const r of rows)
    blocks += `| <img src="Assets/SkillIcons/${r.id}.png" width="32"/> | ${tierLabel[r.tier] ?? r.tier} | **${r.name}** | ${r.effect} |\n`;
  blocks += '\n</details>\n\n';
}

const tpl = fs.readFileSync(path.join(__dirname, 'README.template.md'), 'utf8');
fs.writeFileSync(path.join(root, 'README.md'), tpl.replace('{{NODES}}', blocks.trimEnd()));
console.log('README.md written');
