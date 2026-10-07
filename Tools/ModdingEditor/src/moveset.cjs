// movesets/*.json support. Parses with positions, mirrors the strict C# reader
// (ModMovesetJson), and converts each entry into the luaparse-shaped table nodes that
// native.cjs, claims.cjs and report.cjs already understand, so data files get the same
// vanilla guard checks, conflict claims and move-change report as sf2.moves.patch.
const jsonc=require('jsonc-parser');
const native=require('./native.cjs');
const MOVE_FIELDS=['move','note','disable','priority','playback_rate','animation','input','sound_frame','intervals','attacks'];
const FORK_FIELDS=[...MOVE_FIELDS,'id','subtype','item'];
const GUARD=['expected','value'];
const SHAPES={
    priority:{guard:'integer'},playback_rate:{guard:'number'},input:{guard:'string'},
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
    const type=(node,expected,where)=>{
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
            if(isFork){
                for(const k of ['id'])if(!map.has(k))at(entry,`${where} needs "${k}".`);
                if(map.has('subtype')===map.has('item'))at(entry,`${where} needs exactly one of "subtype" or "item".`);
            }else if(map.size===1||(map.size===2&&map.has('note')))at(entry,`${where} must change at least one field.`);
            const table=lua(entry,text);normalizeImpulse(table);
            entries.push({kind:isFork?'fork':'move',table,offset:entry.offset,
                move:map.get('move').value,id:map.get('id')?.value,subtype:map.get('subtype')?.value,item:map.get('item')?.value});
        });
    }
    return {issues,entries};
}
// Vanilla checks for parsed entries. helpers: {literal, fields} from project.cjs.
function check(entries,helpers,modId){
    const issues=[],forks=new Map();
    const add=(node,_code,message)=>issues.push({offset:node?.range?.[0]??0,length:node?.range?(node.range[1]-node.range[0]):0,message});
    for(const entry of entries){
        const sourceName=forks.get(entry.move)??entry.move;
        if(entry.kind==='fork'){
            native.checkCall('sf2.moves.fork',[withMove(entry.table,sourceName)],{...helpers,fields:t=>{const f=helpers.fields(t);return {...f,source:f.move};}},add);
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
