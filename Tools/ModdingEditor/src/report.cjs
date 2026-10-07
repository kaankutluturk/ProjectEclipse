// Markdown report of what a mod changes in vanilla moves: each sf2.moves.patch,
// replace and lock call beside the vanilla values it guards or discards, plus
// exclusive claims that conflict with other indexed mods. Static: only literal
// arguments resolve, and runtime order or other mods' replacements are not modeled.
const path=require('node:path');
const native=require('./native.cjs');
const claims=require('./claims.cjs');
const cell=value=>String(value).replaceAll('|','\\|').replaceAll('\n',' ');
const code=value=>'`'+String(value).replaceAll('`','\'')+'`';
function moveReport(mod,{literal,fields},others=[]){
    const relative=file=>path.relative(mod.root,file).replaceAll('\\','/');
    const where=call=>`${relative(call.file)}${call.line!=null?':'+(call.line+1):''}`;
    const source=(call,node)=>node?.range?(mod.sources.get(call.file)??'').slice(node.range[0],node.range[1]):undefined;
    const value=(call,node)=>{const v=literal(node);return v!==undefined?(typeof v==='string'?code(v):String(v)):node?code(source(call,node)):'—';};
    const lines=[`# Move changes: ${mod.data.name??mod.data.id}`,'',
        `Static report for \`${mod.data.id}\`, generated from vanilla move data and literal Lua arguments. Computed values show their source text. Run the game to verify gameplay.`,''];
    const byMove=new Map(),computed=[];
    for(const call of mod.moveCalls??[]){
        const field=native.MOVE_FIELDS[call.name],target=call.forkOf??literal(fields(call.args[0])[field]);
        if(typeof target!=='string'){computed.push(call);continue;}
        const list=byMove.get(target)??[];list.push(call);byMove.set(target,list);
    }
    if(!byMove.size&&!computed.length)lines.push('This mod does not patch, replace or relock vanilla moves.','');
    for(const [name,calls] of [...byMove.entries()].sort(([a],[b])=>a.localeCompare(b))){
        const move=native.lookup(name);
        lines.push(`## ${name}`,'');
        if(!move){lines.push(`⚠ No vanilla move is named \`${name}\`; these calls fail at runtime: ${calls.map(where).join(', ')}.`,'');continue;}
        lines.push(`Vanilla: ${code(move.file||'no animation')}, priority ${move.priority}${move.templates.length?`, templates ${move.templates.join(' › ')}`:''}.`,'');
        for(const call of calls){
            const d=fields(call.args[0]),rows=[],problems=[];
            if(call.forkOf){const copy=literal(fields(call.args[0]).move);lines.push(`**Edits of the copy \`${copy}\`** — ${where(call)}`,'');}
            else{native.checkCall(call.name,call.args,{literal,fields},(_node,_code,message)=>problems.push(message));lines.push(`**${call.name}** — ${where(call)}`,'');}
            if(call.name==='sf2.moves.fork'){const subtype=literal(d.subtype),item=literal(d.item),group=(move.weapons.find(g=>g.includes(subtype))??[]).join(' | ');
                rows.push(subtype?['Who gets it',group||'(no weapon lock)',`original: ${group.split(' | ').filter(x=>x!==subtype).join(' | ')||'nobody'} · copy: **${subtype}**`]:['Who gets it',move.weapons.map(g=>g.join(' | ')).join('; ')||'every weapon',`copy only for **${item??value(call,d.item)}**; original for the rest`]);}
            if(call.name==='sf2.moves.patch'||call.forkOf){
                if(literal(d.disable)===true)rows.push(['Selectable','yes','**no** (disabled)']);
                const added=d.conditions?.fields?.length;if(added)rows.push(['Selection conditions',`${move.keys.length} key group(s) and template conditions`,`+${added} added condition(s)`]);
                for(const key of ['interval_start','interval_end']){if(!d[key])continue;const f=fields(d[key]),n=literal(f.name),found=move.intervals.filter(i=>i.name===n);
                    rows.push([`${n??'?'} ${key==='interval_start'?'start':'end'}`,found.length===1?String(key==='interval_start'?found[0].start:found[0].end):'?',value(call,f.value)]);}
                if(d.hit){const attacks=move.intervals.filter(i=>i.type==='Attack');rows.push(['Hit reaction',attacks.length===1?attacks[0].hits.map(h=>h.name).join(', ')||'none':'?',value(call,fields(d.hit).value)]);}
                if(d.sound_frame){const f=fields(d.sound_frame),n=literal(f.name),found=move.sounds.filter(s=>s.name===n);rows.push([`Sound ${n??'?'} frame`,found.length===1?String(found[0].frame??'event'):'?',value(call,f.value)]);}
                if(d.input)rows.push(['Input key',move.keys.map(g=>g.map(k=>k.type).join(' + ')).join('; ')||'none',value(call,fields(d.input).value)]);
                if(d.priority)rows.push(['Priority',String(move.priority),value(call,fields(d.priority).value)]);
                if(d.animation)rows.push(['Animation',code(move.file),value(call,fields(d.animation).value)]);
                if(d.remove_interval){const f=fields(d.remove_interval);rows.push([`Interval ${literal(f.name)??'?'} (${literal(f.type)??'?'})`,`${literal(f.start)}..${literal(f['end'])}`,'**removed**']);}
                if(d.add_interval){const f=fields(d.add_interval);rows.push([`Interval ${literal(f.name)??'?'}`,'absent',`${literal(f.start)}..${literal(f['end'])} (added)`]);}
                if(d.playback_rate)rows.push(['Speed','1.0',`${value(call,fields(d.playback_rate).value)}×`]);
                for(const item of (d.intervals?.fields??[]).map(x=>x.value)){
                    const f=fields(item),sel=fields(f.select),label=`${literal(sel.type)??''}${literal(sel.type)&&literal(sel.name)?'/':''}${literal(sel.name)??''}`;
                    const span=(start,end)=>`${start??0}..${end===undefined?'open':end}`;
                    if(f.add){const a=fields(f.add);rows.push([`Interval ${literal(a.type)??''}${literal(a.type)&&literal(a.name)?'/':''}${literal(a.name)??''}`,'absent',`${span(literal(a.start),a['end']===undefined?undefined:literal(a['end']))} (added)`]);}
                    else if(literal(f.remove)===true)rows.push([`Interval ${label}`,span(literal(sel.start),sel['end']===undefined?undefined:literal(sel['end'])),'**removed**']);
                    else rows.push([`Interval ${label}`,span(literal(sel.start),sel['end']===undefined?undefined:literal(sel['end'])),
                        span(f.start?literal(f.start):literal(sel.start),f['end']?literal(f['end']):(sel['end']===undefined?undefined:literal(sel['end'])))]);
                }
                for(const item of (d.attacks?.fields??[]).map(x=>x.value)){
                    const f=fields(item),id=literal(f.id);
                    for(const key of ['start','end','damage','damage_terms','edges','impulse','hit']){
                        if(!f[key])continue;const g=fields(f[key]);
                        rows.push([`Attack ${id} ${key.replace('_',' ')}`,source(call,g.expected)??'?',source(call,g.value)??'?'].map((v,i)=>i===0?v:code(v)));
                    }
                }
            }else if(call.name==='sf2.moves.replace'){
                const omitted=label=>`**omitted — not inherited**${label?' ('+label+')':''}`;
                rows.push(['Animation',code(move.file),value(call,d.animation)]);
                rows.push(['Priority',String(move.priority),d.priority?value(call,d.priority):omitted('0')]);
                rows.push(['Templates',move.templates.join(' › ')||'none',d.core_templates?value(call,d.core_templates):omitted()]);
                rows.push(['Intervals',move.intervals.map(native.intervalLabel).join(', ')||'none',d.intervals?`${d.intervals.fields?.length??'?'} replacement interval(s)`:omitted()]);
                rows.push(['Key conditions',move.keys.map(g=>g.map(k=>k.type).join(' + ')).join('; ')||'none',d.conditions?`${d.conditions.fields?.length??'?'} replacement condition(s)`:omitted()]);
                rows.push(['Scheduled sounds',String(move.sounds.filter(s=>s.frame!=null).length),d.timeline?'replacement timeline':omitted()]);
            }else if(call.name==='sf2.moves.extend_item_lock')rows.push(['Item lock',`${value(call,d.item_type)} ${value(call,d.source_subtype)}`,`also ${value(call,d.subtype)}`]);
            else if(call.name==='sf2.moves.remove_perk_lock')rows.push(['Perk lock',value(call,d.perk),'**removed**']);
            else if(call.name==='sf2.moves.extend_perk_lock')rows.push(['Perk lock',value(call,d.source_perk),`also ${value(call,d.perk)}`]);
            if(rows.length)lines.push('| Field | Vanilla | After this mod |','| --- | --- | --- |',...rows.map(r=>`| ${r.map(cell).join(' | ')} |`),'');
            for(const problem of problems)lines.push(`- ⚠ ${problem}`);
            if(problems.length)lines.push('');
        }
    }
    if(computed.length)lines.push('## Computed targets','',`${computed.length} call(s) build their move name at runtime and are not shown: ${computed.map(where).join(', ')}.`,'');
    const result=claims.conflicts([mod,...others]),mine=result.claims.filter(c=>c.mod===mod);
    lines.push('## Exclusive claims','');
    if(!(mod.claims??[]).length)lines.push('No literal patch or replacement claims.','');
    else{lines.push('| Claim | Target | Location |','| --- | --- | --- |',...mod.claims.map(c=>`| ${c.kind} | ${cell(code(c.label))} | ${relative(c.file)}${c.line!=null?':'+(c.line+1):''} |`),'');}
    if(mine.length)lines.push(...mine.map(c=>`- ⚠ ${claims.message(c,file=>path.relative(path.dirname(mod.root),file).replaceAll('\\','/'))}`),'');
    else lines.push(others.length?`No conflicts with ${others.length} other indexed mod(s).`:'No other mods were indexed, so cross-mod conflicts were not checked.','');
    return lines.join('\n');
}
module.exports={moveReport};
