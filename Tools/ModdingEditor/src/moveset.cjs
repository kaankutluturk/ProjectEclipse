// movesets/*.json support. Parses with positions, mirrors the strict C# reader
// (ModMovesetJson), and converts each entry into the luaparse-shaped table nodes that
// native.cjs, claims.cjs and report.cjs already understand, so data files get the same
// vanilla guard checks, conflict claims and move-change report as sf2.moves.patch.
const jsonc=require('jsonc-parser');
const native=require('./native.cjs');
const MOVE_FIELDS=['move','note','disable','priority','playback_rate','animation','input','sound_frame','intervals','attacks','new_attacks','clip_range','chains'];
const FORK_FIELDS=[...MOVE_FIELDS,'id','subtype','item','add'];
const GUARD=['expected','value'];
const SHAPES={
    priority:{guard:'integer'},playback_rate:{guard:'number'},input:{guard:'input'},
    sound_frame:{fields:{name:'string',expected:'integer',value:'integer'},required:['name','expected','value']},
};
// Convert a jsonc node into a luaparse-like node with a [start,end] range.
function lua(node,text){
    const range=[node.offset,node.offset+node.length];
    if(node.type==='object')return {type:'TableConstructorExpression',range,fields:(node.children??[]).map(p=>({type:'TableKeyString',key:{type:'Identifier',name:p.children[0].value},value:lua(p.children[1],text),range:[p.offset,p.offset+p.length]}))};
    if(node.type==='array')return {type:'TableConstructorExpression',range,fields:(node.children??[]).map(c=>({type:'TableValue',value:lua(c,text)}))};
    if(node.type==='string')return {type:'StringLiteral',value:node.value,raw:text.slice(range[0],range[1]),range};
    if(node.type==='number')return {type:'NumericLiteral',value:node.value,raw:text.slice(range[0],range[1]),range,json:{integer:/^-?\d+$/.test(text.slice(range[0],range[1]))}};
    if(node.type==='boolean')return {type:'BooleanLiteral',value:node.value,raw:String(node.value),range};
    return {type:'NilLiteral',range};
}
// Lua writes impulse as { x, y, z }; JSON writes [x, y, z]. Normalize for shared checks.
function normalizeImpulse(table){
    for(const attack of arrayValues(field(table,'attacks')))
        for(const side of GUARD){
            const impulse=field(attack,'impulse'),node=impulse&&field(impulse,side);
            if(node?.type==='TableConstructorExpression'&&node.fields.every(f=>f.type==='TableValue')&&node.fields.length===3)
                node.fields=node.fields.map((f,i)=>({type:'TableKeyString',key:{type:'Identifier',name:'xyz'[i]},value:f.value}));
        }
}
const field=(table,name)=>table?.fields?.find(f=>f.type==='TableKeyString'&&f.key.name===name)?.value;
const arrayValues=node=>node?.type==='TableConstructorExpression'?node.fields.filter(f=>f.type==='TableValue').map(f=>f.value):[];
function parse(text){
    const errors=[],issues=[],entries=[];
    const tree=jsonc.parseTree(text,errors,{allowTrailingComma:false,disallowComments:true});
    const at=(node,message)=>issues.push({offset:node?.offset??0,length:node?.length??0,message});
    for(const e of errors)issues.push({offset:e.offset,length:e.length,message:`Invalid JSON: ${jsonc.printParseErrorCode(e.error)}.`});
    if(!tree||errors.length)return {issues,entries};
    if(tree.type!=='object'){at(tree,'A moveset file must be a JSON object.');return {issues,entries};}
    const props=node=>{const seen=new Map();for(const p of node.children??[]){const name=p.children[0].value;if(seen.has(name))at(p.children[0],`Duplicate field "${name}".`);seen.set(name,p.children[1]);}return seen;};
    const known=(node,allowed,where)=>{for(const p of node.children??[])if(!allowed.includes(p.children[0].value))at(p.children[0],`Unknown field "${p.children[0].value}" in ${where}.`);};
    // A key input: one tapped key name, or alternatives (arrays) of keys (a name or { key, press }).
    const KEYS=['Up','Up-Forward','Forward','Down-Forward','Down','Down-Back','Back','Up-Back','Punch','Kick','Ranged','Magic','RaidCharge','Super'];
    const input=(node,where)=>{
        if(node.type==='string'){if(!KEYS.includes(node.value))at(node,`${where} must be a key name such as "Punch".`);return;}
        if(node.type!=='array'){at(node,`${where} must be a key name or an array of key chords.`);return;}
        if(node.children.length>8)at(node,`${where} has more than 8 alternatives.`);
        node.children.forEach((chord,c)=>{
            if(chord.type!=='array'||!chord.children.length||chord.children.length>14){at(chord,`${where}[${c}] must be an array of 1-14 keys.`);return;}
            chord.children.forEach((key,k)=>{
                const here=`${where}[${c}][${k}]`;
                if(key.type==='string'){if(!KEYS.includes(key.value))at(key,`${here} is not a key name.`);return;}
                if(key.type!=='object'){at(key,`${here} must be a key name or { "key", "press" }.`);return;}
                const parts=props(key);known(key,['key','press'],here);
                if(!parts.has('key')||!KEYS.includes(parts.get('key').value))at(key,`${here} needs a "key" name.`);
                if(parts.has('press')&&!['Tap','Hold','Release'].includes(parts.get('press').value))at(parts.get('press'),`${here}.press must be "Tap", "Hold" or "Release".`);
            });
        });
    };
    const type=(node,expected,where)=>{
        if(expected==='input'){input(node,where);return true;}
        const ok=expected==='integer'?node.type==='number'&&Number.isInteger(node.value)&&/^-?\d+$/.test(text.substr(node.offset,node.length)):expected==='number'?node.type==='number':expected==='string'?node.type==='string'&&node.value.length>0:node.type===expected;
        if(!ok)at(node,`${where} must be ${expected==='integer'?'an integer':expected==='number'?'a number':expected==='string'?'a non-empty string':'a'+(/^[aeiou]/.test(expected)?'n ':' ')+expected}.`);
        return ok;
    };
    const root=props(tree);known(tree,['schema','kind','name','moves','forks'],'the file');
    if(!root.has('schema'))at(tree,'Missing "schema".');else if(type(root.get('schema'),'integer','schema')&&root.get('schema').value!==1)at(root.get('schema'),'schema must be 1.');
    if(!root.has('kind'))at(tree,'Missing "kind".');else if(root.get('kind').value!=='eclipse.moveset')at(root.get('kind'),'kind must be "eclipse.moveset".');
    for(const [list,isFork] of [['moves',false],['forks',true]]){
        const node=root.get(list);if(!node)continue;if(!type(node,'array',list))continue;
        node.children.forEach((entry,index)=>{
            const where=`${list}[${index}]`;if(!type(entry,'object',where))return;
            const map=props(entry);known(entry,isFork?FORK_FIELDS:MOVE_FIELDS,where);
            if(!map.has('move')){at(entry,`${where} needs "move".`);return;}
            if(!type(map.get('move'),'string',`${where}.move`))return;
            for(const [name,shape] of Object.entries(SHAPES)){
                const value=map.get(name);if(!value||!type(value,'object',`${where}.${name}`))continue;
                const inner=props(value);
                if(shape.guard){known(value,GUARD,`${where}.${name}`);for(const k of GUARD){if(!inner.has(k))at(value,`${where}.${name} needs "${k}".`);else type(inner.get(k),shape.guard,`${where}.${name}.${k}`);}}
                else{known(value,Object.keys(shape.fields),`${where}.${name}`);for(const k of shape.required)if(!inner.has(k))at(value,`${where}.${name} needs "${k}".`);for(const [k,t] of Object.entries(shape.fields))if(inner.has(k))type(inner.get(k),t,`${where}.${name}.${k}`);}
            }
            const additions=map.get('new_attacks');
            if(additions&&type(additions,'array',`${where}.new_attacks`)){
                if(additions.children.length>32)at(additions,`${where}.new_attacks adds more than 32 attacks.`);
                const REACTIONS=new Set(require('./native.cjs').HIT_REACTIONS??[]);
                const ids=new Set();
                additions.children.forEach((attack,i)=>{
                    const here=`${where}.new_attacks[${i}]`;if(!type(attack,'object',here))return;
                    const f=props(attack);known(attack,['id','start','end','damage','damage_terms','edges','impulse','hit'],here);
                    for(const k of ['id','start','end','damage','damage_terms','edges','impulse','hit'])if(!f.has(k))at(attack,`${here} needs "${k}".`);
                    for(const k of ['id','start','end'])if(f.has(k))type(f.get(k),'integer',`${here}.${k}`);
                    if(f.has('id')&&ids.has(f.get('id').value))at(f.get('id'),`${here}.id repeats another new attack.`);if(f.has('id'))ids.add(f.get('id').value);
                    if(f.has('start')&&f.has('end')&&f.get('end').value<f.get('start').value)at(f.get('end'),`${here}.end is before start.`);
                    if(f.has('damage')&&type(f.get('damage'),'number',`${here}.damage`)&&(f.get('damage').value<0||f.get('damage').value>16))at(f.get('damage'),`${here}.damage must be 0..16.`);
                    if(f.has('damage_terms')&&type(f.get('damage_terms'),'object',`${here}.damage_terms`)){
                        const terms=props(f.get('damage_terms'));
                        if(terms.size<1||terms.size>4)at(f.get('damage_terms'),`${here}.damage_terms needs 1-4 terms.`);
                        for(const [k,v] of terms){if(!['UnarmedDamage','WeaponDamage','RangedDamage','MagicDamage'].includes(k))at(v,`${here}.damage_terms: unknown type "${k}".`);else type(v,'number',`${here}.damage_terms.${k}`);}
                    }
                    if(f.has('edges')&&type(f.get('edges'),'array',`${here}.edges`)&&(!f.get('edges').children.length||f.get('edges').children.length>64))at(f.get('edges'),`${here}.edges needs 1-64 edge names.`);
                    if(f.has('impulse')&&type(f.get('impulse'),'array',`${here}.impulse`)&&f.get('impulse').children.length!==3)at(f.get('impulse'),`${here}.impulse must be [x, y, z].`);
                    if(f.has('hit')&&type(f.get('hit'),'string',`${here}.hit`)&&REACTIONS.size&&!REACTIONS.has(f.get('hit').value))at(f.get('hit'),`${here}.hit is not a hit reaction the game knows.`);
                });
            }
            const range=map.get('clip_range');
            if(range&&type(range,'object',`${where}.clip_range`)){
                const inner=props(range);known(range,GUARD,`${where}.clip_range`);
                const pairs={};
                for(const k of GUARD){
                    if(!inner.has(k)){at(range,`${where}.clip_range needs "${k}".`);continue;}
                    const pair=inner.get(k);
                    if(!type(pair,'array',`${where}.clip_range.${k}`))continue;
                    if(pair.children.length!==2){at(pair,`${where}.clip_range.${k} must be [first, last].`);continue;}
                    if(pair.children.every((n,i)=>type(n,'integer',`${where}.clip_range.${k}[${i}]`))){
                        const [first,last]=pair.children.map(n=>n.value);pairs[k]=[first,last];
                        if(first<0||last>100000)at(pair,`${where}.clip_range.${k} keyframes must be 0..100000.`);
                        else if(last<=first)at(pair,`${where}.clip_range.${k} must end after it starts.`);
                    }
                }
                if(pairs.expected&&pairs.value&&pairs.expected[0]===pairs.value[0]&&pairs.expected[1]===pairs.value[1])at(range,`${where}.clip_range must change the range.`);
            }
            const chains=map.get('chains');
            if(chains&&type(chains,'array',`${where}.chains`)){
                if(chains.children.length>16)at(chains,`${where}.chains lists more than 16 chains.`);
                const froms=new Set();
                chains.children.forEach((chain,i)=>{
                    const here=`${where}.chains[${i}]`;if(!type(chain,'object',here))return;
                    const f=props(chain);known(chain,['from','start','end'],here);
                    for(const k of ['from','start','end'])if(!f.has(k))at(chain,`${here} needs "${k}".`);
                    if(f.has('from')&&type(f.get('from'),'string',`${here}.from`)){if(froms.has(f.get('from').value))at(f.get('from'),`${here} follows the same move twice.`);froms.add(f.get('from').value);}
                    for(const k of ['start','end'])if(f.has(k))type(f.get(k),'integer',`${here}.${k}`);
                    if(f.has('start')&&f.has('end')&&(f.get('start').value<0||f.get('end').value<f.get('start').value||f.get('end').value>100000))at(chain,`${here} needs 0 <= start <= end <= 100000.`);
                });
            }
            if(isFork){
                for(const k of ['id'])if(!map.has(k))at(entry,`${where} needs "${k}".`);
                const adds=map.get('add')?.value===true;
                if(map.has('add')&&map.get('add').type!=='boolean')at(map.get('add'),`${where}.add must be true or false.`);
                if(map.has('subtype')&&map.has('item'))at(entry,`${where} takes at most one of "subtype" or "item".`);
                else if(!adds&&!map.has('subtype')&&!map.has('item'))at(entry,`${where} needs exactly one of "subtype" or "item" (or "add": true for a new move).`);
            }else if(map.size===1||(map.size===2&&map.has('note')))at(entry,`${where} must change at least one field.`);
            const table=lua(entry,text);normalizeImpulse(table);
            entries.push({kind:isFork?'fork':'move',table,offset:entry.offset,
                move:map.get('move').value,id:map.get('id')?.value,subtype:map.get('subtype')?.value,item:map.get('item')?.value,add:map.get('add')?.value===true});
        });
    }
    return {issues,entries};
}
// Vanilla checks for parsed entries. helpers: {literal, fields} from project.cjs.
function check(entries,helpers,modId){
    const issues=[],forks=new Map();
    const add=(node,_code,message)=>issues.push({offset:node?.range?.[0]??0,length:node?.range?(node.range[1]-node.range[0]):0,message});
    // A combo link follows a native move or one of this file's forks.
    const forkNames=new Set(entries.filter(e=>e.kind==='fork'&&e.id).map(e=>`${modId}.${e.id}`));
    for(const entry of entries)for(const chain of arrayValues(field(entry.table,'chains'))){
        const from=field(chain,'from');
        if(from?.type==='StringLiteral'&&!native.lookup(from.value)&&!forkNames.has(from.value))
            add(from,'native-guard',`No base-game move or fork in this file is named "${from.value}".`);
    }
    for(const entry of entries){
        const sourceName=forks.get(entry.move)??entry.move;
        // A new attack's id must not repeat one of the move's own attacks.
        const vanilla=native.lookup(sourceName),additions=field(entry.table,'new_attacks');
        if(vanilla&&additions)for(const attack of arrayValues(additions)){
            const id=field(attack,'id');
            if(id&&vanilla.attacks.some(a=>a.id===id.value))add(id,'native-guard',`"${sourceName}" already has attack id ${id.value}; give the new attack another id.`);
        }
        if(entry.kind==='fork'){
            // A new move ("add": true) keeps its source untouched, so fork lock rules do not apply.
            if(!entry.add)native.checkCall('sf2.moves.fork',[withMove(entry.table,sourceName)],{...helpers,fields:t=>{const f=helpers.fields(t);return {...f,source:f.move};}},add);
            if(entry.id)forks.set(`${modId}.${entry.id}`,sourceName);
            if(native.lookup(sourceName))native.checkCall('sf2.moves.patch',[withMove(entry.table,sourceName)],helpers,add);
        }else native.checkCall('sf2.moves.patch',[withMove(entry.table,sourceName)],helpers,add);
    }
    return issues;
}
// Re-point the entry's move field at the vanilla source for guard checks.
function withMove(table,name){
    return {...table,fields:table.fields.map(f=>f.type==='TableKeyString'&&f.key.name==='move'?{...f,value:{...f.value,value:name}}:f)};
}
module.exports={parse,check,field,arrayValues,MOVE_FIELDS,FORK_FIELDS};
