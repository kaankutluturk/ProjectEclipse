const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs/promises');
const os = require('node:os');
const path = require('node:path');
const p = require('../src/project.cjs');
const { createMod } = require('../src/scaffold.cjs');
const template = path.resolve(__dirname, '../templates/weapon');
const header = 'local sf2 = require("sf2")\n';

test('text input example and starter share an owned string UI contract',async()=>{
 const directory=path.resolve(__dirname,'../../../Mods/example.text-input-lab');
 const mod=await p.indexMod(directory);assert.deepEqual(mod.issues,[]);
 const source=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');assert.deepEqual(p.analyze(source,mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua'])assert.deepEqual(await fs.readFile(path.join(directory,file)),await fs.readFile(path.resolve(__dirname,'../templates/text-input-lab',file)));
 const schema=require('../scripts/api-schema.cjs');
 assert(schema.types.UiNode.fields.kind.includes('text_input'));
 for(const key of ['max_chars?','placeholder?','multiline?'])assert(schema.types.UiNode.fields[key]);
 assert(schema.types.UiDefinition.fields['on_change?'].includes('string'));
 assert.equal(schema.functions['sf2.ui.get_text'].returns,'string');
});

test('camera ownership has distinct handles/capability, lifecycle gates and a mirrored starter',async()=>{
 const directory=path.resolve(__dirname,'../../../Mods/example.camera-lab');
 const mod=await p.indexMod(directory);assert.deepEqual(mod.issues,[]);
 const source=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');assert.deepEqual(p.analyze(source,mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua'])assert.deepEqual(await fs.readFile(path.join(directory,file)),await fs.readFile(path.resolve(__dirname,'../templates/camera-lab',file)));
 const api=require('../data/api.json');assert.equal(api.fighterMethods.acquire_camera.capability,'presentation.camera');
 for(const name of ['set_camera','is_camera_active','release_camera'])assert.equal(api.functions['sf2.world.'+name].capability,'presentation.camera');
 const missing={...mod,data:{...mod.data,capabilities:['content.register','presentation.visuals']}};
 assert(p.analyze(header+'sf2.behaviors.register{id="x",on_tick=function(_,fighter) fighter:acquire_camera{} end}',missing).issues.some(i=>i.capability==='presentation.camera'));
 for(const callback of ['on_fight_begin','on_round_begin','on_round_end','on_fight_end','on_actor_end']){
  const issues=p.analyze(header+'sf2.behaviors.register{id="x",'+callback+'=function(_,fighter) fighter:acquire_camera{} end}',mod).issues;
  assert(issues.some(i=>i.code==='callback-timing'),callback);
 }
 for(const callback of ['on_tick','on_actor_spawn'])assert(!p.analyze(header+'sf2.behaviors.register{id="x",'+callback+'=function(_,fighter) fighter:acquire_camera{} end}',mod).issues.some(i=>i.code==='callback-timing'));
});

test('authored fighter starter mirrors native models and original point animation',async()=>{
 const directory=path.resolve(__dirname,'../../../Mods/example.authored-fighter');
 const mod=await p.indexMod(directory); assert.deepEqual(mod.issues,[]);
 const source=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');
 assert.deepEqual(p.analyze(source,mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua','assets/models/body.xml','assets/models/sash.xml','assets/animations/strike.bytes','assets/animations/strike.rig.json'])
  assert.deepEqual(await fs.readFile(path.join(directory,file)),await fs.readFile(path.resolve(__dirname,'../templates/authored-fighter',file)));
 const binary=await fs.readFile(path.join(directory,'assets/animations/strike.bytes'));
 assert.equal(binary.readInt32LE(0),61); assert.equal(binary.readInt32LE(5),67);
 const missing={...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(c=>c!=='combat.actors')}};
 assert(p.analyze(source,missing).issues.some(i=>i.capability==='combat.actors'));
 const withoutForms={...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(c=>c!=='combat.transform')}};
 assert(p.analyze(source,withoutForms).issues.some(i=>i.capability==='combat.transform'));
 assert(source.includes('player_character = character'));
 const schema=require('../scripts/api-schema.cjs');
 assert.equal(schema.types.FightDefinition.fields['player_character?'][0],'Eclipse.WarriorHandle');
 assert.equal(schema.types.EncounterPlan.fields['player_character?'][0],'Eclipse.WarriorHandle');
 assert(source.includes('sf2.modes.resolve(request, { player_character = selected })'));
 assert(!schema.types.FightPatch.fields['player_character?']);
});

test('actor forms require both capabilities and a scoped runtime callback',()=>{
 const source=header+'sf2.behaviors.register{id="host",on_tick=function(_,fighter) fighter.actor:change_form(character) end}';
 for(const missing of ['combat.actors','combat.transform']){
  const mod={data:{capabilities:['content.register','combat.actors','combat.transform'].filter(c=>c!==missing)},assets:new Map()};
  assert(p.analyze(source,mod).issues.some(i=>i.capability===missing));
 }
 const mod={data:{capabilities:['content.register','combat.actors','combat.transform']},assets:new Map()};
 assert(!p.analyze(source,mod).issues.some(i=>i.code==='callback-timing'));
 assert(p.analyze(source.replace('on_tick','on_actor_end'),mod).issues.some(i=>i.code==='callback-timing'));
});

test('actor companion starter mirrors playable files, scopes capabilities and lifecycle timing',async()=>{
 const dir=path.resolve(__dirname,'../../../Mods/example.actor-companions'),mod=await p.indexMod(dir);
 assert.deepEqual(mod.issues,[]);
 const source=await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8');
 assert.deepEqual(p.analyze(source,mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua','assets/animations/high_punch.bytes'])assert.deepEqual(await fs.readFile(path.join(dir,file)),await fs.readFile(path.resolve(__dirname,'../templates/actor-companions',file)));
 const api=require('../data/api.json');assert.equal(require('../scripts/api-schema.cjs').functions['sf2.actors.register'].returns,'Eclipse.ActorDefinitionHandle');
 for(const [name,method] of Object.entries(api.actorMethods))assert.equal(method.capability,name==='change_form'?'combat.transform':'combat.actors');
 const missing={...mod,data:{...mod.data,capabilities:['content.register']}};
 const issues=p.analyze(header+'sf2.behaviors.register{id="test",on_round_begin=function(_,fighter) local actors=fighter:actors(); for _,actor in ipairs(actors) do actor:remove() end end}',missing).issues;
 assert(issues.some(i=>i.capability==='combat.actors'));assert(issues.filter(i=>i.code==='callback-timing').length>=2);
});

test('scripted burst starter mirrors assets and validates direct projectile commands',async()=>{
    const directory=path.resolve(__dirname,'../../../Mods/example.scripted-burst');
    const mod=await p.indexMod(directory);const text=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');
    assert.deepEqual(mod.issues,[]);assert.deepEqual(p.analyze(text,mod).issues,[]);
    for(const file of ['mod.toml','README.md','scripts/main.lua','assets/animations/shuriken_fly.bytes'])
        assert.deepEqual(await fs.readFile(path.resolve(__dirname,'../templates/scripted-burst',file)),await fs.readFile(path.join(directory,file)));
    assert(p.analyze(text,{...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(c=>c!=='combat.projectiles')}}).issues.some(i=>i.capability==='combat.projectiles'));
    const lifecycle='local sf2=require("sf2");sf2.behaviors.register{id="test",on_round_end=function(_,fighter) fighter:spawn_projectile({},0,0) end}';
    assert(p.analyze(lifecycle,mod).issues.some(i=>i.code==='callback-timing'&&i.message.includes('spawn_projectile')));
});

test('return dart starter validates owned projectile graph and mirrors complete assets',async()=>{
    const directory=path.resolve(__dirname,'../../../Mods/example.return-dart');
    const mod=await p.indexMod(directory);const text=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');
    assert.deepEqual(mod.issues,[]);assert.deepEqual(p.analyze(text,mod).issues,[]);
    for(const file of ['mod.toml','README.md','scripts/main.lua',...['ranged_light_player','ranged_light_weapon','shuriken_fly'].map(n=>'assets/animations/'+n+'.bytes')])
        assert.deepEqual(await fs.readFile(path.resolve(__dirname,'../templates/return-dart',file)),await fs.readFile(path.join(directory,file)));
    assert(p.analyze(text,{...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(c=>c!=='combat.projectiles')}}).issues.some(i=>i.capability==='combat.projectiles'));
});

