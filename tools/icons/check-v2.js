const {fs,path,root,entries,read}=require('./common');
const crypto=require('node:crypto');
const ids=require('./v2-ids.json');const baseline=require('./baseline-v1.json');
let failed=0;const fail=message=>{failed++;console.error(message);};
if(entries.length!==91||new Set(entries.map(e=>e.id)).size!==91)fail('Expected 91 unique manifest IDs');
if(ids.length!==17||new Set(ids).size!==17)fail('Expected 17 unique v2 IDs');
for(const id of ids){
 const e=entries.find(e=>e.id===id);if(!e){fail('Missing ID '+id);continue;}
 const expected=id==='stat_cun'?'stat':id==='awaken_rogue'?'awakening':'rogue';
 if(e.group!==expected)fail('Wrong group '+id);
 if(baseline[e.output])fail('v2 path overlaps v1: '+id);
 try{const p=read(path.join(__dirname,'_raw',id+'.png'));if(p.width!==512||p.height!==512)fail('Raw not 512x512: '+id);}catch(error){fail(id+': '+error.message);}
}
for(const [file,hash] of Object.entries(baseline)){
 const now=crypto.createHash('sha256').update(fs.readFileSync(path.join(root,file))).digest('hex');
 if(now!==hash)fail('Existing PNG changed: '+file);
}
if(failed)process.exitCode=1;else console.log('PASS: 17 v2 IDs/raw sources; 91 unique entries; all 74 v1 PNG hashes unchanged.');
