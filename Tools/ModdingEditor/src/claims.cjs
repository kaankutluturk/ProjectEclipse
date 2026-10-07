// Exclusive content claims made with literal arguments, keyed like the runtime's
// ownership checks: one sf2.moves.patch owner per native move, one sf2.moves.replace
// owner per target, one sf2.assets.replace owner per target asset, and one
// sf2.localization.patch / sf2.battles.patch owner per semantic field. Two mods with
// the same key cannot both load: whichever registers second fails as a whole.
// Claims built from computed values are not indexed.
const path=require('node:path');
const normalize=reference=>{
    if(typeof reference!=='string')return null;const colon=reference.indexOf(':');
    if(colon<=0||colon!==reference.lastIndexOf(':'))return null;
    return reference.slice(0,colon)+':'+reference.slice(colon+1).replaceAll('\\','/').toLowerCase();
};
const CLAIMS={
    'sf2.moves.patch':(d,literal)=>{const move=literal(d.move);return typeof move==='string'&&{kind:'move patch',key:'move-patch:'+move,label:move,node:d.move};},
    'sf2.moves.replace':(d,literal)=>{const target=literal(d.target);return typeof target==='string'&&{kind:'move replacement',key:'move-replace:'+target,label:target,node:d.target};},
    'sf2.assets.replace':(d,literal)=>{const target=normalize(literal(d.target));return target&&{kind:'asset replacement',key:'asset-replace:'+target,label:target,node:d.target};},
    'sf2.localization.patch':(d,literal)=>{const target=normalize(literal(d.target)),language=literal(d.language);
        return target&&typeof language==='string'&&language.trim()&&{kind:'localization patch',key:`localization:${target}|${language.trim().toLowerCase()}`,label:`${target} (${language.trim().toLowerCase()})`,node:d.target};},
    'sf2.moves.fork':(d,literal)=>{const source=literal(d.source??d.move),subtype=literal(d.subtype),item=normalize(literal(d.item));
        if(typeof source!=='string')return null;
        if(typeof subtype==='string')return {kind:'subtype fork',key:`lock-removal:${source}|Weapon|${subtype}`,label:`${source} for ${subtype}`,node:d.subtype};
        return item&&{kind:'item fork',key:`lock-exclusion:${source}|${item}`,label:`${source} for ${item}`,node:d.item};},
    'sf2.moves.remove_item_lock':(d,literal)=>{const move=literal(d.move),type=literal(d.item_type),subtype=literal(d.subtype);
        return [move,type,subtype].every(v=>typeof v==='string')&&{kind:'item lock removal',key:`lock-removal:${move}|${type}|${subtype}`,label:`${move} for ${subtype}`,node:d.subtype};},
    'sf2.moves.exclude_item':(d,literal)=>{const move=literal(d.move),item=normalize(literal(d.item));
        return typeof move==='string'&&item&&{kind:'item exclusion',key:`lock-exclusion:${move}|${item}`,label:`${move} for ${item}`,node:d.item};},
    'sf2.battles.patch':(d,literal)=>{const target=normalize(literal(d.target));return target&&{kind:'battle position patch',key:'battle-position:'+target,label:target,node:d.target};},
};
function claimOf(call,{literal,fields}){const make=CLAIMS[call.name];return make?make(fields(call.args[0]),literal)||null:null;}
// mods: indexed mods with .claims. Returns per-claim conflicts and duplicate mod IDs.
function conflicts(mods){
    const owners=new Map(),byId=new Map(),result=[];
    for(const mod of mods){
        if(typeof mod.data.id==='string'){const roots=byId.get(mod.data.id)??[];roots.push(mod);byId.set(mod.data.id,roots);}
        for(const claim of mod.claims??[]){const list=owners.get(claim.key)??[];list.push({mod,claim});owners.set(claim.key,list);}
    }
    for(const list of owners.values()){
        for(const entry of list){
            // Copies of one mod ID cannot be installed together, so they never conflict.
            const others=list.filter(o=>o.mod.data.id!==entry.mod.data.id),same=list.filter(o=>o!==entry&&o.mod.root===entry.mod.root);
            if(others.length)result.push({...entry,others,duplicate:false});
            else if(same.length)result.push({...entry,others:same,duplicate:true});
        }
    }
    // Only sibling folders share a mods root, where discovery rejects duplicate IDs.
    const duplicateIds=[];
    for(const [id,roots] of byId)for(const mod of roots){const siblings=roots.filter(o=>o!==mod&&path.dirname(o.root)===path.dirname(mod.root));if(siblings.length)duplicateIds.push({id,mod,others:siblings.map(o=>o.root)});}
    return {claims:result,duplicateIds};
}
function message(conflict,relative=f=>f){
    const where=o=>`${relative(o.claim.file)}${o.claim.line!=null?':'+(o.claim.line+1):''}`;
    if(conflict.duplicate)return `This mod claims ${conflict.claim.kind} "${conflict.claim.label}" more than once (also ${conflict.others.map(where).join(', ')}); registration fails.`;
    return `${conflict.claim.kind[0].toUpperCase()+conflict.claim.kind.slice(1)} "${conflict.claim.label}" is also claimed by ${conflict.others.map(o=>`"${o.mod.data.id}" (${where(o)})`).join(', ')}. With both enabled, whichever mod registers second fails to load.`;
}
module.exports={claimOf,conflicts,message,normalize,CLAIMS};