test('arc dart starter validates owned projectile graph and mirrors complete assets',async()=>{
    const directory=path.resolve(__dirname,'../../../Mods/example.arc-dart');
    const mod=await p.indexMod(directory);const text=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');
    assert.deepEqual(mod.issues,[]);assert.deepEqual(p.analyze(text,mod).issues,[]);
    for(const file of ['mod.toml','README.md','scripts/main.lua',...['ranged_light_player','ranged_light_weapon','shuriken_fly'].map(n=>'assets/animations/'+n+'.bytes')])
        assert.deepEqual(await fs.readFile(path.resolve(__dirname,'../templates/arc-dart',file)),await fs.readFile(path.join(directory,file)));
    assert(p.analyze(text,{...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(c=>c!=='combat.animation')}}).issues.some(i=>i.capability==='combat.animation'));
});

test('active strike starter validates playback capabilities, timing, handles and mirrored assets',async()=>{
    const directory=path.resolve(__dirname,'../../../Mods/example.active-strike');
    const mod=await p.indexMod(directory);const text=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');
    assert.deepEqual(mod.issues,[]);assert.deepEqual(p.analyze(text,mod).issues,[]);
    for(const file of ['mod.toml','scripts/main.lua','assets/animations/high_punch.bytes'])
        assert.deepEqual(await fs.readFile(path.resolve(__dirname,'../templates/active-strike',file)),await fs.readFile(path.join(directory,file)));
    assert(p.analyze(text,{...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(c=>c!=='combat.animation')}}).issues.some(i=>i.capability==='combat.animation'));
    for(const callback of ['on_fight_begin','on_round_begin','on_round_end','on_fight_end'])
        assert(p.analyze(header+`sf2.behaviors.register{id="test",${callback}=function(_,fighter) fighter:play_move(move) end}`,mod).issues.some(i=>i.code==='callback-timing'));
    assert(p.analyze(header+'sf2.behaviors.register{id="test",on_tick=function(_,fighter) fighter.opponent:play_move(move) end}',mod).issues.some(i=>i.capability==='combat.target'));
});

test('motion starter validates capabilities, lifetime timing and mirrored files', async () => {
    const directory=path.resolve(__dirname,'../../../Mods/example.repulse');
    const mod=await p.indexMod(directory);
    const text=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(p.analyze(text,mod).issues,[]);
    for(const file of ['mod.toml','scripts/main.lua'])
        assert.equal(await fs.readFile(path.resolve(__dirname,'../templates/repulse',file),'utf8'),await fs.readFile(path.join(directory,file),'utf8'));
    for(const cap of ['combat.motion','combat.target']) {
        const missing={...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(c=>c!==cap)}};
        assert(p.analyze(text,missing).issues.some(i=>i.capability===cap));
    }
    for(const callback of ['on_fight_begin','on_round_begin','on_round_end','on_fight_end']) {
        const source=header+`sf2.behaviors.register { id="test",${callback}=function(_,fighter) fighter:move_by(1,0) end }`;
        assert(p.analyze(source,mod).issues.some(i=>i.code==='callback-timing'));
    }
    const source=header+'sf2.behaviors.register { id="test",on_tick=function(_,fighter) fighter:move_by(1,0) end }';
    assert.deepEqual(p.analyze(source,mod).issues,[]);
});

test('round controller starter validates declaration and invocation capabilities', async () => {
    const directory = path.resolve(__dirname, '../../../Mods/example.hit-objective');
    const mod = await p.indexMod(directory);
    const text = await fs.readFile(path.join(directory, 'scripts/main.lua'), 'utf8');
    assert.deepEqual(mod.issues, []);
    assert.deepEqual(p.analyze(text, mod).issues, []);
    for (const file of ['mod.toml', 'scripts/main.lua'])
        assert.equal(await fs.readFile(path.resolve(__dirname, '../templates/hit-objective', file), 'utf8'), await fs.readFile(path.join(directory, file), 'utf8'));
    const missing = {...mod, data:{...mod.data,capabilities:mod.data.capabilities.filter(cap=>cap!=='combat.round_outcome')}};
    assert(p.analyze(text, missing).issues.some(issue=>issue.capability==='combat.round_outcome'));
    assert(p.analyze(header+'sf2.rules.behavior { id="goal", controls_outcome=true }', missing).issues.some(issue=>issue.capability==='combat.round_outcome'));
    assert(!p.analyze(header+'sf2.rules.behavior { id="goal", controls_outcome=false }', missing).issues.some(issue=>issue.capability==='combat.round_outcome'));
});

test('framework and add-on templates validate service capabilities and dependencies', async () => {
    for (const id of ['example.focus-framework', 'example.focus-addon']) {
        const directory = path.resolve(__dirname, '../../../Mods', id);
        const mod = await p.indexMod(directory);
        assert.deepEqual(mod.issues, []);
        const text = await fs.readFile(path.join(directory, 'scripts/main.lua'), 'utf8');
        assert.deepEqual(p.analyze(text, mod).issues, []);
        const starter = path.resolve(__dirname, '../templates', id.replace('example.', ''));
        const indexedStarter = await p.indexMod(starter);
        assert.deepEqual(indexedStarter.issues, []);
        assert.deepEqual(p.analyze(text, indexedStarter).issues, []);
        for (const file of ['mod.toml', 'scripts/main.lua'])
            assert.equal(await fs.readFile(path.join(starter, file), 'utf8'), await fs.readFile(path.join(directory, file), 'utf8'));
        const missing = p.analyze(text, {...mod, data:{...mod.data, capabilities:[]}}).issues;
        assert(missing.some(issue => issue.capability === (id.endsWith('framework') ? 'extensions.provide' : 'extensions.call')));
    }
    const directory = path.resolve(__dirname, '../../../Mods/example.focus-addon');
    const mod = await p.indexMod(directory);
    const text = header + "sf2.extensions.get('example.focus-framework:extensions/status', 1)";
    const undeclared = p.analyze(text, {...mod, data:{...mod.data, dependencies:[]}}).issues;
    assert(undeclared.some(issue => issue.code === 'dependency'));
    assert(p.analyze(text.replace(', 1)', ', 0)'), mod).issues.some(issue => issue.code === 'range'));
});

test('floor stain starter validates and exposes flight and accumulation fields', async () => {
    const directory = path.resolve(__dirname, '../templates/floor-stains');
    const mod = await p.indexMod(directory);
    assert.deepEqual(mod.issues, []);
    assert.deepEqual(p.analyze(await fs.readFile(path.join(directory, 'scripts/main.lua'), 'utf8'), mod).issues, []);
    const fields = require('../data/api.json').types.FxStainDefinition.fields;
    for (const name of ['speed_min', 'speed_max', 'gravity', 'lift', 'merge_radius', 'max_pool_size'])
        assert.equal(fields[name + '?'][0], 'number');
});

