const fs=require('node:fs/promises'),path=require('node:path'),lua=require('luaparse');
const api=require('../data/api.json');
const native=require('./native.cjs'),claims=require('./claims.cjs'),moveset=require('./moveset.cjs');
const MAX_FILES=10000;
const MANIFEST_FIELDS=['schema','id','name','version','authors','entrypoint','capabilities'];
const slash=s=>s.replaceAll('\\','/');
const safe=s=>typeof s==='string'&&s.length>0&&!path.isAbsolute(s)&&!s.includes(':')&&!slash(s).split('/').some(p=>!p||p==='.'||p==='..');
function stripComment(line){let quoted=false,escape=false;for(let i=0;i<line.length;i++){const c=line[i];if(escape){escape=false;continue;}if(c==='\\'&&quoted){escape=true;continue;}if(c==='"')quoted=!quoted;if(c==='#'&&!quoted)return line.slice(0,i);}return line;}
function parseValue(raw){
    if(/^"(?:[^"\\]|\\.)*"$/.test(raw))return JSON.parse(raw);
    if(/^-?\d+$/.test(raw))return Number(raw);
    if(raw==='true'||raw==='false')return raw==='true';
    if(raw.startsWith('[')&&raw.endsWith(']')){const v=JSON.parse(raw.replace(/,\s*\]$/,']'));if(Array.isArray(v)&&v.every(x=>typeof x==='string'))return v;}
    throw new Error('Use a quoted string, whole number, boolean, or one-line string array.');
}
function manifest(text){
    const data={dependencies:[]},issues=[],positions={};let target=data;
    text.split(/\r?\n/).forEach((line,lineNo)=>{
        const clean=stripComment(line).trim();if(!clean)return;
        if(clean==='[[dependencies]]'){target={};data.dependencies.push(target);return;}
        const match=/^([a-z_]+)\s*=\s*(.+)$/.exec(clean);
        if(!match){issues.push({line:lineNo,message:'Expected a supported key = value or [[dependencies]] section.'});return;}
        const [,key,raw]=match;positions[key]??=lineNo;
        const allowed=target===data?MANIFEST_FIELDS:['id','version'];
        if(!allowed.includes(key)){issues.push({line:lineNo,message:`Unknown ${target===data?'manifest':'dependency'} field: ${key}.`});return;}
        if(Object.hasOwn(target,key)){issues.push({line:lineNo,message:`Duplicate field: ${key}.`});return;}
        try{target[key]=parseValue(raw);}catch(e){issues.push({line:lineNo,message:e.message});}
    });
    const issue=(key,message)=>issues.push({line:positions[key]??0,message});
    for(const key of MANIFEST_FIELDS)if(key!=='entrypoint'&&data[key]===undefined)issue(key,`Missing required manifest field: ${key}.`);
    if(data.schema!==1)issue('schema','schema must be 1.');
    if(typeof data.id!=='string'||!/^[-a-z0-9_.]+$/.test(data.id)||['core','sf2de'].includes(data.id))issue('id','Choose a unique lowercase mod ID; core and sf2de are reserved.');
    if(typeof data.name!=='string'||!data.name.trim())issue('name','name must be a nonempty string.');
    if(typeof data.version!=='string'||!/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$/.test(data.version))issue('version','Use a semantic version such as 1.0.0.');
    for(const key of ['authors','capabilities'])if(!Array.isArray(data[key])||!data[key].every(v=>typeof v==='string'&&v.trim())||(key==='authors'&&!data[key].length))issue(key,`${key} must be a ${key==='authors'?'nonempty ':''}one-line array of nonempty strings.`);else if(new Set(data[key]).size!==data[key].length)issue(key,`${key} contains duplicates.`);
    // entrypoint is optional: a data-only mod ships localizations and movesets without Lua.
    if(data.entrypoint!==undefined&&(!safe(data.entrypoint)||!slash(data.entrypoint).startsWith('scripts/')||!data.entrypoint.endsWith('.lua')))issue('entrypoint','Use a safe path inside scripts/, ending in .lua.');
    const ids=new Set();for(const d of data.dependencies){if(typeof d.id!=='string'||!/^[-a-z0-9_.]+$/.test(d.id)||typeof d.version!=='string')issue('id','Each dependency needs a valid id and version range.');if(ids.has(d.id))issue('id',`Duplicate dependency: ${d.id}.`);ids.add(d.id);}
    return {data,issues,positions};
}
async function walk(root){const files=[],pending=[root];while(pending.length){let entries;const folder=pending.pop();try{entries=await fs.readdir(folder,{withFileTypes:true});}catch(e){if(e.code==='ENOENT')continue;throw e;}for(const entry of entries){if(entry.isSymbolicLink())continue;const file=path.join(folder,entry.name);if(entry.isDirectory())pending.push(file);else if(entry.isFile())files.push(file);if(files.length>MAX_FILES)throw new Error(`Mod contains more than ${MAX_FILES} indexed files.`);}}return files;}
function assetKind(relative){const ext=path.extname(relative).toLowerCase(),id=slash(relative).toLowerCase();if(ext==='.asset')return 'sprite';if(ext==='.png')return 'texture';if(['.xml','.modelz'].includes(ext)&&id.startsWith('models/'))return 'model';if(ext==='.wav')return 'audio';if(['.ogg','.mp3'].includes(ext))return 'unsupported-audio';if(['.xml','.toml','.json','.txt','.lua'].includes(ext))return 'text';return 'binary';}
function canonicalModuleName(module){
    if(typeof module!=='string'||!module.length||module.trim()!==module||module.includes('/')||module.includes('\\')||module.includes(':')||module.startsWith('.')||module.endsWith('.')||module.includes('..'))return null;
    return /^[A-Za-z0-9_.-]+$/.test(module)?module.toLowerCase():null;
}
function literalRequires(text){
    const ast=parse(text),modules=[];if(!ast)return modules;
    const visit=n=>{if(!n||typeof n!=='object')return;if(['CallExpression','TableCallExpression','StringCallExpression'].includes(n.type)&&n.base?.type==='Identifier'&&n.base.name==='require'){
        const args=Array.isArray(n.arguments)?n.arguments:n.arguments?[n.arguments]:n.argument?[n.argument]:[],module=literal(args[0]);
        if(typeof module==='string'&&module!=='sf2'){const canonical=canonicalModuleName(module);if(canonical)modules.push(canonical);}
    }for(const [key,value] of Object.entries(n)){if(['loc','range','comments'].includes(key))continue;if(Array.isArray(value))for(const child of value)visit(child);else if(value&&typeof value==='object')visit(value);}};
    visit(ast);return modules;
}
async function indexMod(root,read=async f=>fs.readFile(f,'utf8')){
    const parsed=manifest(await read(path.join(root,'mod.toml'))),assets=new Map(),localizations=new Map(),issues=[],claimList=[],moveCalls=[],sources=new Map();
    const addLocalization=(id,language,value,file,line)=>{
        const normalized=normalizeLocalizationLanguage(language);
        if(!normalized){issues.push({file,line,message:`Unsafe localization language: ${language}.`});return;}
        const record=localizations.get(id)??{id,translations:[]};
        if(record.translations.some(t=>t.language===normalized)){issues.push({file,line,message:`Duplicate localization '${id}' for language '${normalized}'.`});return;}
        record.translations.push({language:normalized,value,file,line});localizations.set(id,record);
    };
    for(const file of await walk(path.join(root,'assets'))){
        const relative=slash(path.relative(path.join(root,'assets'),file));
        const id=relative.slice(0,path.extname(relative)?-path.extname(relative).length:undefined).toLowerCase(),kind=assetKind(relative);
        if(assets.has(id))issues.push({file,message:`Duplicate logical asset ID: ${id}.`});
        if(kind==='unsupported-audio')issues.push({file,message:'Loose mod audio supports PCM16 WAV, not OGG or MP3.'});
        assets.set(id,{id,kind,file});
        if(relative.endsWith('.asset')){
            const fields=Object.fromEntries((await read(file)).split(/\r?\n/).map(l=>/^\s*(\w+)\s*=\s*(.+?)\s*$/.exec(stripComment(l))).filter(Boolean).map(m=>[m[1],m[2].replace(/^"|"$/g,'')]));
            if(fields.type!=='sprite')issues.push({file,message:'Sprite descriptor requires type=sprite.'});
            if(!safe(fields.texture)||!fields.texture.toLowerCase().endsWith('.png'))issues.push({file,message:'texture must be a relative PNG path inside assets/.'});
            else {const texture=path.join(root,'assets',fields.texture);try{await fs.access(texture);}catch{issues.push({file,message:`Missing texture: ${fields.texture}.`});}}
        }
    }
    for(const file of await walk(path.join(root,'localizations'))){
        if(!file.endsWith('.toml'))continue;
        const language=path.basename(file,'.toml'),seen=new Set();
        (await read(file)).split(/\r?\n/).forEach((l,line)=>{
            const clean=stripComment(l).trim();if(!clean)return;
            const m=/^([^=]+)=\s*(.+)$/.exec(clean);if(!m){issues.push({file,line,message:'Expected localization key = "text".'});return;}
            const id=m[1].trim();let value;try{value=parseValue(m[2].trim());if(typeof value!=='string')throw Error();}catch{issues.push({file,line,message:'Localization values must be quoted strings.'});return;}
            if(seen.has(id))issues.push({file,line,message:`Duplicate localization key: ${id}.`});seen.add(id);
            addLocalization(id,language,value,file,line);
        });
    }
    const lineOf=(text,offset)=>text.slice(0,offset).split('\n').length-1;
    for(const file of (await walk(path.join(root,'movesets'))).filter(f=>f.endsWith('.json')).sort()){
        const text=await read(file);sources.set(file,text);
        const result=moveset.parse(text);
        for(const issue of result.issues)issues.push({file,line:lineOf(text,issue.offset),message:issue.message});
        if(result.entries.length&&!(parsed.data.capabilities??[]).includes('content.patch'))issues.push({file,line:0,message:'This moveset file edits moves; declare "content.patch" in mod.toml capabilities.'});
        const deps=new Set((parsed.data.dependencies??[]).map(d=>d.id));
        for(const entry of result.entries){
            const line=lineOf(text,entry.offset);
            if(entry.kind==='fork'&&typeof entry.item==='string'){const owner=entry.item.split(':')[0];if(owner!==parsed.data.id&&!deps.has(owner))issues.push({file,line,message:`Fork item "${entry.item}" needs a [[dependencies]] entry for "${owner}".`});}
            const call=entry.kind==='fork'?{name:'sf2.moves.fork',args:[{...entry.table,fields:[...entry.table.fields,{type:'TableKeyString',key:{name:'source'},value:moveset.field(entry.table,'move')}]}],node:entry.table}:{name:'sf2.moves.patch',args:[entry.table],node:entry.table};
            const claim=claims.claimOf(call,{literal,fields});
            if(claim)claimList.push({kind:claim.kind,key:claim.key,label:claim.label,file,line:claim.node?.range?lineOf(text,claim.node.range[0]):line,range:claim.node?.range??entry.table.range});
            moveCalls.push({name:call.name,args:call.args,node:entry.table,file,line});
            if(entry.kind==='fork'&&moveset.field(entry.table,'id')){
                // The copy's edits are an ordinary patch of the fork's runtime name.
                const forkName=`${parsed.data.id}.${entry.id}`;
                if(entry.table.fields.some(f=>f.type==='TableKeyString'&&!['id','move','subtype','item','note'].includes(f.key.name))){
                    const patch={...entry.table,fields:entry.table.fields.filter(f=>!['id','subtype','item'].includes(f.key?.name)).map(f=>f.key?.name==='move'?{...f,value:{...f.value,value:forkName}}:f)};
                    claimList.push({kind:'move patch',key:'move-patch:'+forkName,label:forkName,file,line,range:entry.table.range});
                    moveCalls.push({name:'sf2.moves.patch',args:[patch],node:entry.table,file,line,forkOf:entry.move});
                }
            }
        }
        for(const issue of moveset.check(result.entries,{literal,fields},parsed.data.id))issues.push({file,line:lineOf(text,issue.offset),message:issue.message,code:'native-guard'});
    }
    if(safe(parsed.data.entrypoint)){
        const entrypoint=path.join(root,parsed.data.entrypoint);
        try{
            await fs.access(entrypoint);
            const pending=[entrypoint],visited=new Set(),temporary={root,...parsed,assets,localizations,issues:[]};
            while(pending.length){
                const file=pending.pop(),key=path.resolve(file).toLowerCase();if(visited.has(key))continue;visited.add(key);
                let source;try{source=await read(file);}catch(e){if(e.code==='ENOENT')continue;throw e;}
                sources.set(file,source);
                for(const call of analyze(source,temporary).calls){
                    const line=call.node.loc?.start?.line?call.node.loc.start.line-1:undefined;
                    if(call.name==='sf2.localization.register'){
                        const definition=fields(call.args[0]),id=literal(definition.id),language=literal(definition.language),value=literal(definition.value);
                        if(typeof id==='string'&&id.length&&typeof language==='string'&&typeof value==='string'&&value.length)
                            addLocalization(id,language,value,file,line);
                    }
                    const claim=claims.claimOf(call,{literal,fields});
                    if(claim)claimList.push({kind:claim.kind,key:claim.key,label:claim.label,file,line:claim.node?.loc?claim.node.loc.start.line-1:line,range:claim.node?.range??call.node.range});
                    if(native.MOVE_FIELDS[call.name])moveCalls.push({name:call.name,args:call.args,node:call.node,file,line});
                }
                for(const module of literalRequires(source))pending.push(path.join(root,'scripts',...module.split('.'))+'.lua');
            }
        }catch(e){if(e.code==='ENOENT')parsed.issues.push({line:parsed.positions.entrypoint??0,message:`Entrypoint file is missing: ${parsed.data.entrypoint}.`});else throw e;}
    }
    return {root,...parsed,assets,localizations,claims:claimList,moveCalls,sources,issues:[...issues,...parsed.issues.map(x=>({...x,file:path.join(root,'mod.toml')}))]};
}
function normalizeLocalizationLanguage(language){if(typeof language!=='string'||!language.trim())return null;const value=language.trim().toLowerCase();return /^[a-z0-9_-]+$/.test(value)?value:null;}
const literal=n=>n?.type==='UnaryExpression'&&n.operator==='-'?-literal(n.argument):n?.type==='StringLiteral'?(n.value??n.raw.slice(1,-1).replace(/\\(["'\\])/g,'$1')):n?.type==='NumericLiteral'||n?.type==='BooleanLiteral'?n.value:undefined;
const fields=n=>Object.fromEntries((n?.fields??[]).filter(f=>f.type==='TableKeyString'||f.type==='TableKey').map(f=>[f.key.name??literal(f.key),f.value]));
function parse(text){try{return lua.parse(text,{luaVersion:'5.2',locations:true,ranges:true,comments:false});}catch{return null;}}
function analyze(text,mod){
    const ast=parse(text),issues=[],calls=[],contexts=[];if(!ast)return {issues,calls,contexts};
    function resolve(n,env){if(!n)return null;if(n.type==='Identifier')return env.get(n.name);if(n.type==='MemberExpression'){const base=resolve(n.base,env);if(base==='fighter'&&n.identifier.name==='actor')return 'actor';return typeof base==='string'?`${base}.${n.identifier.name}`:null;}if(n.type==='IndexExpression'&&resolve(n.base,env)==='actor-list')return 'actor';if(n.type==='CallExpression'){if(resolve(n.base,env)==='fighter.actors')return 'actor-list';if(n.base.type==='Identifier'&&n.base.name==='ipairs'&&resolve(n.arguments[0],env)==='actor-list')return 'actor-iterator';}return null;}
    function add(n,code,message,capability){issues.push({range:n.range,code,message,capability});}
    const caps=new Set(mod.data.capabilities??[]),dependencies=new Set((mod.data.dependencies??[]).map(d=>d.id));
    function required(n,cap){if(Array.isArray(cap)){for(const item of cap)required(n,item);}else if(cap&&!caps.has(cap))add(n,'capability',`Declare "${cap}" in mod.toml to use this operation.`,cap);}
    function expression(n,env,callback){
        if(!n||typeof n!=='object')return;
        if(['CallExpression','TableCallExpression','StringCallExpression'].includes(n.type)){
            const symbol=resolve(n.base,env),args=Array.isArray(n.arguments)?n.arguments:n.arguments?[n.arguments]:n.argument?[n.argument]:[];
            const name=api.aliases[symbol]??symbol,info=api.functions[name];
            if(info){calls.push({name,node:n,args});required(n,info.capability);
                native.checkCall(name,args,{literal,fields,forkPrefix:typeof mod.data.id==='string'?mod.data.id+'.':undefined},(node,code,message)=>add(node??n,code,message));
                if(name==='sf2.audio.play'||name==='sf2.audio.set_volume'){
                    const volume=name==='sf2.audio.play'?fields(args[1]).volume:args[1],value=literal(volume);
                    if(typeof value==='number'&&(!Number.isFinite(value)||value<0||value>1))add(volume,'range','Audio volume must be finite from 0 through 1.');
                }
                if(name==='sf2.story.play_sequence'&&fields(args[0]).position)required(n,['state.read','state.write']);
                if(name==='sf2.rules.behavior'&&literal(fields(args[0]).controls_outcome)===true)required(n,'combat.round_outcome');
                if(name==='sf2.localization.register'){
                    const definition=fields(args[0]),id=literal(definition.id),language=literal(definition.language),value=literal(definition.value);
                    if(typeof id==='string'&&(!id.trim()||id.includes(':')))add(definition.id,'localization-id','id must be a nonempty local localization key, without a namespace.');
                    if(typeof language==='string'&&!normalizeLocalizationLanguage(language))add(definition.language,'localization-language','language must contain only letters, digits, underscores or hyphens.');
                    if(typeof value==='string'&&!value.length)add(definition.value,'localization-value','value must be a nonempty string.');
                }
                if(info.referenceKind&&typeof literal(args[0])==='string'){
                    const reference=literal(args[0]),colon=reference.indexOf(':'),owner=colon<0?mod.data.id:reference.slice(0,colon),id=colon<0?reference:reference.slice(colon+1);
                    if(owner!==mod.data.id&&!dependencies.has(owner))add(args[0],'dependency',`Declare dependency "${owner}" before referencing its content.`);
                    if(owner===mod.data.id&&info.referenceKind!=='extension'){const collection=info.referenceKind==='localization'?mod.localizations:mod.assets;const localId=info.referenceKind==='localization'?id.replace(/^localization\//,''):id.toLowerCase();const target=collection.get(localId);if(!target)add(args[0],'missing-reference',`No ${info.referenceKind} found for "${reference}" in this mod.`);else if(info.referenceKind!=='localization'&&target.kind!==info.referenceKind)add(args[0],'asset-kind',`"${reference}" is ${target.kind}, but this call requires ${info.referenceKind}.`);}
                }
                if(info.bounds)for(const [key,[min,max]] of Object.entries(info.bounds)){const index=Object.keys(info.params).indexOf(key),value=literal(args[index]);if(typeof value==='number'&&(!Number.isInteger(value)||value<min||value>max))add(args[index],'range',`${key} must be an integer from ${min} through ${max}.`);}
                if(name==='sf2.behaviors.register'){
                    const definition=fields(args[0]);
                    for(const cb of api.callbacks){const handler=definition[cb];if(handler?.type!=='FunctionDeclaration')continue;const next=new Map(env);for(const p of handler.parameters)next.set(p.name,null);const parameters=handler.parameters[0]?.name,fighter=handler.parameters[1]?.name,event=handler.parameters[2]?.name;if(fighter)next.set(fighter,'fighter');
                        contexts.push({range:handler.range,callback:cb,parameters,fighter,event,stateful:!!definition.state,schema:fields(definition.parameters),stateSchema:fields(fields(definition.state).fields)});
                        block(handler.body,next,cb);
                    }
                }
            }
            if(typeof symbol==='string'&&symbol.startsWith('fighter.')){const method=symbol.split('.').at(-1),target=symbol.startsWith('fighter.opponent.');const spec=api.fighterMethods[method];if(spec){required(n,spec.capability);if(target)required(n,'combat.target');if(['move_by','play_move','mark_rect','mark_sprite','acquire_camera','projectiles','spawn_projectile','actors','spawn_actor'].includes(method)&&['on_fight_begin','on_round_begin','on_round_end','on_fight_end'].includes(callback)||['projectiles','spawn_projectile','acquire_camera'].includes(method)&&callback==='on_actor_end')add(n,'callback-timing',method+' requires an active simulation callback.');if(method==='scale_incoming_damage'&&callback!=='on_damage_resolving')add(n,'callback-timing','scale_incoming_damage is available only in on_damage_resolving.');}}
            if(typeof symbol==='string'&&symbol.startsWith('actor.')){const method=symbol.slice(6),spec=api.actorMethods[method];if(spec){if(spec.capability!=='combat.actors')required(n,'combat.actors');required(n,spec.capability);if(['on_fight_begin','on_round_begin','on_round_end','on_fight_end','on_actor_end'].includes(callback))add(n,'callback-timing',method+' requires an active actor callback reference.');}}
            for(const arg of args){if(info&&name==='sf2.behaviors.register'&&arg===args[0]){for(const f of arg.fields??[])if(!api.callbacks.includes(f.key?.name))expression(f.value,env,callback);}else expression(arg,env,callback);}return;
        }
        if(n.type==='FunctionDeclaration'){const next=new Map(env);for(const p of n.parameters)next.set(p.name,null);block(n.body,next,callback);return;}
        for(const [key,value] of Object.entries(n)){if(['loc','range','comments'].includes(key))continue;if(Array.isArray(value))for(const v of value)expression(v,env,callback);else if(value&&typeof value==='object')expression(value,env,callback);}
    }
    function block(body,env,callback){for(const node of body??[]){
        if(node.type==='LocalStatement'||node.type==='AssignmentStatement'){
            for(const init of node.init)expression(init,env,callback);
            node.variables.forEach((variable,i)=>{if(variable.type!=='Identifier')return;const init=node.init[i];const name=init?.type==='CallExpression'&&init.base.type==='Identifier'&&init.base.name==='require'&&literal(init.arguments[0])==='sf2'&&!env.has('require')?'sf2':resolve(init,env);env.set(variable.name,name);});
        }else if(node.type==='IfStatement'){for(const clause of node.clauses){expression(clause.condition,env,callback);block(clause.body,new Map(env),callback);}}
        else if(['DoStatement','WhileStatement','RepeatStatement','ForNumericStatement','ForGenericStatement'].includes(node.type)){const next=new Map(env);if(node.variable)next.set(node.variable.name,null);for(const v of node.variables??[])next.set(v.name,null);for(const iterator of node.iterators??[])expression(iterator,env,callback);if(node.type==='ForGenericStatement'&&node.variables.length>=2&&resolve(node.iterators[0],env)==='actor-iterator')next.set(node.variables[1].name,'actor');block(node.body,next,callback);}
        else expression(node,env,callback);
    }}
    block(ast.body,new Map(),null);return {issues,calls,contexts};
}
// Called only for incomplete-string completion. Parse a repaired document so
// aliases/comments/shadowed names are handled by the same AST analysis.
function completionContext(text,offset,mod){
    const prefix=text.slice(0,offset),match=/(["'])([^"'\n]*)$/.exec(prefix);if(!match)return null;
    const start=offset-match[2].length;
    let result;
    for(const ending of [match[1],match[1]+')',match[1]+')\n}',match[1]+')\n}\nend'] ){
        const repaired=prefix+ending+text.slice(offset).replace(/^[^"'\r\n]*["']/, '');
        result=analyze(repaired,mod).calls.find(c=>c.args[0]?.type==='StringLiteral'&&c.args[0].range[0]===start-1);if(result)break;
    }
    if(!result)return null;return {name:result.name,kind:api.functions[result.name]?.referenceKind,start,prefix:match[2]};
}
// String literal inside an sf2.moves.* definition table, with the field path from
// the call's table to it. Completion repairs the unterminated string first.
function moveFieldContext(text,offset,mod,repair=true){
    const find=(source,at)=>{
        for(const call of analyze(source,mod).calls){
            if(!native.MOVE_FIELDS[call.name]||call.args[0]?.type!=='TableConstructorExpression')continue;
            const visit=(node,keys)=>{
                for(const field of node.fields??[]){
                    const key=field.type==='TableKeyString'?field.key.name:field.type==='TableKey'?literal(field.key):undefined,next=key===undefined?keys:[...keys,key];
                    if(field.value?.type==='StringLiteral'&&field.value.range[0]<at&&at<=field.value.range[1])return {node:field.value,path:next};
                    if(field.value?.type==='TableConstructorExpression'){const found=visit(field.value,next);if(found)return found;}
                }
            };
            const found=visit(call.args[0],[]);
            if(found)return {name:call.name,path:found.path,definition:fields(call.args[0]),node:found.node,value:literal(found.node)};
        }
    };
    if(!repair)return find(text,offset);
    const prefix=text.slice(0,offset),match=/(["'])([^"'\n]*)$/.exec(prefix);if(!match)return null;
    for(const ending of [match[1],match[1]+'}',match[1]+'}\n}',match[1]+'}\n}\n}',match[1]+'}\nend']){
        const found=find(prefix+ending+text.slice(offset).replace(/^[^"'\r\n]*["']/,''),offset);
        if(found)return {...found,start:offset-match[2].length,prefix:match[2]};
    }
    return null;
}
module.exports={moveFieldContext,manifest,indexMod,walk,assetKind,analyze,completionContext,literal,fields,safe,stripComment,parseValue,normalizeLocalizationLanguage,canonicalModuleName,literalRequires};
