// Vanilla move knowledge for sf2.moves.* calls. data/native-moves.json is generated
// by Tools/Audits/QueryMoves.py from the same normalized XML and template
// inheritance the runtime loads. Guard checks mirror MoveCombatPatchRuntime and
// AnimationData.ReplaceExternalMoves; another enabled mod's replacement of the same
// move can still change what the runtime sees.
const data=require('../data/native-moves.json');
const FRAME_INTERVALS=['SemiUninterrupt','Uninterrupt','SelfUninterrupt','Unstable'];
const MOVE_FIELDS={'sf2.moves.patch':'move','sf2.moves.replace':'target','sf2.moves.extend_item_lock':'move','sf2.moves.remove_perk_lock':'move','sf2.moves.extend_perk_lock':'move','sf2.moves.fork':'source','sf2.moves.remove_item_lock':'move','sf2.moves.exclude_item':'move'};
const interval=i=>({name:i[0],type:i[1],start:i[2],end:i[3],hits:i.slice(4).map(h=>({name:h[0],start:h[1],end:h[2]}))});
function lookup(name){
    const record=Object.hasOwn(data.moves,name)?data.moves[name]:undefined;if(!record)return;
    return {name,file:record.file,priority:record.priority,midFrames:record.mid??0,looped:!!record.looped,physics:!!record.physics,type:record.type??'',templates:record.templates??[],legacy:!!record.legacy,
        attacks:(record.attacks??[]).map(a=>({id:a[0],damage:a[1],terms:a[2],edges:a[3],impulse:a[4]})),weapons:record.weapons??[],
        intervals:(record.intervals??[]).map(interval),sounds:(record.sounds??[]).map(s=>({name:s[0],frame:s[1]})),keys:(record.keys??[]).map(g=>g.map(k=>({type:k[0],press:k[1]})))};
}
const ambiguous=name=>data.duplicates.includes(name);
const range=(start,end)=>`${start}..${end<0?'open':end}`;
const intervalLabel=i=>(i.name??i.type??'?')+(i.name&&i.type?` (${i.type})`:'');
// One function per guarded field. Each returns an error message or nothing.
function checkPatch(move,definition,{literal,fields},add){
    const at=(key,sub)=>sub?fields(definition[key])[sub]??definition[key]:definition[key];
    const bounds=(startPatch,endPatch)=>{
        const name=literal(fields(startPatch??endPatch).name);if(typeof name!=='string')return;
        const key=startPatch?'interval_start':'interval_end';
        const found=move.intervals.filter(i=>i.name===name);
        if(!found.length)return add(at(key,'name'),'native-guard',`Vanilla "${move.name}" has no "${name}" interval; the runtime rejects this patch.`);
        if(found.length>1)return add(at(key,'name'),'native-guard',`Vanilla "${move.name}" has ${found.length} "${name}" intervals; the runtime rejects ambiguous interval patches.`);
        const target=found[0];
        for(const [patch,field,actual] of [[startPatch,'interval_start',target.start],[endPatch,'interval_end',target.end]]){
            const expected=literal(fields(patch).expected);
            if(patch&&typeof expected==='number'&&expected!==actual)add(at(field,'expected'),'native-guard',`Vanilla "${move.name}" ${name} ${field==='interval_start'?'starts':'ends'} at ${actual}, not ${expected}; the runtime rejects this patch.`);
        }
        const start=startPatch?literal(fields(startPatch).value):target.start,end=endPatch?literal(fields(endPatch).value):target.end;
        if(typeof start==='number'&&typeof end==='number'&&start>end)add(at(startPatch?'interval_start':'interval_end','value'),'native-guard',end<0&&!endPatch
            ?`Vanilla "${move.name}" ${name} has no End (read as -1), so the runtime rejects a later start; also patch interval_end.`
            :`${name} would run ${start}..${end}; start must not be after end.`);
    };
    const startPatch=definition.interval_start,endPatch=definition.interval_end;
    const startName=literal(fields(startPatch).name),endName=literal(fields(endPatch).name);
    if(startPatch&&endPatch&&startName!==endName){bounds(undefined,endPatch);bounds(startPatch,undefined);}
    else if(startPatch||endPatch)bounds(startPatch,endPatch);
    if(definition.hit){
        const expected=literal(fields(definition.hit).expected),attacks=move.intervals.filter(i=>i.type==='Attack');
        if(attacks.length!==1)add(definition.hit,'native-guard',`hit patches need exactly one attack interval; vanilla "${move.name}" has ${attacks.length}.`);
        else{const hits=attacks[0].hits;
            if(hits.length!==1||hits[0].start!=null||hits[0].end!=null)add(definition.hit,'native-guard',`hit patches need one full-interval reaction; vanilla "${move.name}" has ${hits.length===1?'a partial-interval reaction':hits.length+' reactions'}.`);
            else if(typeof expected==='string'&&hits[0].name!==expected)add(at('hit','expected'),'native-guard',`Vanilla "${move.name}" hits with "${hits[0].name}", not "${expected}"; the runtime rejects this patch.`);}
    }
    if(definition.sound_frame){
        const name=literal(fields(definition.sound_frame).name),expected=literal(fields(definition.sound_frame).expected);
        if(typeof name==='string'){const found=move.sounds.filter(s=>s.name===name);
            if(!found.length)add(at('sound_frame','name'),'native-guard',`Vanilla "${move.name}" has no direct "${name}" sound action.`);
            else if(found.length>1)add(at('sound_frame','name'),'native-guard',`Vanilla "${move.name}" plays "${name}" ${found.length} times; the runtime rejects ambiguous sound patches.`);
            else if(found[0].frame==null)add(at('sound_frame','name'),'native-guard',`"${name}" in "${move.name}" is event-driven and has no frame to patch.`);
            else if(typeof expected==='number'&&found[0].frame!==expected)add(at('sound_frame','expected'),'native-guard',`Vanilla "${move.name}" plays "${name}" at frame ${found[0].frame}, not ${expected}; the runtime rejects this patch.`);}
    }
    if(definition.input){
        const expected=literal(fields(definition.input).expected);
        if(move.keys.length!==1)add(definition.input,'native-guard',`input patches need exactly one top-level key condition; vanilla "${move.name}" has ${move.keys.length}.`);
        else if(typeof expected==='string'&&!(move.keys[0].length===1&&move.keys[0][0].type===expected))add(at('input','expected'),'native-guard',`Vanilla "${move.name}" uses ${move.keys[0].map(k=>k.type).join(' + ')||'no key'}, not "${expected}"; the runtime rejects this patch.`);
    }
    if(definition.priority){const expected=literal(fields(definition.priority).expected);
        if(typeof expected==='number'&&expected!==move.priority)add(at('priority','expected'),'native-guard',`Vanilla "${move.name}" has priority ${move.priority}, not ${expected}; the runtime rejects this patch.`);}
    if(definition.animation){const expected=literal(fields(definition.animation).expected);
        if(typeof expected==='string'&&expected!==move.file)add(at('animation','expected'),'native-guard',`Vanilla "${move.name}" plays "${move.file}", not "${expected}"; the runtime rejects this patch.`);}
    if(definition.remove_interval){
        const f=fields(definition.remove_interval),name=literal(f.name),type=literal(f.type),start=literal(f.start),end=literal(f['end']);
        if(typeof name==='string'&&typeof type==='string'){
            const found=move.intervals.filter(i=>i.name===name&&(i.type||i.name)===type);
            if(!found.length)add(definition.remove_interval,'native-guard',`Vanilla "${move.name}" has no "${name}" interval of type ${type}.`);
            else if(found.length>1)add(definition.remove_interval,'native-guard',`Vanilla "${move.name}" has ${found.length} matching "${name}" intervals; the runtime rejects ambiguous removals.`);
            else if((typeof start==='number'&&start!==found[0].start)||(typeof end==='number'&&end!==found[0].end))add(definition.remove_interval,'native-guard',`Vanilla "${move.name}" ${name} runs ${found[0].start}..${found[0].end}; start and end must match exactly.`);
        }
    }
    if(definition.playback_rate){
        const f=fields(definition.playback_rate),expected=literal(f.expected),value=literal(f.value);
        if(move.looped||move.physics)add(definition.playback_rate,'native-guard',`playback_rate is unavailable for ${move.looped?'looped':'physics'} moves such as "${move.name}".`);
        else if(typeof expected==='number'&&Math.round(expected*1000)!==1000)add(f.expected,'native-guard',`Vanilla "${move.name}" plays at 1.0, not ${expected}; the runtime rejects this patch.`);
        else if(typeof value==='number'){
            const limit=Math.min(2,move.midFrames+1),permille=Math.round(value*1000);
            if(permille<500||permille>limit*1000)add(f.value,'native-guard',`playback_rate for "${move.name}" must be 0.5..${limit}`+(limit<2?` (MidFrames ${move.midFrames} allows at most ${limit}).`:'.'));
        }
    }
    checkIntervalList(move,definition.intervals,{literal,fields},add);
    checkAttackList(move,definition.attacks,{literal,fields},add);
    if(definition.add_interval){const name=literal(fields(definition.add_interval).name);
        if(!move.intervals.length)add(definition.add_interval,'native-guard',`add_interval needs a move with native intervals; vanilla "${move.name}" has none.`);
        else if(typeof name==='string'&&move.intervals.some(i=>i.name===name))add(at('add_interval','name'),'native-guard',`Vanilla "${move.name}" already has an interval named "${name}"; patch its bounds instead.`);}
}
const arrayItems=node=>node?.type==='TableConstructorExpression'?node.fields.filter(f=>f.type==='TableValue').map(f=>f.value):[];
// Selector semantics mirror MoveCombatPatchRuntime.SelectInterval: exact authored identity,
// an omitted end means an open-ended interval.
function selectInterval(move,select,{literal,fields}){
    const f=fields(select),type=literal(f.type)??'',name=literal(f.name)??'',start=literal(f.start)??0,end=f['end']===undefined?-1:literal(f['end']);
    if([type,name,start,end].some(v=>v===undefined))return {computed:true};
    const found=move.intervals.filter(i=>i.type!=='Attack'&&(i.type??'')===type&&(i.name??'')===name&&i.start===start&&i.end===end);
    return {found,label:`${type||name}${type&&name?'/'+name:''} ${start}..${end<0?'open':end}`};
}
function checkIntervalList(move,node,helpers,add){
    for(const item of arrayItems(node)){
        const f=helpers.fields(item);
        if(f.add){const name=helpers.literal(helpers.fields(f.add).name);
            if(!move.intervals.length)add(f.add,'native-guard',`intervals.add needs a move with native intervals; vanilla "${move.name}" has none.`);
            else if(typeof name==='string'&&move.intervals.some(i=>i.name===name))add(f.add,'native-guard',`Vanilla "${move.name}" already has an interval named "${name}".`);
            continue;}
        if(!f.select)continue;
        const result=selectInterval(move,f.select,helpers);if(result.computed)continue;
        if(!result.found.length){
            const near=move.intervals.filter(i=>i.type!=='Attack').map(intervalLabel2).join(', ');
            add(f.select,'native-guard',`No vanilla "${move.name}" interval matches ${result.label}. Its intervals: ${near||'none'}.`);
        }else if(result.found.length>1)add(f.select,'native-guard',`${result.found.length} vanilla "${move.name}" intervals match ${result.label}; the runtime rejects ambiguous selectors.`);
    }
}
const intervalLabel2=i=>`${i.type||i.name}${i.type&&i.name?'/'+i.name:''} ${i.start}..${i.end<0?'open':i.end}`;
function checkAttackList(move,node,{literal,fields},add){
    const same=(a,b)=>Math.fround(a)===Math.fround(b);
    for(const item of arrayItems(node)){
        const f=fields(item),id=literal(f.id);if(typeof id!=='number')continue;
        const attack=move.attacks.find(a=>a.id===id);
        if(!attack){add(f.id,'native-guard',`Vanilla "${move.name}" has no attack with id ${id}${move.attacks.length?` (ids: ${move.attacks.map(a=>a.id).join(', ')})`:''}.`);continue;}
        const interval=move.intervals.filter(i=>i.type==='Attack')[move.attacks.indexOf(attack)];
        const guard=key=>fields(f[key]).expected;
        const expect=(key,ok,actual)=>{if(f[key]&&!ok)add(guard(key)??f[key],'native-guard',`Vanilla "${move.name}" attack ${id} has ${key} ${actual}; the runtime rejects this patch.`);};
        const start=literal(guard('start')),end=literal(guard('end')),damage=literal(guard('damage'));
        if(typeof start==='number')expect('start',start===interval.start,interval.start);
        if(typeof end==='number')expect('end',end===interval.end,interval.end<0?'open':interval.end);
        if(typeof damage==='number'&&attack.damage!=null)expect('damage',same(damage,attack.damage),attack.damage);
        if(f.damage_terms){const map=fields(guard('damage_terms')),keys=Object.keys(map);
            if(keys.length&&keys.every(k=>typeof literal(map[k])==='number')){
                const ok=keys.length===Object.keys(attack.terms).length&&keys.every(k=>k in attack.terms&&same(literal(map[k]),attack.terms[k]));
                expect('damage_terms',ok,Object.entries(attack.terms).map(([k,v])=>`${k} ${v}`).join(', ')||'none');}}
        if(f.edges){const list=arrayItems(guard('edges')).map(literal);
            if(list.length&&list.every(v=>typeof v==='string'))expect('edges',list.length===attack.edges.length&&list.every((v,i)=>v===attack.edges[i]),attack.edges.join(', '));}
        if(f.impulse){const m=fields(guard('impulse')),xyz=['x','y','z'].map(k=>m[k]===undefined?0:literal(m[k]));
            if(xyz.every(v=>typeof v==='number'))expect('impulse',xyz.every((v,i)=>same(v,attack.impulse[i])),`(${attack.impulse.join(', ')})`);}
        if(f.hit){const expected=literal(guard('hit')),hits=interval.hits;
            if(typeof expected==='string')expect('hit',hits.length===1&&hits[0].name===expected&&hits[0].start==null&&hits[0].end==null,hits.map(h=>h.name).join(', ')||'no reaction');}
    }
}
// Diagnostics for a resolved sf2.moves.* call. helpers: {literal, fields}.
function checkCall(name,args,helpers,add){
    const field=MOVE_FIELDS[name];if(!field)return;
    const definition=helpers.fields(args[0]),node=definition[field],target=helpers.literal(node);
    if(typeof target!=='string')return;
    // The mod's own forks ("<mod id>.<id>") are copies, not vanilla names.
    if(helpers.forkPrefix&&target.startsWith(helpers.forkPrefix))return;
    const move=lookup(target);
    if(!move)return add(node,'native-move',`No vanilla move is named "${target}". ${name} targets must be exact native move names; check spelling and case.`);
    if(ambiguous(target))return add(node,'native-move',`Vanilla data defines "${target}" more than once; the runtime rejects ambiguous targets.`);
    if(name==='sf2.moves.replace'){const expected=helpers.literal(definition.expected_file);
        if(typeof expected==='string'&&expected!==move.file)add(definition.expected_file,'native-guard',`Vanilla "${target}" plays "${move.file}", not "${expected}"; the runtime rejects this replacement.`);}
    if(name==='sf2.moves.patch')checkPatch(move,definition,helpers,add);
    if(name==='sf2.moves.fork'||name==='sf2.moves.remove_item_lock'){
        const subtype=helpers.literal(definition.subtype),groups=move.weapons;
        if(typeof subtype==='string'&&(name==='sf2.moves.fork'||helpers.literal(definition.item_type)==='Weapon')){
            const group=groups.find(g=>g.includes(subtype));
            if(!group)add(definition.subtype,'native-guard',`Vanilla "${target}" is not locked to subtype "${subtype}"`+(groups.length?` (its groups: ${groups.map(g=>g.join('|')).join('; ')}).`:'; it has no weapon lock, so fork it for an item instead.'));
            else if(group.length===1)add(definition.subtype,'native-guard',`"${subtype}" is the only subtype in "${target}"'s lock; removing it would unlock the move for every fighter.`);
        }
    }
}
function describe(name){
    const move=lookup(name);if(!move)return;
    const lines=[`**${move.name}** — vanilla move${ambiguous(name)?' (defined more than once)':''}`,'',
        `- Animation: \`${move.file||'none'}\``,`- Priority: ${move.priority}`+(move.type?` · Type: ${move.type}`:'')];
    if(move.templates.length)lines.push(`- Templates: ${move.templates.join(' › ')}`);
    for(const group of move.keys)lines.push(`- Keys: ${group.map(k=>k.type+(k.press?` (${k.press})`:'')).join(' + ')}`);
    for(const i of move.intervals)lines.push(`- Interval ${intervalLabel2(i)}`+(i.hits.length?` · hit ${i.hits.map(h=>h.name).join(', ')}`:''));
    for(const a of move.attacks)lines.push(`- Attack id ${a.id}: damage ${a.damage}`+(Object.keys(a.terms).length?` (${Object.entries(a.terms).map(([k,v])=>`${k} ${v}`).join(', ')})`:'')+` · edges ${a.edges.join(', ')}`);
    lines.push(`- MidFrames ${move.midFrames}`+(move.looped?' · looped':'')+(move.physics?' · physics':''));
    for(const s of move.sounds.filter(s=>s.frame!=null))lines.push(`- Sound \`${s.name}\` at frame ${s.frame}`);
    if(move.legacy)lines.push('','Restored from the legacy move table.');
    return lines.join('\n');
}
// Completion values for a string at `path` (field keys from the call's table) in `call`.
function completions(call,path,definition,literal){
    const field=MOVE_FIELDS[call];if(!field)return [];
    const targetName=literal(definition?.[field]),move=typeof targetName==='string'?lookup(targetName):undefined;
    if(path.length===1&&path[0]===field)return Object.keys(data.moves).map(name=>({label:name,detail:data.moves[name].file||'vanilla move',documentation:describe(name)}));
    if(path.length===1&&path[0]==='expected_file')return move?[{label:move.file,detail:`animation of ${move.name}`}]:[];
    if(path[0]==='core_templates')return data.templates.map(name=>({label:name,detail:'native template'}));
    if(!move)return [];
    if(['interval_start','interval_end'].includes(path[0])&&path[1]==='name')return move.intervals.filter(i=>FRAME_INTERVALS.includes(i.name)).map(i=>({label:i.name,detail:range(i.start,i.end)}));
    if(path[0]==='remove_interval'&&path[1]==='name')return move.intervals.filter(i=>i.name).map(i=>({label:i.name,detail:`${i.type??i.name} ${range(i.start,i.end)}`}));
    if(path[0]==='sound_frame'&&path[1]==='name')return move.sounds.filter(s=>s.frame!=null).map(s=>({label:s.name,detail:`frame ${s.frame}`}));
    if(path[0]==='intervals'&&path[1]==='select'&&(path[2]==='name'||path[2]==='type'))
        return [...new Set(move.intervals.filter(i=>i.type!=='Attack').map(i=>(path[2]==='name'?i.name:i.type)).filter(Boolean))].map(v=>({label:v,detail:'native interval '+path[2]}));
    if(['hit','animation','input'].includes(path[0])&&path[1]==='expected'){
        if(path[0]==='animation')return [{label:move.file,detail:'current animation'}];
        if(path[0]==='input')return move.keys.length===1?move.keys[0].map(k=>({label:k.type,detail:'current key'})):[];
        const attacks=move.intervals.filter(i=>i.type==='Attack');return attacks.length===1?attacks[0].hits.map(h=>({label:h.name,detail:'current reaction'})):[];
    }
    return [];
}
module.exports={data,lookup,ambiguous,checkCall,describe,completions,MOVE_FIELDS,intervalLabel,range};