for (const name of ['programmable-ai', 'generated-expedition', 'shifting-guardian']) test(name+' example and starter share a valid public contract', async () => {
    const directory=path.resolve(__dirname,'../../../ArchivedMods/example.'+name);
    const mod=await p.indexMod(directory);
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(p.analyze(await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8'),mod).issues,[]);
    for (const file of ['scripts/main.lua','mod.toml','localizations/eng.toml'])
        assert.equal(await fs.readFile(path.resolve(__dirname,'../templates',name,file),'utf8'),await fs.readFile(path.join(directory,file),'utf8'));
});

test('clean starter indexes and validates, including table-call syntax', async () => {
    const mod = await p.indexMod(template);
    assert.deepEqual(mod.issues, []);
    assert.equal(mod.assets.get('sprites/weapon').kind, 'sprite');
    assert.equal(mod.localizations.get('weapon.training_blade').translations[0].language, 'eng');
    assert.deepEqual(p.analyze(await fs.readFile(path.join(template, 'scripts/main.lua'), 'utf8'), mod).issues, []);
});
test('spotlight rule is typed and documented for battle mods', async () => {
    const mod = await p.indexMod(path.resolve(__dirname, '../templates/battle-rules'));
    const source = header + 'local light = sf2.rules.light_in_the_darkness { id="light", radius=0.2, shape=1, target=sf2.rules.PLAYER }\n';
    assert.deepEqual(p.analyze(source, mod).issues, []);
    const api = require('../data/api.json');
    assert.equal(api.functions['sf2.rules.light_in_the_darkness'].capability, 'content.register');
    assert.equal(api.types.Rule_light_in_the_darkness.fields.radius[0], 'number');
});
test('Lua localization registration is typed, indexed, capability checked, and immediately key-addressable', async t => {
    const root=await fs.mkdtemp(path.join(os.tmpdir(),'eclipse-localization-'));
    t.after(()=>fs.rm(root,{recursive:true,force:true}));
    await fs.mkdir(path.join(root,'scripts'),{recursive:true});
    await fs.writeFile(path.join(root,'mod.toml'),[
        'schema = 1','id = "lua.locale"','name = "Lua Locale"','version = "1.0.0"',
        'authors = ["Test"]','entrypoint = "scripts/main.lua"','capabilities = ["content.register"]',''
    ].join('\n'));
    const source=header+[
        'local name = sf2.localization.register { id="item.desolator", language=" ENG ", value="Desolator" }',
        'sf2.localization.register { id="item.desolator", language="pol", value="Desolator" }',
        'local same = sf2.localization.key("item.desolator")',
        'local text = sf2.localization.text(name, "eng")',''
    ].join('\n');
    await fs.writeFile(path.join(root,'scripts/main.lua'),source);
    const mod=await p.indexMod(root);
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(mod.localizations.get('item.desolator').translations.map(x=>x.language),['eng','pol']);
    assert.deepEqual(p.analyze(source,mod).issues,[]);
    const api=require('../data/api.json'),schema=require('../scripts/api-schema.cjs');
    assert.equal(schema.functions['sf2.localization.register'].returns,'Eclipse.LocalizationHandle');
    assert.equal(api.functions['sf2.localization.register'].capability,'content.register');
    assert.deepEqual(api.types.LocalizationDefinition.fields,{id:'string',language:'string',value:'string'});
    const withoutCapability={...mod,data:{...mod.data,capabilities:[]}};
    assert(p.analyze(source,withoutCapability).issues.some(i=>i.capability==='content.register'));
});

test('Lua and TOML localization registrations share normalized duplicate validation', async t => {
    const root=await fs.mkdtemp(path.join(os.tmpdir(),'eclipse-localization-duplicate-'));
    t.after(()=>fs.rm(root,{recursive:true,force:true}));
    await fs.mkdir(path.join(root,'scripts'),{recursive:true});
    await fs.mkdir(path.join(root,'localizations'),{recursive:true});
    await fs.writeFile(path.join(root,'mod.toml'),[
        'schema = 1','id = "lua.locale"','name = "Lua Locale"','version = "1.0.0"',
        'authors = ["Test"]','entrypoint = "scripts/main.lua"','capabilities = ["content.register"]',''
    ].join('\n'));
    await fs.writeFile(path.join(root,'localizations/eng.toml'),'item.desolator = "From TOML"\n');
    await fs.writeFile(path.join(root,'scripts/main.lua'),header+'sf2.localization.register { id="item.desolator", language="ENG", value="From Lua" }\n');
    const mod=await p.indexMod(root);
    assert(mod.issues.some(i=>i.message.includes("Duplicate localization 'item.desolator' for language 'eng'")));
});
test('Lua localization indexing follows reachable literal local require modules once', async t => {
    const root=await fs.mkdtemp(path.join(os.tmpdir(),'eclipse-localization-module-'));
    t.after(()=>fs.rm(root,{recursive:true,force:true}));
    await fs.mkdir(path.join(root,'scripts/content'),{recursive:true});
    await fs.writeFile(path.join(root,'mod.toml'),[
        'schema = 1','id = "lua.module"','name = "Lua Module"','version = "1.0.0"',
        'authors = ["Test"]','entrypoint = "scripts/main.lua"','capabilities = ["content.register"]',''
    ].join('\n'));
    await fs.writeFile(path.join(root,'scripts/main.lua'),header+'require("content.equipment")\nrequire("content.equipment")\n');
    await fs.writeFile(path.join(root,'scripts/content/equipment.lua'),header+[
        'sf2.localization.register { id="item.required", language="eng", value="Required" }',
        'require("content.nested")',''
    ].join('\n'));
    await fs.writeFile(path.join(root,'scripts/content/nested.lua'),header+'sf2.localization.register { id="item.nested", language="eng", value="Nested" }\n');
    await fs.writeFile(path.join(root,'scripts/content/unreferenced.lua'),header+'sf2.localization.register { id="item.unreferenced", language="eng", value="Unreferenced" }\n');
    const mod=await p.indexMod(root);
    assert.deepEqual(mod.issues,[]);
    assert(mod.localizations.has('item.required'));
    assert(mod.localizations.has('item.nested'));
    assert(!mod.localizations.has('item.unreferenced'));
    assert.equal(mod.localizations.get('item.required').translations.length,1);
});

test('actual DE128 reachable Lua localization is indexed', async () => {
    const mod=await p.indexMod(path.resolve(__dirname,'../../../Mods/de128'));
    assert.deepEqual(mod.issues,[]);
    assert(mod.localizations.has('item.titans_desolator'));
    assert(mod.localizations.get('item.titans_desolator').translations.some(x=>x.language==='eng'&&x.value==="Titan's Desolator"));
    for(const name of ['mdl_body_berstuuk_early','mdl_head_berstuuk'])
        assert.equal(mod.assets.get(`models/underworld/${name}`)?.kind,'model');
    for(const name of ['mdl_body_titan','mdl_head_titan','mdl_ranged_titans_harpoon','mdl_magic_fireball'])
        assert.equal(mod.assets.get(`models/titan/${name}`)?.kind,'model');
    for(const name of ['boss_architect_hummer_new','boss_arkhos_hardmode_new','boss_bison_hard_new',
        'boss_crystal_hardmode_new','boss_fatum_hardmode_new','boss_fire_hardmode_new',
        'boss_hoaxen_hardmode_new','boss_hunger_hardmode_new','boss_lamb_fungus_hard_new',
        'boss_lamb_hard_new','boss_lamb_hunger_hard_new','boss_mushroom_hardmode_new',
        'boss_rakshasa_hardmode_new','boss_ravana_hard_new','boss_saturn_hard_new',
        'boss_tenebris_hardmode_new','boss_vortex_hardmode_new','boss_war_hardmode_new',
        'boss_whisper_hardmode_new','new_man_shuang_gou_hardmode_new'])
        assert.equal(mod.assets.get(`sprites/underworld/${name}`)?.kind,'sprite');
    for(const state of ['base','active'])
        assert.equal(mod.assets.get(`sprites/underworld/battlebtnprince_${state}`)?.kind,'sprite');
});
test('references, dependencies, capability requirements, and numeric limits', async () => {
    const mod = await p.indexMod(template);
    const result = p.analyze(header + `
sf2.assets.sprite("missing")
sf2.assets.model("sprites/weapon")
sf2.assets.sprite("another.mod:sprites/weapon")
sf2.price.coins(-1)
sf2.price.coins(1.5)
sf2.price.coins(2147483648)
sf2.behaviors.register { id="x", on_damage_dealt=function(params, fighter, event)
    fighter:change_health(-5)
    fighter.opponent:change_health(-5)
    fighter:scale_incoming_damage(0.5)
end }
`, mod);
    for (const code of ['missing-reference', 'asset-kind', 'dependency', 'range', 'capability', 'callback-timing']) assert(result.issues.some(i => i.code === code), code);
    assert.equal(result.issues.filter(i => i.code === 'range').length, 3);
    assert(result.issues.some(i => i.capability === 'combat.target'));
});
test('aliases resolve; comments and shadowed locals produce no false API warnings', async () => {
    const mod = await p.indexMod(template);
    assert.equal(p.analyze(header + 'local a=sf2.assets\na.sprite("missing")', mod).issues[0].code, 'missing-reference');
    for (const text of ['-- sf2.assets.sprite("missing")', 'local sf2={}\nsf2.assets.sprite("missing")', 'local function f(sf2) sf2.assets.sprite("missing") end']) assert.deepEqual(p.analyze(header + text, mod).issues, []);
});
test('incomplete asset strings preserve AST context', async () => {
    const mod = await p.indexMod(template);
    for (const suffix of ['sf2.assets.sprite("spr', 'local a=sf2.assets\na.sprite("spr']) {
        const text = header + suffix, c = p.completionContext(text, text.length, mod);
        assert.equal(c?.kind, 'sprite'); assert.equal(c.prefix, 'spr');
    }
    assert.equal(p.completionContext(header + '-- sf2.assets.sprite("spr', (header + '-- sf2.assets.sprite("spr').length, mod), null);
});
test('manifest checks unsupported fields, unsafe paths, duplicates, and missing values', () => {
    const result = p.manifest('schema=2\nid="core"\nentrypoint="../main.lua"\nunknown="x"\nid="again"');
    for (const fragment of ['schema must', 'reserved', 'safe path', 'Unknown', 'Duplicate', 'Missing required']) assert(result.issues.some(i => i.message.includes(fragment)), fragment);
    for (const bad of ['../x', '/x', 'C:\\x', 'a/../x', 'a//x']) assert.equal(p.safe(bad), false);
    assert.equal(p.parseValue('["a", "b",]')[1], 'b');
});
test('current manifests validate and removed api metadata is rejected', async () => {
    const text = await fs.readFile(path.join(template, 'mod.toml'), 'utf8');
    assert.deepEqual(p.manifest(text).issues, []);
    assert.deepEqual(p.manifest('api="1.0.0"\n' + text).issues, [
        { line: 0, message: 'Unknown manifest field: api.' },
    ]);
});
test('scaffold creates a valid mod, refuses overwrite, and prevents path escape', async t => {
    const root = await fs.mkdtemp(path.join(os.tmpdir(), 'eclipse-editor-'));
    t.after(() => fs.rm(root, { recursive: true, force: true }));
    const destination = await createMod(template, root, 'test.blade', 'Test Blade', 'Test Author');
    const mod = await p.indexMod(destination);
    assert.deepEqual(mod.issues, []); assert.equal(mod.data.id, 'test.blade');
    await assert.rejects(createMod(template, root, 'test.blade', 'Again', 'Author'), { code: 'EEXIST' });
    await assert.rejects(createMod(template, root, '../escape', 'Escape', 'Author'));
    await fs.writeFile(path.join(destination, 'assets', 'bad.ogg'), 'test');
    assert((await p.indexMod(destination)).issues.some(i => i.message.includes('PCM16')));
});


test('battle rule starter is recognized by the authored API contract', async () => {
    const directory = path.resolve(__dirname, '../../../ArchivedMods/example.battle-rules');
    const mod = await p.indexMod(directory);
    assert.deepEqual(mod.issues, []);
    const result = p.analyze(await fs.readFile(path.join(directory, 'scripts/main.lua'), 'utf8'), mod);
    assert.deepEqual(result.issues, []);
    const api = require('../data/api.json');
    assert(api.functions['sf2.rules.behavior']);
    assert(api.types.Rule_behavior.fields.behavior);
});

test('core fight patch example validates with registered rule handles', async () => {
    const directory = path.resolve(__dirname, '../../../ArchivedMods/example.core-fight');
    const mod = await p.indexMod(directory);
    assert.deepEqual(mod.issues, []);
    assert.deepEqual(p.analyze(await fs.readFile(path.join(directory, 'scripts/main.lua'), 'utf8'), mod).issues, []);
    const api = require('../data/api.json');
    assert(api.types.FightPatch.fields['append_rules?']);
    assert(api.types.FightPatch.fields['warriors?']);
    assert(api.types.FightPatch.fields['reward_drops?']);
    assert.equal(api.types.FightPatch.fields['music?'][0], 'Eclipse.AudioHandle|string');
    assert(api.types.RewardDropPatch.fields.reward);
});

test('reward grant configure callback has typed context and result contract', () => {
    const api=require('../scripts/api-schema.cjs');
    assert.deepEqual(api.types.RewardGrantContext.fields,{player_level:'integer',item_id:'string'});
    assert.deepEqual(api.types.RewardGrantEnchantment.fields,{perk:'Eclipse.PerkHandle','aspect?':'number',
        'chance_factor?':'number','chance?':'number','frames?':'integer','parameters?':'table<string, number>'});
    assert.deepEqual(api.types.RewardGrantConfiguration.fields,{'level?':'integer','enchantments?':'Eclipse.RewardGrantEnchantment[]'});
    assert.equal(api.types.ItemGrant.fields['configure?'],'fun(context:Eclipse.RewardGrantContext):Eclipse.RewardGrantConfiguration');
    assert.equal(api.types.RewardCandidate.fields['configure?'],'fun(context:Eclipse.RewardGrantContext):Eclipse.RewardGrantConfiguration');
});

test('Ascension encounter planning and native challenge rules have typed contracts', () => {
    const api=require('../scripts/api-schema.cjs');
    assert.equal(api.types.EncounterPlan.fields['rules?'],'Eclipse.RuleHandle[]');
    assert.equal(api.types.EncounterPlan.fields['description?'],'string');
    assert.deepEqual(api.types.HotGroundNode.fields,{name:'string',axis:'"X"|"Y"','min?':'number','max?':'number'});
    assert.deepEqual(api.types.Rule_hot_ground.fields,{id:'string','target?':'"player"|"opponent"','mode?':'"normal"|"eclipse"|"all"','rounds?':'integer[]',frames:'integer',nodes:'Eclipse.HotGroundNode[]','animations?':'string[]'});
    assert.equal(api.types.Rule_ring_out.fields.axis,'"X"|"Y"');
    assert.equal(api.types.Rule_regeneration.fields.frames_after_hit,'integer');
    assert.equal(api.types.Rule_remove_interval.fields.type,'"Attack"|"Block"|"Invulnerable"|"SelfUninterrupt"|"Uninterrupt"|"Unstable"');
    assert.deepEqual(api.types.Rule_no_animation.fields,{id:'string',name:'string','mode?':'"normal"|"eclipse"|"all"','rounds?':'integer[]'});
    assert.equal(api.types.Rule_perk.fields['aspect?'],'number');
    for (const name of ['hot_ground','ring_out','regeneration','no_animation','remove_interval'])
        assert(api.functions['sf2.rules.'+name],name);
});

test('DE combat perk callbacks and fighter operations are typed', () => {
    const api=require('../scripts/api-schema.cjs');
    assert.deepEqual(api.types.HitPhaseEvent.fields,{
        damage:'number',blocked:'boolean',critical:'boolean',target:'"self"|"opponent"',
        weapon:'boolean',unarmed:'boolean',ranged:'boolean',magic:'boolean'
    });
    assert(api.callbacks.includes('on_hit_post_crit'));
    assert(api.callbacks.includes('on_post_hit'));
    assert(api.types.StatefulBehavior.fields['on_hit_post_crit?'].includes('Eclipse.HitPhaseEvent'));
    assert(api.types.StatefulBehavior.fields['on_post_hit?'].includes('Eclipse.OutgoingFighter'));
    assert.deepEqual(api.fighterMethods.add_outgoing_damage,{params:{amount:'number'},capability:'combat.modify_outgoing_hit'});
    assert.deepEqual(api.fighterMethods.show_status_icon,{params:{key:'string',sprite:'Eclipse.SpriteHandle',frames:'integer','stacks?':'integer'},capability:'combat.effects'});
    assert.deepEqual(api.fighterMethods.clear_status_icon,{params:{key:'string'},capability:'combat.effects'});
    assert.deepEqual(api.fighterMethods.set_control_blocked,{params:{control:'"punch"|"kick"|"ranged"|"magic"|"raid_charge"',blocked:'boolean'},capability:'combat.effects'});
    assert.deepEqual(api.fighterMethods.set_control_visible,{params:{control:'"raid_charge"',visible:'boolean'},capability:'combat.effects'});
    assert.deepEqual(api.fighterMethods.set_button_cooldown,{params:{control:'"punch"|"kick"|"ranged"|"raid_charge"',frames:'integer'},capability:'combat.effects'});
});

test('move perk-lock removal and initial perk rank are typed', () => {
    const api=require('../scripts/api-schema.cjs');
    assert.deepEqual(api.types.MovePerkLockRemoval.fields,{move:'string',perk:'Eclipse.PerkHandle'});
    assert.equal(api.functions['sf2.moves.remove_perk_lock'].capability,'content.patch');
    assert.equal(api.functions['sf2.moves.extend_perk_lock'].capability,'content.patch');
    assert.deepEqual(Object.keys(api.types.MovePerkLockExtension.fields),['move','source_perk','perk']);
    assert.equal(api.types.PerkDefinition.fields['initial_upgrade?'],'integer');
    assert.equal(api.types.TemplatePerk.fields['initial_upgrade?'],'integer');
});

test('move short form is typed for conditions, points, timeline and presets', () => {
    const api=require('../scripts/api-schema.cjs');
    const move=api.types.MoveDefinition.fields;
    assert.match(move['conditions?'],/Eclipse\.MoveShortCondition/);
    assert.match(move['events?'],/^"controlled"\|/);
    assert.match(move['direction?'],/"face_enemy"$/);
    assert.equal(move['timeline?'],'Eclipse.MoveTimeline');
    assert.match(move['tactic_distance?'],/Eclipse\.MoveShortTacticDistance/);
    const condition=api.types.MoveShortCondition.fields;
    for(const key of ['key?','not_mod?','not_interval?','not_animation?','controllable?','distance?','not_all?','any?'])assert(key in condition,key);
    assert.match(api.types.MoveAlignment.fields.pivot,/Eclipse\.MoveShortPoint/);
    assert.equal(api.types.MoveTimeline.fields['[integer]'],'Eclipse.MoveShortAction|Eclipse.MoveShortAction[]');
    assert.match(api.types.MoveTimeline.fields['animation_end?'],/MoveShortAction/);
    assert.equal(api.types.MoveInterval.fields['to?'],'integer');
    assert.match(api.types.MoveAttack.fields['damage_terms?'],/MoveDamageTermMap/);
});

test('guarded move replacement has typed fields and requires patch capability', async () => {
    const api=require('../scripts/api-schema.cjs');
    assert.equal(api.functions['sf2.moves.replace'].capability,'content.patch');
    assert.equal(api.functions['sf2.moves.replace'].returns,'Eclipse.MoveHandle');
    const fields=api.types.MoveReplacementDefinition.fields;
    assert.equal(fields.target,'string');
    assert.equal(fields.expected_file,'string');
    assert.equal(fields.animation,'Eclipse.BinaryHandle|string');
    assert.equal(fields['templates?'],undefined);
    assert.equal(api.types.MoveAlignment.fields['shift_model_node?'],'string');
    assert.equal(api.types.MoveDirectionCondition,undefined);
    assert.equal(api.types.MoveShortCondition.fields['direction?'],'"Me"|"Enemy"');
    const mod=await p.indexMod(template);
    const source=header+'sf2.moves.replace { id="step", target="ExistingStep", expected_file="old.bytes", animation=sf2.assets.binary("animations/step") }';
    assert(p.analyze(source,mod).issues.some(i=>i.capability==='content.patch'));
});

test('perk upgrade example and starter validate assets, localization and branch definitions', async () => {
    for (const relative of ['../../../ArchivedMods/example.perk-upgrades', '../templates/perk-upgrades']) {
        const directory = path.resolve(__dirname, relative);
        const mod = await p.indexMod(directory);
        assert.deepEqual(mod.issues, [], relative);
        assert.deepEqual(p.analyze(await fs.readFile(path.join(directory, 'scripts/main.lua'), 'utf8'), mod).issues, [], relative);
    }
});

test('outgoing rule example validates its callback and capability', async () => {
    const directory=path.resolve(__dirname,'../../../ArchivedMods/example.outgoing-rule');
    const mod=await p.indexMod(directory);
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(p.analyze(await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('combo reserve example validates native activity callbacks', async () => {
    const directory=path.resolve(__dirname,'../../../ArchivedMods/example.combo-reserve');
    const mod=await p.indexMod(directory);
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(p.analyze(await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('charge UI example validates owned handles and callback capability', async () => {
    const directory=path.resolve(__dirname,'../../../ArchivedMods/example.charge-ui');
    const mod=await p.indexMod(directory);
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(p.analyze(await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8'),mod).issues,[]);
});
test('branching mode example validates its callback and roster', async () => {
    const directory=path.resolve(__dirname,'../../../ArchivedMods/example.branching-trial');
    const mod=await p.indexMod(directory);
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(p.analyze(await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('saved random streams require both capabilities and seeded starter validates', async () => {
    const directory=path.resolve(__dirname,'../../../ArchivedMods/example.seeded-trial');
    const mod=await p.indexMod(directory);
    assert.deepEqual(mod.issues,[]);
    const source=await fs.readFile(path.join(directory,'scripts/main.lua'),'utf8');
    assert.deepEqual(p.analyze(source,mod).issues,[]);
    const template=path.resolve(__dirname,'../templates/seeded-trial');
    for(const file of ['scripts/main.lua','mod.toml','localizations/eng.toml'])
        assert.equal(await fs.readFile(path.join(template,file),'utf8'),await fs.readFile(path.join(directory,file),'utf8'));
    for(const held of [[],['state.read'],['state.write'],['state.read','state.write']]) {
        const missing=p.analyze(header+"sf2.random.integer('route',1,3)\nsf2.random.number('route')",{...mod,data:{...mod.data,capabilities:held}}).issues;
        for(const cap of ['state.read','state.write'])
            assert.equal(missing.filter(i=>i.capability===cap).length,held.includes(cap)?0:2);
    }
});


test('quest suppression requires patch capability and example validates', async () => {
    const mod=await p.indexMod(path.resolve(__dirname,'../../../ArchivedMods/example.quest-suppression'));
    assert.deepEqual(mod.issues,[]);
    assert.deepEqual(p.analyze(await fs.readFile(path.resolve(__dirname,'../../../ArchivedMods/example.quest-suppression/scripts/main.lua'),'utf8'),mod).issues,[]);
    const starter=await p.indexMod(template);
    const issues=p.analyze(header+'sf2.quests.suppress { target="core:quests/quests.xml/example" }',starter).issues;
    assert(issues.some(i=>i.message.includes('content.patch')));
});


test('animated arena validates typed curve authoring', async () => {
 const dir=path.resolve(__dirname,'../../../ArchivedMods/example.animated-arena');
 const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('dojo selector validates presentation capability and owned UI', async () => {
 const dir=path.resolve(__dirname,'../../../ArchivedMods/example.dojo-selector');
 const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
 const starter=await p.indexMod(template);
 assert(p.analyze(header+'sf2.locations.reset_dojo()',starter).issues.some(i=>i.message.includes('presentation.dojo')));
});

test('DE128 campaign music patches use packaged audio handles', async () => {
    const directory = path.resolve(__dirname, '../../../Mods/de128');
    const mod = await p.indexMod(directory);
    assert.deepEqual(mod.issues, []);
    const source = await fs.readFile(path.join(directory, 'scripts/content/campaign_music.lua'), 'utf8');
    assert.deepEqual(p.analyze(source, mod).issues, []);
    for (const track of ['ninja_in_the_night_old', 'old_sensei_old', 'deadly_smoke_old', 'burning_town_old'])
        assert.equal(mod.assets.get(`audio/campaign/${track}`).kind, 'audio');
});

test('DE128 native map button uses a typed packaged sprite and click notification', async () => {
 const dir=path.resolve(__dirname,'../../../Mods/de128');
 const mod=await p.indexMod(dir);
 assert.deepEqual(mod.issues,[]);
 const source=await fs.readFile(path.join(dir,'scripts/content/dojo_changer.lua'),'utf8');
 assert.deepEqual(p.analyze(source,mod).issues,[]);
 assert.equal(mod.assets.get('sprites/dojo_changer/button').kind,'sprite');
 assert.equal(mod.assets.get('sprites/dojo_changer/button_pressed').kind,'sprite');
 const api=require('../data/api.json');
 assert.equal(api.types.Action_show_map_button.fields.image,'Eclipse.SpriteHandle|string');
 assert.match(api.types.StoryEvent.fields.kind,/map_button/);
});

test('profile queries require read capability', async () => {
 const mod=await p.indexMod(template);
 for(const call of ['sf2.profile.level()','sf2.profile.item(item)','sf2.profile.fight("core:fights/zone_1/tournament/3")'])
  assert(p.analyze(header+call,mod).issues.some(i=>i.message.includes('profile.read')));
});

test('story subscriptions require event capability', async () => {
 const mod=await p.indexMod(template);
 for(const call of ['sf2.story.on("purchase", function(event) end)','sf2.story.off(subscription)','sf2.story.is_active(subscription)'])
  assert(p.analyze(header+call,mod).issues.some(i=>i.message.includes('story.events')));
});

test('battle progression operations require story progression capability', async () => {
 const mod=await p.indexMod(template);
 for(const call of ['sf2.battles.set_locked(battle,false)','sf2.battles.reveal(battle,false)','sf2.battles.focus(battle)','sf2.profile.set_eclipse_mode(false)'])
  assert(p.analyze(header+call,mod).issues.some(i=>i.message.includes('story.progression')));
});

test('story observer example validates', async () => {
 const dir=path.resolve(__dirname,'../../../ArchivedMods/example.story-observer');
 const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('scene navigation capability and example validate', async () => {
 const starter=await p.indexMod(template);
 assert(p.analyze(header+'sf2.scenes.open("shop")',starter).issues.some(i=>i.message.includes('presentation.navigate')));
 const dir=path.resolve(__dirname,'../../../ArchivedMods/example.scene-menu');
 const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
});


test('Eclipse reward example validates its manifest and reward patch', async () => {
 const dir=path.resolve(__dirname,'../../../ArchivedMods/example.eclipse-reward');
 const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('katana achievement example validates', async () => {
 const dir=path.resolve(__dirname,'../../../ArchivedMods/example.katana-achievement');
 const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('shifting guardian validates the form request contract', async () => {
 const dir=path.resolve(__dirname,'../../../ArchivedMods/example.shifting-guardian');
 const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
});

test('sequence playback has compact typed steps and declares UI and saved-state capabilities', async () => {
    const api=require('../scripts/api-schema.cjs');
    assert.deepEqual(api.functions['sf2.story.play_sequence'].capability,['story.events','ui.create']);
    assert.equal(api.types.StorySequenceDefinition.fields['position?'],'string');
    for(const name of ['MoveEvent','MovePoint','MoveScheduledAction','MoveDamageTerm','MoveTacticDistance'])assert.equal(api.types[name],undefined);
    const mod=await p.indexMod(template);
    const issues=p.analyze(header+'sf2.story.play_sequence { position="next", steps={} }',mod).issues;
    for(const cap of ['story.events','ui.create','state.read','state.write'])assert(issues.some(i=>i.capability===cap));
});


test('audio instances have distinct handles, bounds, capabilities and a mirrored starter', async()=>{
 const schema=require('../scripts/api-schema.cjs');
 assert.equal(schema.functions['sf2.audio.play'].returns,'Eclipse.AudioInstanceHandle|nil, string|nil');
 for(const name of ['play','is_playing','set_volume','stop'])assert.equal(schema.functions['sf2.audio.'+name].capability,'audio.play');
 const starter=await p.indexMod(template);
 assert(p.analyze(header+'sf2.audio.play(clip,{volume=1.1})',starter).issues.some(i=>i.code==='range'));
 assert(p.analyze(header+'sf2.audio.set_volume(sound,-1)',starter).issues.some(i=>i.code==='range'));
 assert(p.analyze(header+'sf2.audio.stop(sound)',starter).issues.some(i=>i.capability==='audio.play'));
 const dir=path.resolve(__dirname,'../../../Mods/example.audio-lab');const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua','assets/audio/beacon.wav'])assert.deepEqual(await fs.readFile(path.join(dir,file)),await fs.readFile(path.resolve(__dirname,'../templates/audio-lab',file)));
});


test('Pulse Arena ships mirrored typed marker/sensor example and capabilities',async()=>{
 const dir=path.resolve(__dirname,'../../../Mods/example.pulse-arena');const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 const lua=await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8');assert.deepEqual(p.analyze(lua,mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua','assets/textures/pulse.png',...Array.from({length:4},(_,i)=>'assets/sprites/pulse_'+(i+1)+'.asset')])assert.deepEqual(await fs.readFile(path.join(dir,file)),await fs.readFile(path.resolve(__dirname,'../templates/pulse-arena',file)));
 const api=require('../data/api.json');assert.equal(api.fighterMethods.mark_rect.capability,'presentation.visuals');assert.equal(api.fighterMethods.overlaps_rect.returns,'boolean|nil, string|nil');
 for(const name of ['remove_marker','set_marker_color','is_marker_active','set_marker_rect','set_marker_sprite'])assert.equal(api.functions['sf2.world.'+name].capability,'presentation.visuals');
 const missing={...mod,data:{...mod.data,capabilities:['content.register']}};const issues=p.analyze('local sf2=require("sf2")\nsf2.behaviors.register{id="x",on_round_begin=function(_,fighter) fighter:mark_rect{x=0,y=0,width=1,height=1} end}\nsf2.world.remove_marker({})',missing).issues;
 assert(issues.some(i=>i.capability==='presentation.visuals'));assert(issues.some(i=>i.code==='callback-timing'));
 assert.equal(api.fighterMethods.mark_sprite.params.sprite,'Eclipse.SpriteHandle');
 const badTiming=p.analyze('local sf2=require("sf2");sf2.behaviors.register{id="x",on_round_end=function(_,fighter) fighter:mark_sprite({}, {x=0,y=0,width=1,height=1}) end}',mod);
 assert(badTiming.issues.some(i=>i.code==='callback-timing'&&i.message.includes('mark_sprite')));
});


test('scripted actors expose copied provenance and ship a mirrored programmable starter',async()=>{
 const api=require('../data/api.json');assert.equal(api.types.FighterSnapshot.fields['actor?'],'Eclipse.ActorIdentity');
 assert.deepEqual(Object.keys(api.types.ActorIdentity.fields),['id','definition','owner','team']);
 const dir=path.resolve(__dirname,'../../../Mods/example.scripted-actors');const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 assert.deepEqual(p.analyze(await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8'),mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua'])assert.deepEqual(await fs.readFile(path.join(dir,file)),await fs.readFile(path.resolve(__dirname,'../templates/scripted-actors',file)));
});

test('actor behavior callbacks infer self references and enforce their capability and terminal lifetime',()=>{
 const api=require('../data/api.json');for(const callback of ['on_actor_spawn','on_actor_end'])assert(api.callbacks.includes(callback));
 assert.equal(api.types.ActorDefinition.fields['behavior?'][0],'Eclipse.BehaviorHandle');
 const mod={data:{id:'fixture',capabilities:['content.register']},assets:new Map(),localizations:new Map()};
 const source='local sf2=require("sf2");sf2.behaviors.register{id="host",on_actor_spawn=function(_,fighter) local own=fighter.actor; own:snapshot() end,on_actor_end=function(_,fighter) fighter.actor:remove() end}';
 const result=p.analyze(source,mod);
 assert.equal(result.issues.filter(issue=>issue.capability==='combat.actors').length,2);
 assert(result.issues.some(issue=>issue.code==='callback-timing'&&issue.message.includes('remove')));
 assert.deepEqual(result.contexts.map(context=>context.callback),['on_actor_spawn','on_actor_end']);
});

test('ranged actor starter mirrors flight assets and terminal projectile commands are rejected',async()=>{
 const dir=path.resolve(__dirname,'../../../Mods/example.ranged-actors');const mod=await p.indexMod(dir);assert.deepEqual(mod.issues,[]);
 const source=await fs.readFile(path.join(dir,'scripts/main.lua'),'utf8');assert.deepEqual(p.analyze(source,mod).issues,[]);
 for(const file of ['mod.toml','README.md','scripts/main.lua','assets/animations/shuriken_fly.bytes'])assert.deepEqual(await fs.readFile(path.join(dir,file)),await fs.readFile(path.resolve(__dirname,'../templates/ranged-actors',file)));
 const missing={...mod,data:{...mod.data,capabilities:mod.data.capabilities.filter(cap=>cap!=='combat.projectiles')}};
 assert(p.analyze(source,missing).issues.some(issue=>issue.capability==='combat.projectiles'));
 const terminal='local sf2=require("sf2");sf2.behaviors.register{id="x",on_actor_end=function(_,fighter) fighter:projectiles();fighter:spawn_projectile(dart,0,0) end}';
 assert.equal(p.analyze(terminal,mod).issues.filter(issue=>issue.code==='callback-timing').length,2);
});

const patchMod={data:{id:'test.patches',capabilities:['content.patch','content.register','assets.replace'],dependencies:[]},assets:new Map(),localizations:new Map()};
const nativeIssues=source=>p.analyze(header+source,patchMod).issues.filter(i=>i.code.startsWith('native'));
test('vanilla move data is generated from the current XML',t=>{
 const {spawnSync}=require('node:child_process'),script=path.resolve(__dirname,'../../Audits/QueryMoves.py');
 const python=['python3','python'].find(name=>!spawnSync(name,['--version']).error);
 if(!python){t.skip('Python is unavailable');return;}
 const result=spawnSync(python,[script,'export-index','--check'],{encoding:'utf8'});
 assert.equal(result.status,0,result.stdout+result.stderr);
});
test('documented move patches pass vanilla guards and mismatches are reported',()=>{
 for(const source of [
  'sf2.moves.patch{move="RangedHeavyPlayer",interval_end={name="Uninterrupt",expected=42,value=40}}',
  'sf2.moves.patch{move="ChakramFly",hit={expected="High",value="MiddleShortPlus"}}',
  'sf2.moves.patch{move="ShopRangedTryOnHeavyPlayer",sound_frame={name="snd_disk",expected=18,value=16}}',
  'sf2.moves.patch{move="FrontKick",add_interval={name="SemiUninterrupt",start=0,["end"]=4},interval_start={name="Uninterrupt",expected=0,value=5}}',
  'sf2.moves.patch{move="LightingChainPlayer",input={expected="Super",value="RaidCharge"},priority={expected=9000,value=200},interval_start={name="Uninterrupt",expected=9,value=0}}',
  'sf2.moves.patch{move="RatWavePlayer",input={expected="Up",value="RaidCharge"},animation={expected="rats_wave.bytes",value=clip}}',
  'sf2.moves.patch{move="PerkFearRayPlayer",remove_interval={name="Evade",type="Invulnerable",start=0,["end"]=47}}',
  'sf2.moves.replace{id="x",target="HighKick",expected_file="high_kick.bytes",animation=clip}',
  'sf2.moves.patch{move=name,priority={expected=1,value=2}}',
 ])assert.deepEqual(nativeIssues(source),[],source);
 const messages=source=>nativeIssues(source).map(i=>i.message);
 assert.match(messages('sf2.moves.patch{move="Highkick",disable=true}')[0],/No vanilla move is named "Highkick"/);
 assert.match(messages('sf2.moves.patch{move="HighKick",priority={expected=100,value=90}}')[0],/priority 110, not 100/);
 assert.match(messages('sf2.moves.patch{move="RangedHeavyPlayer",interval_end={name="Uninterrupt",expected=41,value=40}}')[0],/ends at 42, not 41/);
 assert.match(messages('sf2.moves.patch{move="HighKick",hit={expected="Low",value="High"}}')[0],/hits with "High", not "Low"/);
 assert.match(messages('sf2.moves.patch{move="HighKick",sound_frame={name="snd_swish5",expected=3,value=2}}')[0],/frame 4, not 3/);
 assert.match(messages('sf2.moves.patch{move="HighKick",input={expected="Punch",value="Kick"}}')[0],/uses Kick, not "Punch"/);
 assert.match(messages('sf2.moves.patch{move="PerkFearRayPlayer",remove_interval={name="Evade",type="Invulnerable",start=0,["end"]=46}}')[0],/must match exactly/);
 assert.match(messages('sf2.moves.patch{move="FrontKick",add_interval={name="Uninterrupt",start=0,["end"]=4}}')[0],/already has an interval named "Uninterrupt"/);
 assert.match(messages('sf2.moves.replace{id="x",target="HighKick",expected_file="x.bytes",animation=clip}')[0],/plays "high_kick.bytes"/);
 assert.match(messages('sf2.moves.patch{move="HighKick",interval_start={name="Block",expected=16,value=20}}')[0],/no "Block" interval/);
});
test('move fields complete vanilla names and hover summarizes the target',()=>{
 const native=require('../src/native.cjs');
 const at=(source,offset=source.length)=>p.moveFieldContext(header+source,header.length+offset,patchMod);
 let context=at('sf2.moves.patch{move="HighK');assert.deepEqual([context.name,context.path,context.prefix],['sf2.moves.patch',['move'],'HighK']);
 assert(native.completions(context.name,context.path,context.definition,p.literal).some(c=>c.label==='HighKick'));
 context=at('sf2.moves.patch{move="HighKick",interval_end={name="');
 assert.deepEqual(native.completions(context.name,context.path,context.definition,p.literal).map(c=>c.label),['Unstable','Uninterrupt']);
 context=at('sf2.moves.replace{target="HighKick",expected_file="');
 assert.deepEqual(native.completions(context.name,context.path,context.definition,p.literal).map(c=>c.label),['high_kick.bytes']);
 const source='sf2.moves.patch{move="HighKick"}';
 const hover=p.moveFieldContext(header+source,header.length+source.indexOf('High')+2,patchMod,false);
 assert.equal(hover.value,'HighKick');
 assert.match(native.describe(hover.value),/Priority: 110[\s\S]*Interval Attack 6\.\.8 · hit High[\s\S]*Attack id 0: damage 0.12/);
});
test('exclusive claims conflict across mods, within a mod, and by sibling mod ID',async t=>{
 const claims=require('../src/claims.cjs'),report=require('../src/report.cjs');
 const root=await fs.mkdtemp(path.join(os.tmpdir(),'eclipse-claims-'));t.after(()=>fs.rm(root,{recursive:true,force:true}));
 const make=async(id,lua)=>{const dir=await createMod(template,root,id,id,'Author');
  const manifest=path.join(dir,'mod.toml');await fs.writeFile(manifest,(await fs.readFile(manifest,'utf8')).replace('capabilities = [','capabilities = ["content.patch", "assets.replace", '));
  await fs.appendFile(path.join(dir,'scripts/main.lua'),'\n'+lua+'\n');return p.indexMod(dir);};
 const alice=await make('alice.tuning','sf2.moves.patch{move="HighKick",priority={expected=110,value=90}}\nsf2.assets.replace{target="core:Sprites\\\\Icon",replacement="sprites/weapon"}\nsf2.moves.replace{id="step",target="FrontKick",expected_file="front_kick.bytes",animation=clip}');
 const bob=await make('bob.tuning','sf2.moves.patch{move="HighKick",disable=true}\nsf2.assets.replace{target="core:sprites/icon",replacement="sprites/weapon"}\nsf2.localization.patch{target="core:localization/x",language="ENG",value="a"}\nsf2.localization.patch{target="core:localization/x",language="eng",value="b"}');
 assert.deepEqual(alice.claims.map(c=>c.key),['move-patch:HighKick','asset-replace:core:sprites/icon','move-replace:FrontKick']);
 const result=claims.conflicts([alice,bob]);
 const keys=result.claims.map(c=>`${c.mod.data.id} ${c.claim.key} ${c.duplicate}`).sort();
 assert.deepEqual(keys,['alice.tuning asset-replace:core:sprites/icon false','alice.tuning move-patch:HighKick false','bob.tuning asset-replace:core:sprites/icon false','bob.tuning localization:core:localization/x|eng true','bob.tuning localization:core:localization/x|eng true','bob.tuning move-patch:HighKick false']);
 assert.match(claims.message(result.claims.find(c=>c.mod===alice&&c.claim.kind==='move patch')),/also claimed by "bob.tuning"/);
 // Copies of one mod never conflict with each other; siblings with one ID are reported.
 const copy={...alice,root:path.join(root,'copy','alice.tuning')};
 assert.deepEqual(claims.conflicts([alice,copy]).claims,[]);
 assert.deepEqual(claims.conflicts([alice,copy]).duplicateIds,[]);
 const sibling={...alice,root:path.join(root,'alice-again')};
 assert.equal(claims.conflicts([alice,sibling]).duplicateIds.length,2);
 const markdown=report.moveReport(alice,p,[bob]);
 assert.match(markdown,/\| Priority \| 110 \| 90 \|/);
 assert.match(markdown,/\| Intervals \| .*Attack \| \*\*omitted — not inherited\*\* \|/);
 assert.match(markdown,/Move patch "HighKick" is also claimed by "bob.tuning"/);
});
test('list-based move patch fields are checked against vanilla data and reported',()=>{
 const report=require('../src/report.cjs');
 const good='sf2.moves.patch{move="HighKick",playback_rate={expected=1.0,value=1.25},intervals={{select={name="Uninterrupt",start=0,["end"]=15},["end"]=12},{select={type="Block",start=16},remove=true},{add={type="Invulnerable",name="Dodge",start=0,["end"]=3}}},'+
  'attacks={{id=0,damage={expected=0.12,value=0.2},damage_terms={expected={UnarmedDamage=0},value={UnarmedDamage=0,WeaponDamage=0}},edges={expected={"EThigh_2","ECalf_2","EInstep_2","EToe_2","EFoot_2"},value={"EFoot_2"}},impulse={expected={x=245,y=0,z=350},value={x=300}},hit={expected="High",value="Middle"},start={expected=6,value=5}}}}';
 assert.deepEqual(nativeIssues(good),[]);
 const messages=nativeIssues('sf2.moves.patch{move="HighKick",playback_rate={expected=1.0,value=1.5},intervals={{select={type="Block",start=16,["end"]=20},remove=true}},attacks={{id=4,damage={expected=0.1,value=0.2}},{id=0,damage={expected=0.13,value=0.2}}}}').map(i=>i.message);
 assert(messages.some(m=>/No vanilla "HighKick" interval matches Block 16\.\.20/.test(m)));
 assert(messages.some(m=>/no attack with id 4 \(ids: 0\)/.test(m)));
 assert(messages.some(m=>/attack 0 has damage 0\.12/.test(m)));
 assert.match(nativeIssues('sf2.moves.patch{move="SaturnProjectileStart1",playback_rate={expected=1.0,value=1.5}}')[0]?.message??'',/unavailable for looped/);
 const mod={...patchMod,root:'/tmp/x',moveCalls:[],sources:new Map([['/tmp/x/scripts/main.lua',header+good]]),claims:[]};
 const call=p.analyze(header+good,mod).calls.find(c=>c.name==='sf2.moves.patch');
 mod.moveCalls.push({...call,file:'/tmp/x/scripts/main.lua',line:1});
 const markdown=report.moveReport(mod,p);
 assert.match(markdown,/\| Speed \| 1\.0 \| 1\.25× \|/);
 assert.match(markdown,/\| Interval Block \| 16\.\.open \| \*\*removed\*\* \|/);
 assert.match(markdown,/\| Attack 0 damage \| `0\.12` \| `0\.2` \|/);
});
test('data-only moveset mods index, validate against vanilla and claim forks',async t=>{
 const moveset=require('../src/moveset.cjs'),claims=require('../src/claims.cjs');
 assert.deepEqual(p.manifest('schema = 1\nid = "x.y"\nname = "X"\nversion = "1.0.0"\nauthors = ["A"]\ncapabilities = []\n').issues,[]);
 const starter=await p.indexMod(path.resolve(__dirname,'../templates/moveset'));
 assert.deepEqual(starter.issues,[]);
 assert.deepEqual(starter.claims.map(c=>c.key),['move-patch:HighKick','lock-removal:KatanaHeavySlash|Weapon|NinjaSword','move-patch:tutorial.moveset.KatanaHeavySlash_Ninja']);
 const head='{"schema":1,"kind":"eclipse.moveset",';
 const issues=text=>moveset.parse(text).issues.map(i=>i.message);
 assert.match(issues(head+'"moves":[{"move":"A","move":"B","disable":true}]}').join(),/Duplicate field "move"/);
 assert.match(issues(head+'"moves":[{"move":"A","colour":1}]}').join(),/Unknown field "colour"/);
 assert.match(issues(head+'"moves":[{"move":"A","priority":{"expected":1.5,"value":2}}]}').join(),/must be an integer/);
 assert.match(issues(head+'"moves":[{"move":"A"}]}').join(),/must change at least one field/);
 assert.match(issues(head+'"forks":[{"id":"F","move":"A"}]}').join(),/exactly one of "subtype" or "item"/);
 assert.match(issues('{"schema":1,"kind":"eclipse.moveset",}').join(),/Invalid JSON/);
 const root=await fs.mkdtemp(path.join(os.tmpdir(),'eclipse-moveset-'));t.after(()=>fs.rm(root,{recursive:true,force:true}));
 const make=async(id,json,extra='')=>{const dir=path.join(root,id);await fs.mkdir(path.join(dir,'movesets'),{recursive:true});
  await fs.writeFile(path.join(dir,'mod.toml'),`schema = 1\nid = "${id}"\nname = "${id}"\nversion = "1.0.0"\nauthors = ["A"]\ncapabilities = ["content.patch"]\n${extra}`);
  await fs.writeFile(path.join(dir,'movesets/moveset.json'),json);return p.indexMod(dir);};
 const bad=await make('bad.moves',head+'\n"moves":[{"move":"HighKick",\n"priority":{"expected":100,"value":90}}],\n"forks":[{"id":"F","move":"HighKick","subtype":"Katana"},{"id":"G","move":"KatanaHeavySlash","item":"core:items/weapon/weapon_golden_katana"}]}');
 const messages=bad.issues.map(i=>`${i.line}:${i.message}`);
 assert(messages.some(m=>/^2:.*priority 110, not 100/.test(m)),messages.join('\n'));
 assert(messages.some(m=>/not locked to subtype "Katana".*fork it for an item instead/.test(m)),messages.join('\n'));
 assert(messages.some(m=>/needs a \[\[dependencies\]\] entry for "core"/.test(m)),messages.join('\n'));
 const a=await make('alice.moves',head+'"forks":[{"id":"N","move":"KatanaHeavySlash","subtype":"NinjaSword"}]}');
 const b=await make('bob.moves',head+'"forks":[{"id":"M","move":"KatanaHeavySlash","subtype":"NinjaSword"}]}');
 const conflict=claims.conflicts([a,b]).claims.find(c=>c.mod===a);
 assert.match(claims.message(conflict),/Subtype fork "KatanaHeavySlash for NinjaSword" is also claimed by "bob.moves"/);
 const schema=require('../schemas/moveset.schema.json'),api=require('../data/api.json');
 assert.deepEqual(schema.properties.moves.items.properties.attacks.items.properties.hit.properties.value.enum,JSON.parse('['+api.types.MoveHitPatch.fields.expected.split('|').join(',')+']'));
 const forkLua=p.analyze(header+'local n=sf2.moves.fork{id="F",source="HighKick",subtype="Katana"}\nsf2.moves.patch{move="test.patches.F",priority={expected=1,value=2}}',patchMod).issues.filter(i=>i.code.startsWith('native')).map(i=>i.message);
 assert.equal(forkLua.length,1);assert.match(forkLua[0],/not locked to subtype "Katana"/);
});

test('moveset key inputs and new moves', async () => {
 const moveset=require('../src/moveset.cjs');
 const head='{"schema":1,"kind":"eclipse.moveset",';
 const issues=text=>moveset.parse(text).issues.map(i=>i.message);
 // Chord alternatives, press types and the legacy single-key string all parse.
 assert.deepEqual(issues(head+'"moves":[{"move":"KatanaHeavySlash","input":{"expected":[["Punch",{"key":"Forward","press":"Hold"}]],"value":[["Kick",{"key":"Back","press":"Hold"}],["Up-Back"]]}}]}'),[]);
 assert.deepEqual(issues(head+'"moves":[{"move":"HighKick","input":{"expected":"Kick","value":[]}}]}'),[]);
 assert.match(issues(head+'"moves":[{"move":"A","input":{"expected":[["Jump"]],"value":"Kick"}}]}').join(),/not a key name/);
 assert.match(issues(head+'"moves":[{"move":"A","input":{"expected":[["Kick",{"key":"Back","press":"Twice"}]],"value":"Kick"}}]}').join(),/press must be "Tap", "Hold" or "Release"/);
 // A new move may skip subtype and item, but not name both.
 assert.deepEqual(issues(head+'"forks":[{"id":"rising","move":"KatanaUpperSlash","add":true}]}'),[]);
 assert.match(issues(head+'"forks":[{"id":"rising","move":"A","add":true,"subtype":"Katana","item":"core:items/weapon/x"}]}').join(),/at most one of "subtype" or "item"/);
 // Chords compare with vanilla: KatanaDoubleSlash is two Punch taps.
 const native=require('../src/native.cjs');
 const parsed=moveset.parse(head+'"moves":[{"move":"KatanaDoubleSlash","input":{"expected":[["Punch"]],"value":[["Kick"]]}}]}');
 const helpers={literal:n=>n&&('value' in n)&&n.type!=='TableConstructorExpression'?n.value:undefined,
  fields:t=>Object.fromEntries((t?.fields??[]).filter(f=>f.type==='TableKeyString').map(f=>[f.key.name,f.value]))};
 const found=moveset.check(parsed.entries,helpers,'test.mod').map(i=>i.message).join('\n');
 assert.match(found,/uses Punch \+ Punch, not "Punch"/);
});
