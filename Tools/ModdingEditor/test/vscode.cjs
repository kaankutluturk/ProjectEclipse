const vscode = require('vscode');
const assert = require('node:assert/strict');
const path = require('node:path');
const fs = require('node:fs');

exports.run = async function () {
    const root = path.resolve(__dirname, '..');
    const report = path.join(root, '.test-runtime/vscode-result.txt');
    const passed = [];
    try {
        const folder = vscode.workspace.workspaceFolders[0];
        const config = vscode.workspace.getConfiguration('Lua', folder.uri);
        const otherLibrary = path.join(folder.uri.fsPath, 'other-library');
        fs.mkdirSync(otherLibrary, { recursive: true });
        await config.update('workspace.library', [otherLibrary], vscode.ConfigurationTarget.WorkspaceFolder);
        const extension = vscode.extensions.getExtension('eclipse-modding.eclipse-modding-preview');
        assert(extension, 'Preview extension was not loaded');
        const library = path.join(extension.extensionUri.fsPath, 'library');
        await extension.activate();
        const authoredRoot=vscode.Uri.joinPath(folder.uri,'authored-lab');
        fs.cpSync(path.join(root,'templates/authored-fighter'),authoredRoot.fsPath,{recursive:true});
        const authoredUri=vscode.Uri.joinPath(authoredRoot,'scripts','main.lua');
        const authoredDocument=await vscode.workspace.openTextDocument(authoredUri);
        await vscode.window.showTextDocument(authoredDocument);
        await extension.exports.refresh();
        const authoredManifest=path.join(authoredRoot.fsPath,'mod.toml');
        const authoredManifestText=fs.readFileSync(authoredManifest,'utf8');
        fs.writeFileSync(authoredManifest,authoredManifestText.replace(', "combat.actors"',''));
        await extension.exports.refresh();
        const authoredDeadline=Date.now()+15000;
        while(!vscode.languages.getDiagnostics(authoredUri).some(d=>d.code==='capability:combat.actors')&&Date.now()<authoredDeadline)
            await new Promise(resolve=>setTimeout(resolve,100));
        assert(vscode.languages.getDiagnostics(authoredUri).some(d=>d.code==='capability:combat.actors'),'Authored starter was not analyzed in its own manifest scope');
        fs.writeFileSync(authoredManifest,authoredManifestText);await extension.exports.refresh();
        const authoredCleanDeadline=Date.now()+15000;
        while(vscode.languages.getDiagnostics(authoredUri).length&&Date.now()<authoredCleanDeadline)
            await new Promise(resolve=>setTimeout(resolve,100));
        assert.deepEqual(vscode.languages.getDiagnostics(authoredUri),[],'Complete authored starter has VS Code project diagnostics');
        fs.writeFileSync(authoredManifest,authoredManifestText.replace(', "combat.transform"',''));
        await extension.exports.refresh();
        const formDeadline=Date.now()+15000;
        while(!vscode.languages.getDiagnostics(authoredUri).some(d=>d.code==='capability:combat.transform')&&Date.now()<formDeadline)
            await new Promise(resolve=>setTimeout(resolve,100));
        assert(vscode.languages.getDiagnostics(authoredUri).some(d=>d.code==='capability:combat.transform'),'Player form calls did not require their own capability');
        fs.writeFileSync(authoredManifest,authoredManifestText);await extension.exports.refresh();
        const formCleanDeadline=Date.now()+15000;
        while(vscode.languages.getDiagnostics(authoredUri).length&&Date.now()<formCleanDeadline)
            await new Promise(resolve=>setTimeout(resolve,100));
        assert.deepEqual(vscode.languages.getDiagnostics(authoredUri),[],'Restored player form capability did not clear diagnostics');
        passed.push('PASS: authored player form permission is independently validated');
        passed.push('PASS: Authored Fighter Lab native assets/script validate and actor permission follows its own manifest');
        const playerFieldUri=vscode.Uri.joinPath(authoredRoot,'scripts','player-field.lua');
        const playerFieldText='local sf2=require("sf2")\nsf2.fights.register { id="test",  }';
        fs.writeFileSync(playerFieldUri.fsPath,playerFieldText);
        await vscode.workspace.openTextDocument(playerFieldUri);
        let playerFieldFound=false;const playerFieldDeadline=Date.now()+30000;
        while(Date.now()<playerFieldDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',playerFieldUri,new vscode.Position(1,playerFieldText.split('\n')[1].indexOf('  }')+1));
            if(result?.items.some(item=>String(typeof item.label==='string'?item.label:item.label.label).replace(/\?$/,'')==='player_character')){playerFieldFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,300));
        }
        assert(playerFieldFound,'Owned fight player_character completion missing');
        passed.push('PASS: owned fight player character field completes in the authored starter');
        const playerPlanUri=vscode.Uri.joinPath(authoredRoot,'scripts','player-plan.lua');
        const playerPlanText='local sf2=require("sf2")\nsf2.modes.register{id="test",fights={},on_prepare=function(request)\n sf2.modes.resolve(request,{  })\nend}';
        fs.writeFileSync(playerPlanUri.fsPath,playerPlanText);await vscode.workspace.openTextDocument(playerPlanUri);
        let playerPlanFound=false;const playerPlanDeadline=Date.now()+30000;
        while(Date.now()<playerPlanDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',playerPlanUri,new vscode.Position(2,playerPlanText.split('\n')[2].indexOf('  }')+1));
            if(result?.items.some(item=>String(typeof item.label==='string'?item.label:item.label.label).replace(/\?$/,'')==='player_character')){playerPlanFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,300));
        }
        assert(playerPlanFound,'Prepared encounter player_character completion missing');
        passed.push('PASS: prepared player character field completes inside the real mode callback');
        await vscode.commands.executeCommand('eclipseModding.enable');
        await vscode.commands.executeCommand('eclipseModding.enable');
        const expected = [otherLibrary, library];
        assert.deepEqual(vscode.workspace.getConfiguration('Lua', folder.uri).get('workspace.library'), expected);
        passed.push('PASS: enable is idempotent and preserves other libraries');

        const document = await vscode.workspace.openTextDocument(vscode.Uri.joinPath(folder.uri, 'probe.lua'));
        await vscode.window.showTextDocument(document);
        let found = false;
        const deadline = Date.now() + 45000;
        while (Date.now() < deadline) {
            const result = await vscode.commands.executeCommand('vscode.executeCompletionItemProvider', document.uri, new vscode.Position(1, 10));
            if (result?.items.some(item => String(typeof item.label === 'string' ? item.label : item.label.label).startsWith('register_weapon'))) {
                found = true;
                break;
            }
            await new Promise(resolve => setTimeout(resolve, 500));
        }
        assert(found, 'VS Code did not offer register_weapon through the installed Lua extension');
        passed.push('PASS: real VS Code Lua extension provides weapon completion after enabling the preview');

        const serviceUri = vscode.Uri.joinPath(folder.uri, 'scripts', 'service-completion.lua');
        fs.writeFileSync(serviceUri.fsPath, 'local sf2=require("sf2")\nsf2.extensions.');
        await vscode.workspace.openTextDocument(serviceUri);
        let servicesFound = false;
        const serviceDeadline = Date.now() + 30000;
        while (Date.now() < serviceDeadline) {
            const result = await vscode.commands.executeCommand('vscode.executeCompletionItemProvider', serviceUri, new vscode.Position(1, 15));
            const labels = result?.items.map(item => String(typeof item.label === 'string' ? item.label : item.label.label)) ?? [];
            if (['register', 'get', 'call', 'try_call'].every(name => labels.some(label => label === name || label.startsWith(name + '(')))) {
                servicesFound = true;
                break;
            }
            await new Promise(resolve => setTimeout(resolve, 500));
        }
        assert(servicesFound, 'Framework service function completion missing');
        passed.push('PASS: framework service registration, lookup and calls complete');

        const arenaUri=vscode.Uri.joinPath(folder.uri,'scripts','arena-completion.lua');
        fs.writeFileSync(arenaUri.fsPath,'local sf2=require("sf2")\nsf2.world.');await vscode.workspace.openTextDocument(arenaUri);
        let arenaFound=false;const arenaDeadline=Date.now()+30000;
        while(Date.now()<arenaDeadline){const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',arenaUri,new vscode.Position(1,10));const labels=result?.items.map(item=>String(typeof item.label==='string'?item.label:item.label.label))??[];if(['remove_marker','is_marker_active','set_marker_color','set_marker_rect','set_marker_sprite'].every(name=>labels.some(label=>label===name||label.startsWith(name+'(')))){arenaFound=true;break;}await new Promise(resolve=>setTimeout(resolve,500));}
        assert(arenaFound,'Owned arena marker completion missing');passed.push('PASS: owned arena marker completion');
        const audioUri=vscode.Uri.joinPath(folder.uri,'scripts','audio-completion.lua');
        fs.writeFileSync(audioUri.fsPath,'local sf2=require("sf2")\nsf2.audio.');await vscode.workspace.openTextDocument(audioUri);
        let audioFound=false;const audioDeadline=Date.now()+30000;
        while(Date.now()<audioDeadline){const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',audioUri,new vscode.Position(1,10));const labels=result?.items.map(item=>String(typeof item.label==='string'?item.label:item.label.label))??[];if(['play','stop','set_volume','is_playing'].every(name=>labels.some(label=>label===name||label.startsWith(name+'(')))){audioFound=true;break;}await new Promise(resolve=>setTimeout(resolve,500));}
        assert(audioFound,'Owned audio function completion missing');passed.push('PASS: owned audio play/query/volume/stop completion');

        const identityUri=vscode.Uri.joinPath(folder.uri,'scripts','identity-completion.lua');
        const identityLine=' if actor then actor. end';
        fs.writeFileSync(identityUri.fsPath,'local sf2=require("sf2")\nsf2.tactics.register{id="brain",on_decide=function(memory,event)\n local actor=event.self.actor\n'+identityLine+'\nend}');
        await vscode.workspace.openTextDocument(identityUri);
        let identityFound=false;const identityDeadline=Date.now()+30000;
        while(Date.now()<identityDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',identityUri,new vscode.Position(3,identityLine.indexOf('actor.')+6));
            const labels=(result?.items||[]).map(item=>typeof item.label==='string'?item.label:item.label.label);
            if(['id','definition','owner','team'].every(name=>labels.includes(name))){identityFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(identityFound,'Copied AI actor identity completion missing');passed.push('PASS: AI actor identity/owner/team completion');
        const actorHostUri=vscode.Uri.joinPath(folder.uri,'scripts','actor-host-completion.lua');
        const actorHostLine=' if fighter.actor then fighter.actor: end';
        fs.writeFileSync(actorHostUri.fsPath,'local sf2=require("sf2")\nsf2.behaviors.register{id="host",on_actor_spawn=function(_,fighter)\n'+actorHostLine+'\nend}');
        await vscode.workspace.openTextDocument(actorHostUri);
        let actorHostFound=false;const actorHostDeadline=Date.now()+30000;
        while(Date.now()<actorHostDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',actorHostUri,new vscode.Position(2,actorHostLine.indexOf('fighter.actor:')+14));
            const labels=(result?.items||[]).map(item=>typeof item.label==='string'?item.label:item.label.label);
            if(['snapshot','move_by','remove','set_target','change_health','play_move','change_form'].every(name=>labels.some(label=>label===name||label.startsWith(name+'(')))){actorHostFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(actorHostFound,'Actor behavior self-reference completion missing');passed.push('PASS: actor spawn callback self-reference methods');
        const actorUri=vscode.Uri.joinPath(folder.uri,'scripts','actor-completion.lua');
        const actorLine=' for _,actor in ipairs(fighter:actors() or {}) do actor: end';
        fs.writeFileSync(actorUri.fsPath,'local sf2=require("sf2")\nsf2.behaviors.register{id="probe",on_tick=function(_,fighter)\n'+actorLine+'\nend}');
        await vscode.workspace.openTextDocument(actorUri);
        let actorsFound=false;const actorsDeadline=Date.now()+30000;
        while(Date.now()<actorsDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',actorUri,new vscode.Position(2,actorLine.indexOf('actor:')+6));
            const labels=result?.items.map(item=>String(typeof item.label==='string'?item.label:item.label.label))??[];
            if(['snapshot','move_by','set_target','remove','change_health','play_move','change_form'].every(name=>labels.some(label=>label===name||label.startsWith(name+'(')))){actorsFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(actorsFound,'Queried independent fighter completion missing');passed.push('PASS: queried actor snapshot/motion/target/health/playback/removal completion');

        const policyUri=vscode.Uri.joinPath(folder.uri,'scripts','rule-policy.lua');
        const policyLine='sf2.fights.patch { target="core:fights/zone_1/tournament/3", append_rules={} }';
        fs.writeFileSync(policyUri.fsPath,'local sf2=require("sf2")\n'+policyLine);
        await vscode.workspace.openTextDocument(policyUri);
        let policyFound=false;const policyDeadline=Date.now()+30000;
        while(Date.now()<policyDeadline){
            const hover=await vscode.commands.executeCommand('vscode.executeHoverProvider',policyUri,new vscode.Position(1,policyLine.indexOf('append_rules')+2));
            if(hover?.some(h=>h.contents.some(c=>(c.value??String(c)).includes('compose across mods')))){policyFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(policyFound,'Additive rule composition guidance missing from field hover');
        passed.push('PASS: fight rule field hover explains mod composition');

        const stainUri = vscode.Uri.joinPath(folder.uri, 'scripts', 'stain-completion.lua');
        fs.writeFileSync(stainUri.fsPath, 'local sf2=require("sf2")\nsf2.fx.stain {\n    \n}');
        await vscode.workspace.openTextDocument(stainUri);
        let stainsFound = false;
        const stainDeadline = Date.now() + 30000;
        while (Date.now() < stainDeadline) {
            const result = await vscode.commands.executeCommand('vscode.executeCompletionItemProvider', stainUri, new vscode.Position(2, 4));
            const labels = (result?.items ?? []).map(item => typeof item.label === 'string' ? item.label : item.label.label);
            if (['speed_min', 'speed_max', 'gravity', 'lift', 'merge_radius', 'max_pool_size'].every(field => labels.some(label => label.startsWith(field))))
            { stainsFound = true; break; }
            await new Promise(resolve => setTimeout(resolve, 500));
        }
        assert(stainsFound, 'Stain flight and accumulation completion missing');
        passed.push('PASS: stain flight and accumulation completion');

        const controlUri=vscode.Uri.joinPath(folder.uri,'scripts','control-completion.lua');
        fs.writeFileSync(controlUri.fsPath,'local sf2=require("sf2")\nsf2.behaviors.register { id="controls",on_round_begin=function(_,fighter)\n fighter:\nend }');
        await vscode.workspace.openTextDocument(controlUri);
        let controlFound=false;
        const controlDeadline=Date.now()+30000;
        while(Date.now()<controlDeadline) {
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',controlUri,new vscode.Position(2,9));
            if(['set_control_blocked','end_round','move_by','play_move','projectiles','mark_rect','mark_sprite','acquire_camera','overlaps_rect'].every(name=>result?.items.some(item=>String(typeof item.label==='string'?item.label:item.label.label).startsWith(name)))){controlFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(controlFound,'Player control restriction completion missing');
        passed.push('PASS: fighter control restriction, round outcome, movement and playback completion');

        const projectileUri=vscode.Uri.joinPath(folder.uri,'scripts','projectile-completion.lua');
        fs.writeFileSync(projectileUri.fsPath,'local sf2=require("sf2")\nsf2.behaviors.register {id="projectiles",on_tick=function(_,fighter)\n local list=fighter:projectiles();if not list then return end\n for _,projectile in ipairs(list) do\n projectile:\n end\nend}');
        await vscode.workspace.openTextDocument(projectileUri);
        let projectileFound=false;const projectileDeadline=Date.now()+30000;
        while(Date.now()<projectileDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',projectileUri,new vscode.Position(4,12));
            if(['snapshot','move_by','remove'].every(name=>result?.items.some(item=>String(typeof item.label==='string'?item.label:item.label.label).startsWith(name)))){projectileFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(projectileFound,'Callback-scoped projectile completion missing');
        passed.push('PASS: owned projectile snapshot/movement/removal completion from fighter query');
        const rangedUri=vscode.Uri.joinPath(folder.uri,'scripts','ranged-host.lua');
        fs.writeFileSync(rangedUri.fsPath,'local sf2=require("sf2")\nsf2.behaviors.register{id="ranged",on_actor_spawn=function(_,fighter)\n fighter:\nend}');
        await vscode.workspace.openTextDocument(rangedUri);
        let rangedFound=false;const rangedDeadline=Date.now()+30000;
        while(Date.now()<rangedDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',rangedUri,new vscode.Position(2,9));
            if(['spawn_projectile','projectiles'].every(name=>result?.items.some(item=>String(typeof item.label==='string'?item.label:item.label.label).startsWith(name)))){rangedFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(rangedFound,'Actor-host projectile methods missing');
        passed.push('PASS: actor spawn projectile creation/query completion');
        const spawnUri=vscode.Uri.joinPath(folder.uri,'scripts','spawn-completion.lua');
        fs.writeFileSync(spawnUri.fsPath,'local sf2=require("sf2")\nlocal flight=sf2.moves.register{id="flight",animation="animations/flight"}\nlocal dart=sf2.projectiles.register{id="dart",name="dart",core_skeleton="SkeletonMissile",start_move=flight,copy_parent_type="Weapon"}\nsf2.behaviors.register{id="spawn",on_tick=function(_,fighter)\n local receipt=fighter:spawn_projectile(dart,0,0)\n local value=receipt.\nend}');
        const spawnDoc=await vscode.workspace.openTextDocument(spawnUri);const spawnPosition=spawnDoc.positionAt(spawnDoc.getText().indexOf('receipt.\n')+8);
        let spawnFound=false;const spawnDeadline=Date.now()+30000;
        while(Date.now()<spawnDeadline){const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',spawnUri,spawnPosition);if(['status','error','projectile_id'].every(key=>result?.items.some(item=>String(typeof item.label==='string'?item.label:item.label.label).startsWith(key)))){spawnFound=true;break;}await new Promise(resolve=>setTimeout(resolve,500));}
        assert(spawnFound,'Direct projectile spawn receipt completion missing');passed.push('PASS: direct projectile spawn receipt completion');
        const attackUri=vscode.Uri.joinPath(folder.uri,'scripts','attack-completion.lua');
        fs.writeFileSync(attackUri.fsPath,'local sf2=require("sf2")\nsf2.behaviors.register {id="attack",on_damage_dealt=function(_,fighter,event)\n local attack=event.attack;if attack then local value=attack. end\nend}');
        const attackDoc=await vscode.workspace.openTextDocument(attackUri);
        const attackPosition=attackDoc.positionAt(attackDoc.getText().indexOf('attack. end')+7);
        let attackFound=false;const attackDeadline=Date.now()+30000;
        while(Date.now()<attackDeadline){
            const result=await vscode.commands.executeCommand('vscode.executeCompletionItemProvider',attackUri,attackPosition);
            if(['kind','model_name','animation_name','point','projectile_id','projectile_owner'].every(key=>result?.items.some(item=>String(typeof item.label==='string'?item.label:item.label.label).startsWith(key)))){attackFound=true;break;}
            await new Promise(resolve=>setTimeout(resolve,500));
        }
        assert(attackFound,'Copied native attack source completion missing');
        passed.push('PASS: native attack source fields infer from damage callback');


        const uri = vscode.Uri.joinPath(folder.uri, 'scripts', 'editor-test.lua');
        fs.writeFileSync(uri.fsPath, 'local sf2 = require("sf2")\nsf2.assets.sprite("sprites/weapon")\nsf2.localization.key("weapon.training_blade")\n');
        const modDoc = await vscode.workspace.openTextDocument(uri);
        await vscode.window.showTextDocument(modDoc);
        const refs = await vscode.commands.executeCommand('vscode.executeCompletionItemProvider', uri, new vscode.Position(1, 22));
        assert(refs.items.some(i => i.label === 'sprites/weapon'), 'Missing real sprite completion');
        const definitions = await vscode.commands.executeCommand('vscode.executeDefinitionProvider', uri, new vscode.Position(1, 24));
        assert(definitions.some(d => (d.uri ?? d.targetUri).fsPath.endsWith('weapon.asset')), 'Sprite definition navigation failed');
        const hover = await vscode.commands.executeCommand('vscode.executeHoverProvider', uri, new vscode.Position(2, 27));
        assert(hover.some(h => h.contents.some(c => (c.value ?? String(c)).replaceAll('&nbsp;', ' ').includes('Training Blade'))), 'Localization hover missing: ' + JSON.stringify(hover.map(h => h.contents.map(c => c.value))));
        passed.push('PASS: local asset completion, go-to-definition, and localization hover');

        const edit = new vscode.WorkspaceEdit();
        edit.insert(uri, new vscode.Position(3, 0), 'sf2.behaviors.register { id="test", on_damage_received=function(params, fighter, event) fighter:change_health(5) end }\n');
        await vscode.workspace.applyEdit(edit);
        await extension.exports.refresh();
        // A debounced document-open/change refresh can supersede the explicit
        // refresh. Wait for the published diagnostic, not one particular run.
        let issues = [];
        const diagnosticDeadline = Date.now() + 10000;
        do {
            issues = vscode.languages.getDiagnostics(uri).filter(d => d.source === 'Eclipse Modding');
            if (issues.some(d => d.code === 'capability:combat.change_life')) break;
            await new Promise(resolve => setTimeout(resolve, 100));
        } while (Date.now() < diagnosticDeadline);
        assert(issues.some(d => d.code === 'capability:combat.change_life'), 'Missing capability diagnostic');
        const fixes = await vscode.commands.executeCommand('vscode.executeCodeActionProvider', uri, issues[0].range);
        const fix = fixes.find(f => f.title === 'Declare combat.change_life in mod.toml');
        assert(fix?.edit, 'Missing capability quick fix');
        await vscode.workspace.applyEdit(fix.edit);
        await extension.exports.refresh();
        // Manifest-change debounce can supersede this refresh as well. Verify
        // the published result, with the same bound as diagnostic creation.
        const removalDeadline = Date.now() + 10000;
        while (vscode.languages.getDiagnostics(uri).some(d => d.code === 'capability:combat.change_life') && Date.now() < removalDeadline)
            await new Promise(resolve => setTimeout(resolve, 100));
        assert(!vscode.languages.getDiagnostics(uri).some(d => d.code === 'capability:combat.change_life'), 'Quick fix did not clear the capability diagnostic');
        passed.push('PASS: missing capability diagnostics and manifest quick fix work with unsaved edits');

        const randomEdit=new vscode.WorkspaceEdit();
        randomEdit.insert(uri,new vscode.Position(modDoc.lineCount,0),'\nsf2.random.integer("route",1,3)\n');
        await vscode.workspace.applyEdit(randomEdit);
        await extension.exports.refresh();
        for(const cap of ['state.read','state.write']) {
            // A pending document/manifest debounce can supersede refresh here too.
            let issue;
            const deadline=Date.now()+10000;
            do {
                issue=vscode.languages.getDiagnostics(uri).find(d=>d.code===`capability:${cap}`);
                if(issue) break;
                await new Promise(resolve=>setTimeout(resolve,100));
            } while(Date.now()<deadline);
            assert(issue,`Missing independent random capability diagnostic: ${cap}`);
            const actions=await vscode.commands.executeCommand('vscode.executeCodeActionProvider',uri,issue.range);
            const action=actions.find(f=>f.title===`Declare ${cap} in mod.toml`);
            assert(action?.edit,`Missing random capability quick fix: ${cap}`);
            await vscode.workspace.applyEdit(action.edit);
            await extension.exports.refresh();
            assert(!vscode.languages.getDiagnostics(uri).some(d=>d.code===`capability:${cap}`));
        }
        passed.push('PASS: both random stream capabilities have independent working manifest quick fixes');

        const schemaUri = vscode.Uri.joinPath(folder.uri, 'scripts', 'schema-test.lua');
        fs.writeFileSync(schemaUri.fsPath, 'local sf2=require("sf2")\nsf2.behaviors.register { id="hits", state={fields={hits={type="integer",default=0}}}, on_damage_dealt=function(self, fighter, event)\n local value=self.state.\nend }');
        await vscode.workspace.openTextDocument(schemaUri);
        const keys = await vscode.commands.executeCommand('vscode.executeCompletionItemProvider', schemaUri, new vscode.Position(2, 24));
        assert(keys.items.some(i => i.label === 'hits'), 'Declared state key completion missing');
        passed.push('PASS: state schema keys complete inside inline callbacks');

        const luarcUri = vscode.Uri.joinPath(folder.uri, '.luarc.json');
        if (!fs.existsSync(luarcUri.fsPath)) fs.writeFileSync(luarcUri.fsPath, '{}');
        const luarcDoc = await vscode.workspace.openTextDocument(luarcUri);
        const luarcEdit = new vscode.WorkspaceEdit();
        luarcEdit.replace(luarcUri, new vscode.Range(luarcDoc.positionAt(0), luarcDoc.positionAt(luarcDoc.getText().length)), '// Keep this comment\n{ "workspace": { "library": ["other-library"] }, "runtime": { "version": "Lua 5.2" } }\n');
        await vscode.workspace.applyEdit(luarcEdit);
        await luarcDoc.save();
        await vscode.commands.executeCommand('eclipseModding.enable');
        const luarc = fs.readFileSync(path.join(folder.uri.fsPath, '.luarc.json'), 'utf8');
        assert(luarc.includes('// Keep this comment') && luarc.includes('other-library') && luarc.includes('library'));
        assert(require('jsonc-parser').parse(luarc).workspace.library.includes(library));
        passed.push('PASS: .luarc.json settings and comments are preserved');

        await vscode.commands.executeCommand('eclipseModding.disable');
        assert.deepEqual(vscode.workspace.getConfiguration('Lua', folder.uri).get('workspace.library'), [otherLibrary]);
        assert.deepEqual(require('jsonc-parser').parse(fs.readFileSync(path.join(folder.uri.fsPath, '.luarc.json'), 'utf8')).workspace.library, ['other-library']);
        passed.push('PASS: disable removes only the preview library');
        fs.writeFileSync(report, passed.join('\n') + '\n');
        console.log(passed.join('\n'));
    } catch (error) {
        fs.writeFileSync(report, passed.join('\n') + '\nFAIL: ' + error.stack);
        throw error;
    }
};
